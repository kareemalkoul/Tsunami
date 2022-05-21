using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Kodai.Fluid.SPH
{


    struct FluidParticleForces3D
    {
        public Vector3 Acceleration;
    };

    public abstract class FluidBase3D<T> : MonoBehaviour where T : struct
    {

        [SerializeField] protected NumParticleEnum particleNum = NumParticleEnum.NUM_8K;    // Number of particles
        [SerializeField] protected float smoothlen = 0.012f;                                // Particle radius
        [SerializeField] private float pressureStiffness = 200.0f;                          // Pressure term coefficient
        [SerializeField] protected float restDensity = 1000.0f;                             // resting density
        [SerializeField] protected float particleMass = 0.0002f;                            // particle mass
        [SerializeField] protected float viscosity = 0.1f;                                  // Viscosity coefficient
        [SerializeField] protected float maxAllowableTimestep = 0.005f;                     // Time step width
        [SerializeField] protected float wallStiffness = 3000.0f;                           // The power of the wall of penalty law
        [SerializeField] protected int iterations = 4;                                      // Number of iterations per frame of simulation
        [SerializeField] protected Vector3 gravity = new Vector3(0.0f, -0.5f, 0.0f);        // gravity
        [SerializeField] protected Vector3 range = new Vector3(1, 1, 1);                    // Simulation space
        [SerializeField] protected bool simulate = true;                                    // Simulation execution or stop for a while
        [SerializeField] protected float tensionThreshold = 0.7f;
        [SerializeField] protected float tensionCoefficient = 0.0728f;


        private int numParticles;                                                           // Number of particles
        private float timeStep;                                                             // Time step width
        private float densityCoef;                                                          // Poly6 kernel density coefficient 
        private float gradPressureCoef;                                                     // Pressure coefficient of Spiky kernel
        private float lapViscosityCoef;                                                     // Viscosity coefficient of Laplacian kernel  
        private float lapTensionCoef;                                                       // Tension coefficient of Laplacian kernel
        private float gradTensionCoef;                                                      // Tension coefficient of grad kernel
        private bool oddStep;

        #region DirectCompute
        [SerializeField] ComputeShader fluidCS;
        private static readonly int THREAD_SIZE_X = 1024;                                   // Number of threads on the compute shader side
        private ComputeBuffer particlesBufferRead;                                          // Buffer to hold particle data
        private ComputeBuffer particlesBufferWrite;                                         // Buffer to write particle data
        private ComputeBuffer particlesPressureBuffer;                                      // A buffer that holds particle pressure dataA buffer that holds particle pressure data
        private ComputeBuffer particleDensitiesBuffer;                                      // Buffer that holds particle density data
        private ComputeBuffer particleForcesBuffer;                                         // Buffer that holds particle acceleration data
        private ComputeBuffer particleForcesOldBufferRead;                                       // Buffer that holds particle old acceleration data
        private ComputeBuffer particleForcesOldBufferWrite;                                       // Buffer that holds particle old acceleration data
        private ComputeBuffer debugBuffer;                                                  // Debug buffer

        #endregion

        #region Accessor
        public int NumParticles
        {
            get { return numParticles; }
        }

        public Vector3 Range
        {
            get { return range; }
        }

        public ComputeBuffer ParticlesBufferRead
        {
            get { return particlesBufferRead; }
        }

        #endregion

        #region Mono
        protected virtual void Awake()
        {
            if (fluidCS == null)
                fluidCS = (ComputeShader)Resources.Load("SPH3D");
            numParticles = (int)particleNum;
        }

        protected virtual void Start()
        {
            InitBuffers();
        }

        private void Update()
        {

            //this condition for stop simulation for a time
            if (!simulate)
            {
                return;
            }

            //if maxAllowableTimestep is more than the delta time between curr frame and prev frame 
            //then make time step is delta time 
            //this maintaple happen when the maxAllowableTimestep is big than deltaTime
            // cause like move 2 second in compusting and is in 1 seconds in showing 
            timeStep = Mathf.Min(maxAllowableTimestep, Time.deltaTime);

            // 3D kernel coefficient
            //valvulate constant with eachgather
            densityCoef = particleMass * 315f / (64f * Mathf.PI * Mathf.Pow(smoothlen, 9));             // Poly6 for 3D
            gradPressureCoef = particleMass * -45.0f / (Mathf.PI * Mathf.Pow(smoothlen, 6));            // Spiky for 3D
            lapViscosityCoef = particleMass * 45f / (Mathf.PI * Mathf.Pow(smoothlen, 6));               // Viscosity for 3D
            gradTensionCoef = particleMass * -24 / (32 * Mathf.PI * Mathf.Pow(smoothlen, 9));           // Poly6 for 3D
            lapTensionCoef = particleMass * -945 / (32 * Mathf.PI * Mathf.Pow(smoothlen, 9));            // Poly6 for 3D
            oddStep = !oddStep;
            // Transfer of shader constants
            fluidCS.SetInt("_NumParticles", numParticles);
            fluidCS.SetFloat("_TimeStep", timeStep);
            fluidCS.SetFloat("_Smoothlen", smoothlen);
            fluidCS.SetFloat("_PressureStiffness", pressureStiffness);
            fluidCS.SetFloat("_GradTensionCoef", gradTensionCoef);
            fluidCS.SetFloat("_LapTensionCoef", lapTensionCoef);
            fluidCS.SetFloat("_tensionThreshold", tensionThreshold);
            fluidCS.SetFloat("_tensionCoefficient", tensionCoefficient);
            fluidCS.SetFloat("_RestDensity", restDensity);
            fluidCS.SetFloat("_Viscosity", viscosity);
            fluidCS.SetFloat("_DensityCoef", densityCoef);
            fluidCS.SetFloat("_GradPressureCoef", gradPressureCoef);
            fluidCS.SetFloat("_LapViscosityCoef", lapViscosityCoef);
            fluidCS.SetFloat("_WallStiffness", wallStiffness);
            fluidCS.SetVector("_Range", range);
            fluidCS.SetVector("_Gravity", gravity);
            fluidCS.SetBool("_oddStep",oddStep);

            AdditionalCSParams(fluidCS);

            // Repeat several times in smaller time steps to improve the accuracy of the calculation.
            for (int i = 0; i < iterations; i++)
            {
                RunFluidSolver();
            }
        }

        private void OnDestroy()
        {
            DeleteBuffer(debugBuffer);
            DeleteBuffer(particlesBufferRead);
            DeleteBuffer(particlesBufferWrite);
            DeleteBuffer(particlesPressureBuffer);
            DeleteBuffer(particleDensitiesBuffer);
            DeleteBuffer(particleForcesBuffer);
            DeleteBuffer(particleForcesOldBufferRead);
            DeleteBuffer(particleForcesOldBufferWrite);

        }

        #endregion Mono

        /// <summary>
        /// Fluid simulation main routine
        /// </summary>
        private void RunFluidSolver()
        {

            // TODO:why -1 ? is -1 for init the value?
            int kernelID = -1;
            // num of particle to each Thread
            //if 1K particles then 1 for each thread ,2 for 2k and 4 for 4k ,...etc.because the numParticles n*1.024 and the size_x is 1024
            int threadGroupsX = numParticles / THREAD_SIZE_X;

            // Density
            kernelID = fluidCS.FindKernel("DensityCS");
            fluidCS.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            fluidCS.SetBuffer(kernelID, "_ParticlesDensityBufferWrite", particleDensitiesBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            // Pressure
            kernelID = fluidCS.FindKernel("PressureCS");
            fluidCS.SetBuffer(kernelID, "_ParticlesDensityBufferRead", particleDensitiesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesPressureBufferWrite", particlesPressureBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            // Force
            kernelID = fluidCS.FindKernel("ForceCS");
            fluidCS.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            fluidCS.SetBuffer(kernelID, "_ParticlesDensityBufferRead", particleDensitiesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesPressureBufferRead", particlesPressureBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesForceBufferWrite", particleForcesBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            // Integrate
            kernelID = fluidCS.FindKernel("IntegrateCS");
            fluidCS.SetBuffer(kernelID, "_DebugBuffer", debugBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            fluidCS.SetBuffer(kernelID, "_ParticlesForceBufferRead", particleForcesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesBufferWrite", particlesBufferWrite);

            fluidCS.SetBuffer(kernelID, "_ParticlesForceOldBufferWrite", particleForcesOldBufferWrite);
            fluidCS.SetBuffer(kernelID, "_ParticlesForceOldBufferRead", particleForcesOldBufferRead);

            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            var result = new float[threadGroupsX];
            debugBuffer.GetData(result);
            foreach (var eachResult in result)
            {
                if (eachResult < 0) Debug.Log(eachResult);
            }

            SwapComputeBuffer(ref particlesBufferRead, ref particlesBufferWrite);   // Swapping buffers
            SwapComputeBuffer(ref particleForcesOldBufferWrite, ref particleForcesOldBufferRead);
        }

        /// <summary>
        /// Use this method if you want to add a transfer of shader constants in a child class
        /// </summary>
        /// <param name="shader"></param>
        protected virtual void AdditionalCSParams(ComputeShader shader) { }

        /// <summary>
        /// Particle initial position and initial velocity setting
        /// </summary>
        /// <param name="particles"></param>
        protected abstract void InitParticleData(ref T[] particles);

        /// <summary>
        /// Buffer initialization
        /// </summary>
        private void InitBuffers()
        {
            var particles = new T[numParticles];
            InitParticleData(ref particles);
            particlesBufferRead = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(T)));
            particlesBufferRead.SetData(particles);
            particles = null;   //TODO:why null?

            particlesBufferWrite = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(T)));
            particlesPressureBuffer = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidParticlePressure)));
            particleForcesBuffer = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidParticleForces3D)));
            particleDensitiesBuffer = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidParticleDensity)));
            debugBuffer = new ComputeBuffer(numParticles, sizeof(float));
            particleForcesOldBufferRead = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidParticleForces3D)));
            particleForcesOldBufferWrite = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidParticleForces3D)));


        }

        /// <summary>
        /// Swap the buffer specified in the argument
        /// </summary>
        private void SwapComputeBuffer(ref ComputeBuffer ping, ref ComputeBuffer pong)
        {
            ComputeBuffer temp = ping;
            ping = pong;
            pong = temp;
        }

        /// <summary>
        /// Free buffer
        /// </summary>
        /// <param name="buffer"></param>
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