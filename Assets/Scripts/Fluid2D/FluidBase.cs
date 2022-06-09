using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Kareem.Fluid.SPH
{
    public abstract class FluidBase<T> : MonoBehaviour where T : struct
    {
        [SerializeField]
        protected NumParticleEnum particleNum = NumParticleEnum.NUM_8K;

        [SerializeField]
        protected float smoothlen = 0.012f;

        [SerializeField]
        private float pressureStiffness = 200.0f;

        [SerializeField]
        protected float restDensity = 1000.0f;

        [SerializeField]
        protected float particleMass = 0.0002f;

        [SerializeField]
        protected float viscosity = 0.1f;

        [SerializeField]
        protected float maxAllowableTimestep = 0.005f;

        [SerializeField]
        protected float wallStiffness = 3000.0f;

        [SerializeField]
        protected int iterations = 4;

        [SerializeField]
        protected Vector2 gravity = new Vector2(0.0f, -0.5f);

        [SerializeField]
        protected Vector2 range = new Vector2(1, 1);

        [SerializeField]
        protected bool simulate = true;

        [SerializeField]
        protected float tensionThreshold = 0.7f;

        [SerializeField]
        protected float tensionCoefficient = 0.0728f;

        private int numParticles;
        private float timeStep;
        private float densityCoef;
        private float gradPressureCoef;
        private float lapViscosityCoef;
        private float lapTensionCoef;
        private float gradTensionCoef;

        #region DirectCompute
        [SerializeField]
        protected ComputeShader fluidCS;
        private static readonly int THREAD_SIZE_X = 1024;
        private ComputeBuffer particlesBufferRead;
        private ComputeBuffer particlesBufferWrite;
        private ComputeBuffer particlesPressureBuffer;
        private ComputeBuffer particleDensitiesBuffer;
        private ComputeBuffer particleForcesBuffer;
        #endregion

        #region Accessor
        public int NumParticles
        {
            get { return numParticles; }
        }

        public ComputeBuffer ParticlesBufferRead
        {
            get { return particlesBufferRead; }
        }
        public ComputeBuffer ParticleDensitiesBuffer
        {
            get { return particleDensitiesBuffer; }
        }
        public ComputeBuffer ParticlesPressureBuffer
        {
            get { return particlesPressureBuffer; }
        }
        #endregion

        #region Mono
        protected virtual void Awake()
        {
            numParticles = (int)particleNum;
        }

        protected virtual void Start()
        {
            if (fluidCS == null)
                fluidCS = (ComputeShader)Resources.Load("SPH2D");
            InitBuffers();
        }

        private void Update()
        {
            if (!simulate)
            {
                return;
            }

            timeStep = Mathf.Min(maxAllowableTimestep, Time.deltaTime);

            densityCoef = particleMass * 4f / (Mathf.PI * Mathf.Pow(smoothlen, 8));
            gradPressureCoef = particleMass * -30.0f / (Mathf.PI * Mathf.Pow(smoothlen, 5));
            lapViscosityCoef = particleMass * 20f / (3 * Mathf.PI * Mathf.Pow(smoothlen, 5));
            gradTensionCoef = particleMass * -24 / (Mathf.PI * Mathf.Pow(smoothlen, 8));
            lapTensionCoef = particleMass * -24 / (Mathf.PI * Mathf.Pow(smoothlen, 8));

            fluidCS.SetInt("_NumParticles", numParticles);
            fluidCS.SetFloat("_TimeStep", timeStep);
            fluidCS.SetFloat("_Smoothlen", smoothlen);
            fluidCS.SetFloat("_PressureStiffness", pressureStiffness);
            fluidCS.SetFloat("_RestDensity", restDensity);
            fluidCS.SetFloat("_Viscosity", viscosity);
            fluidCS.SetFloat("_GradTensionCoef", gradTensionCoef);
            fluidCS.SetFloat("_LapTensionCoef", lapTensionCoef);
            fluidCS.SetFloat("_tensionThreshold", tensionThreshold);
            fluidCS.SetFloat("_tensionCoefficient", tensionCoefficient);
            fluidCS.SetFloat("_DensityCoef", densityCoef);
            fluidCS.SetFloat("_GradPressureCoef", gradPressureCoef);
            fluidCS.SetFloat("_LapViscosityCoef", lapViscosityCoef);
            fluidCS.SetFloat("_WallStiffness", wallStiffness);
            fluidCS.SetVector("_Range", range);
            fluidCS.SetVector("_Gravity", gravity);

            AdditionalCSParams(fluidCS);

            for (int i = 0; i < iterations; i++)
            {
                RunFluidSolver();
            }
        }

        private void OnDestroy()
        {
            DeleteBuffer(particlesBufferRead);
            DeleteBuffer(particlesBufferWrite);
            DeleteBuffer(particlesPressureBuffer);
            DeleteBuffer(particleDensitiesBuffer);
            DeleteBuffer(particleForcesBuffer);
        }
        #endregion Mono




        private void RunFluidSolver()
        {
            int kernelID = -1;
            int threadGroupsX = numParticles / THREAD_SIZE_X;

            kernelID = fluidCS.FindKernel("DensityCS");
            fluidCS.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            fluidCS.SetBuffer(kernelID, "_ParticlesDensityBufferWrite", particleDensitiesBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            kernelID = fluidCS.FindKernel("PressureCS");
            fluidCS.SetBuffer(kernelID, "_ParticlesDensityBufferRead", particleDensitiesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesPressureBufferWrite", particlesPressureBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            kernelID = fluidCS.FindKernel("ForceCS");
            fluidCS.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            fluidCS.SetBuffer(kernelID, "_ParticlesDensityBufferRead", particleDensitiesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesPressureBufferRead", particlesPressureBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesForceBufferWrite", particleForcesBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            kernelID = fluidCS.FindKernel("IntegrateCS");
            fluidCS.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            fluidCS.SetBuffer(kernelID, "_ParticlesForceBufferRead", particleForcesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesBufferWrite", particlesBufferWrite);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            SwapComputeBuffer(ref particlesBufferRead, ref particlesBufferWrite);
        }

        protected virtual void AdditionalCSParams(ComputeShader shader) { }

        protected abstract void InitParticleData(ref T[] particles);

        private void InitBuffers()
        {
            particlesBufferRead = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(T)));
            var particles = new T[numParticles];
            InitParticleData(ref particles);
            particlesBufferRead.SetData(particles);
            particles = null;

            particlesBufferWrite = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(T)));
            particlesPressureBuffer = new ComputeBuffer(
                numParticles,
                Marshal.SizeOf(typeof(FluidParticlePressure))
            );
            particleForcesBuffer = new ComputeBuffer(
                numParticles,
                Marshal.SizeOf(typeof(FluidParticleForces))
            );
            particleDensitiesBuffer = new ComputeBuffer(
                numParticles,
                Marshal.SizeOf(typeof(FluidParticleDensity))
            );
        }

        private void SwapComputeBuffer(ref ComputeBuffer ping, ref ComputeBuffer pong)
        {
            ComputeBuffer temp = ping;
            ping = pong;
            pong = temp;
        }

        private void DeleteBuffer(ComputeBuffer buffer)
        {
            if (buffer != null)
            {
                buffer.Release();
                buffer = null;
            }
        }
    }
}
