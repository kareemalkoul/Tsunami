using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kareem.Fluid.SPH
{
    public abstract class FluidBase3D<T> : MonoBehaviour where T : struct
    {
        [SerializeField]
        protected NumParticleEnum particleNum = NumParticleEnum.NUM_8K; // Number of particles

        [SerializeField]
        protected float smoothlen = 0.012f; // Particle radius

        [SerializeField]
        private float pressureStiffness = 200.0f; // Pressure term coefficient

        [SerializeField]
        protected float restDensity = 1000.0f; // resting density

        [SerializeField]
        protected float particleMass = 0.0002f; // particle mass

        [SerializeField]
        protected float viscosity = 0.1f; // Viscosity coefficient

        [SerializeField]
        protected float maxAllowableTimestep = 0.005f; // Time step width

        [SerializeField]
        protected float wallStiffness = 3000.0f; // The power of the wall of penalty law

        [SerializeField]
        protected int iterations = 4; // Number of iterations per frame of simulation

        [SerializeField]
        protected Vector3 gravity = new Vector3(0.0f, -0.5f, 0.0f); // gravity

        [SerializeField]
        protected Vector3 range = new Vector3(1, 1, 1); // Simulation space

        [SerializeField]
        protected bool simulate = true; // Simulation execution or stop for a while

        [SerializeField]
        protected float tensionThreshold = 0.7f;

        [SerializeField, HideInInspector]
        protected float tensionCoefficient = 0.0728f;

        [SerializeField, Range(0.0f, 1.0f), HideInInspector]
        protected float Damping = 0.0728f;

        private int numParticles; // Number of particles
        private float timeStep; // Time step width
        private float densityCoef; // Poly6 kernel density coefficient
        private float gradPressureCoef; // Pressure coefficient of Spiky kernel
        private float lapViscosityCoef; // Viscosity coefficient of Laplacian kernel
        private float lapTensionCoef; // Tension coefficient of Laplacian kernel
        private float gradTensionCoef; // Tension coefficient of grad kernel
        private bool oddStep;
        
        [HideInInspector, SerializeField]
        protected float particleRadius = 0.15f; // Radius for a particle

        #region hashVars
        [SerializeField]
        public int dimensions = 200;

        [SerializeField]
        public int maximumParticlesPerCell = 500;

        #endregion
        #region DirectCompute
        [SerializeField]
        ComputeShader fluidCS;

        [SerializeField]
        ComputeShader hashCS;
        private static readonly int THREAD_SIZE_X = 1024; // Number of threads on the compute shader side
        private ComputeBuffer particlesBuffer; // Buffer to hold particle data
        private ComputeBuffer particlesPressureBuffer; // A buffer that holds particle pressure dataA buffer that holds particle pressure data
        private ComputeBuffer particleDensitiesBuffer; // Buffer that holds particle density data
        private ComputeBuffer particleForcesBuffer; // Buffer that holds particle acceleration data
        private ComputeBuffer NeighbourListBuffer;
        private ComputeBuffer NeighbourTrackerBuffer;
        private ComputeBuffer HashGridBuffer;
        private ComputeBuffer HashGridTrackerBuffer;
        #endregion
        #region Profiling
        private CustomSampler HashProfiling;
        private CustomSampler DesnsityProfiling;

        private CustomSampler PressureProfiling;
        private CustomSampler ForceProfiling;
        private CustomSampler IntegrateProfiling;

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

        public ComputeBuffer ParticlesBuffer
        {
            get { return particlesBuffer; }
        }

        #endregion

        #region Mono
        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void Start()
        {
            InitBuffers();
            InitProfilling();
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
            densityCoef = particleMass * 315f / (64f * Mathf.PI * Mathf.Pow(smoothlen, 9)); // Poly6 for 3D
            gradPressureCoef = particleMass * -45.0f / (Mathf.PI * Mathf.Pow(smoothlen, 6)); // Spiky for 3D
            lapViscosityCoef = particleMass * 45f / (Mathf.PI * Mathf.Pow(smoothlen, 6)); // Viscosity for 3D
            gradTensionCoef = particleMass * -24 / (32 * Mathf.PI * Mathf.Pow(smoothlen, 9)); // Poly6 for 3D
            lapTensionCoef = particleMass * -945 / (32 * Mathf.PI * Mathf.Pow(smoothlen, 9)); // Poly6 for 3D
            oddStep = !oddStep;
            // Transfer of shader constants

            setValues(fluidCS);
            setValues(hashCS);
            AdditionalCSParams(fluidCS);

            // Repeat several times in smaller time steps to improve the accuracy of the calculation.
            for (int i = 0; i < iterations; i++)
            {
                RunFluidSolver();
            }
        }

        private void setValues(ComputeShader shader)
        {
            shader.SetInt("_NumParticles", numParticles);
            shader.SetFloat("_TimeStep", timeStep);
            shader.SetFloat("_Smoothlen", smoothlen);
            shader.SetFloat("_PressureStiffness", pressureStiffness);
            shader.SetFloat("_GradTensionCoef", gradTensionCoef);
            shader.SetFloat("_LapTensionCoef", lapTensionCoef);
            shader.SetFloat("_tensionThreshold", tensionThreshold);
            shader.SetFloat("_tensionCoefficient", tensionCoefficient);
            shader.SetFloat("_RestDensity", restDensity);
            shader.SetFloat("_Viscosity", viscosity);
            shader.SetFloat("_DensityCoef", densityCoef);
            shader.SetFloat("_GradPressureCoef", gradPressureCoef);
            shader.SetFloat("_LapViscosityCoef", lapViscosityCoef);
            shader.SetFloat("_WallStiffness", wallStiffness);
            shader.SetVector("_Range", range);
            shader.SetVector("_Gravity", gravity);
            shader.SetFloat("_Damping", Damping);
            shader.SetBool("_oddStep", oddStep);

            shader.SetFloat("CellSize", smoothlen * 2);
            shader.SetInt("Dimensions", dimensions);
            shader.SetInt("maximumParticlesPerCell", maximumParticlesPerCell);
            shader.SetFloat("particleRadius",particleRadius);
        }

        protected void OnDestroy()
        {
            DeleteBuffers();
        }

        #endregion Mono

        /// <summary>
        /// Fluid simulation main routine
        /// </summary>
        void printIt(int []input){
            for(int i = 0 ; i<input.Length;i++){
                if(input[i] < maximumParticlesPerCell)
                Debug.Log(i+" "+ input[i]);
            }
        }
        private void RunFluidSolver()
        {
            int kernelID = -1;
            // num of particle to each Thread
            //if 1K particles then 1 for each thread ,2 for 2k and 4 for 4k ,...etc.because the numParticles n*1.024 and the size_x is 1024
            int threadGroupsX = numParticles / THREAD_SIZE_X;
            //init hash
            HashProfiling.Begin();
            kernelID = hashCS.FindKernel("ClearHashGrid");
            hashCS.SetBuffer(kernelID, "_hashGridTracker", HashGridTrackerBuffer);
            hashCS.Dispatch(kernelID, dimensions * dimensions * dimensions / THREAD_SIZE_X, 1, 1);

            kernelID = hashCS.FindKernel("RecalculateHashGrid");
            hashCS.SetBuffer(kernelID, "_ParticlesBuffer", particlesBuffer);
            hashCS.SetBuffer(kernelID, "_hashGrid", HashGridBuffer);
            hashCS.SetBuffer(kernelID, "_hashGridTracker", HashGridTrackerBuffer);
            hashCS.Dispatch(kernelID, threadGroupsX, 1, 1);

            kernelID = hashCS.FindKernel("BuildNeighbourList");
            hashCS.SetBuffer(kernelID, "_ParticlesBuffer", particlesBuffer);
            hashCS.SetBuffer(kernelID, "_hashGrid", HashGridBuffer);
            hashCS.SetBuffer(kernelID, "_hashGridTracker", HashGridTrackerBuffer);
            hashCS.SetBuffer(kernelID, "_neighbourList", NeighbourListBuffer);
            hashCS.SetBuffer(kernelID, "_neighbourTracker", NeighbourTrackerBuffer);
            hashCS.Dispatch(kernelID, threadGroupsX, 1, 1);
            HashProfiling.End();

            // Density
            DesnsityProfiling.Begin();
            kernelID = fluidCS.FindKernel("DensityCS");
            fluidCS.SetBuffer(kernelID, "_neighbourTracker", NeighbourTrackerBuffer);
            fluidCS.SetBuffer(kernelID, "_neighbourList", NeighbourListBuffer);

            fluidCS.SetBuffer(kernelID, "_ParticlesBuffer", particlesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesDensityBuffer", particleDensitiesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesPressureBuffer", particlesPressureBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);
            PressureProfiling.End();

            // Force
            ForceProfiling.Begin();
            kernelID = fluidCS.FindKernel("ForceCS");
            fluidCS.SetBuffer(kernelID, "_neighbourTracker", NeighbourTrackerBuffer);
            fluidCS.SetBuffer(kernelID, "_neighbourList", NeighbourListBuffer);

            fluidCS.SetBuffer(kernelID, "_ParticlesBuffer", particlesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesDensityBuffer", particleDensitiesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesPressureBuffer", particlesPressureBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesForceBuffer", particleForcesBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);
            ForceProfiling.End();

            // Integrate
            IntegrateProfiling.Begin();
            kernelID = fluidCS.FindKernel("IntegrateCS");
            fluidCS.SetBuffer(kernelID, "_ParticlesBuffer", particlesBuffer);
            fluidCS.SetBuffer(kernelID, "_ParticlesForceBuffer", particleForcesBuffer);
            fluidCS.Dispatch(kernelID, threadGroupsX, 1, 1);
           // Debug.Log("before print positions");
          //  printPositions();
          //  Debug.Log("after print positions");

        }

        private void printPositions(){
            FluidParticle3D[] particles = new FluidParticle3D[numParticles];
            particlesBuffer.GetData(particles);
            for(int i = 0 ; i < numParticles;i++){
                if(Double.IsNaN(particles[i].Position.x)){
                    Debug.Log("nan at "+i);
                }
            }
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

        protected void Init()
        {
            if (fluidCS == null)
                fluidCS = (ComputeShader)Resources.Load("SPH3D");
            if (hashCS == null)
                hashCS = (ComputeShader)Resources.Load("hash.compute");
            numParticles = (int)particleNum;
        }

        /// <summary>
        /// Buffer initialization
        /// </summary>
        protected void InitBuffers()
        {
            var particles = new T[numParticles];
            InitParticleData(ref particles);
            particlesBuffer = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(T)));
            particlesBuffer.SetData(particles);
            particles = null;

            particlesPressureBuffer = new ComputeBuffer(
                numParticles,
                Marshal.SizeOf(typeof(FluidParticlePressure))
            );
            particleForcesBuffer = new ComputeBuffer(
                numParticles,
                Marshal.SizeOf(typeof(FluidParticleForces3D))
            );
            particleDensitiesBuffer = new ComputeBuffer(
                numParticles,
                Marshal.SizeOf(typeof(FluidParticleDensity))
            );

            NeighbourListBuffer = new ComputeBuffer(NumParticles * maximumParticlesPerCell * 8, sizeof(int));

            NeighbourTrackerBuffer = new ComputeBuffer(NumParticles, sizeof(int));

            HashGridBuffer = new ComputeBuffer(dimensions * dimensions * dimensions * maximumParticlesPerCell, sizeof(uint));
            HashGridTrackerBuffer = new ComputeBuffer(dimensions * dimensions * dimensions, sizeof(uint));
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

        protected void DeleteBuffers()
        {
            DeleteBuffer(particlesBuffer);
            DeleteBuffer(particlesPressureBuffer);
            DeleteBuffer(particleDensitiesBuffer);
            DeleteBuffer(particleForcesBuffer);

            DeleteBuffer(NeighbourListBuffer);
            DeleteBuffer(NeighbourTrackerBuffer);
            DeleteBuffer(HashGridBuffer);
            DeleteBuffer(HashGridTrackerBuffer);
        }

        protected void InitProfilling()
        {
            HashProfiling = CustomSampler.Create("kareem/Hash");
            DesnsityProfiling = CustomSampler.Create("kareem/Desnsity");
            PressureProfiling = CustomSampler.Create("kareem/Pressure");
            ForceProfiling = CustomSampler.Create("kareem/Force");
            IntegrateProfiling = CustomSampler.Create("kareem/Integrate");
        }
    }
}
