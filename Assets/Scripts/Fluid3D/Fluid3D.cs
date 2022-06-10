using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Kareem.Fluid.SPH
{
    public struct FluidParticle3D
    {
        public Vector3 Position;
        public Vector3 Velocity;
    };

    public class Fluid3D : FluidBase3D<FluidParticle3D>
    {
        [HideInInspector]
        private List<GameObject> ParticlesGameObjec;

        [SerializeField]
        private float MouseInteractionRadius = 1f; // Wide range of mouse interactions

        [SerializeField]
        private bool createGameObject = false;

        [SerializeField]
        public InitParticleWay initParticleWay = InitParticleWay.SPHERE;

        private bool isMouseDown;
        private Vector3 screenToWorldPointPos;

        ///
        ///Hide fields
        [HideInInspector, SerializeField]
        private float ballRadius = 0.1f; // Circular radius at particle position initialization

        [HideInInspector, SerializeField]
        public float separationFactor = 1.4f;

        [HideInInspector, SerializeField]
        private float volume = 1;

        [HideInInspector, SerializeField]
        private float particleRadius = 0.15f; // Radius for a particle

        ///finish Hide Fields
        ///



        /// <summary>
        /// Particle initial position setting
        /// </summary>
        /// <param name="particles"></param>
        protected override void InitParticleData(ref FluidParticle3D[] particles)
        {
            switch (initParticleWay)
            {
                case InitParticleWay.SPHERE:
                    initSphereMethod(ref particles);
                    break;
                case InitParticleWay.CUBE:
                    initCubeMethod(ref particles);
                    break;
            }

            if (createGameObject)
                CreateGameObjectForParticles(ref particles);
        }

        void CreateGameObjectForParticles(ref FluidParticle3D[] particles)
        {
            if (ParticlesGameObjec == null)
                ParticlesGameObjec = new List<GameObject>();
            for (int i = 0; i < NumParticles; i++)
            {
                GameObject particle = new GameObject();
                particle.name = "Particle " + i;
                particle.transform.position = particles[i].Position;
                particle.transform.parent = gameObject.transform;
                ParticlesGameObjec.Add(particle);
            }
        }

        void initSphereMethod(ref FluidParticle3D[] particles)
        {
            for (int i = 0; i < NumParticles; i++)
            {
                particles[i].Velocity = Vector3.zero;
                particles[i].Position = range / 2f + Random.insideUnitSphere * ballRadius; // Initialize particles into a sphere
            }
        }

        void initCubeMethod(ref FluidParticle3D[] particles)
        {
            float particleScale = 2 * particleRadius;
            volume = Mathf.Pow(particleScale, 3) * NumParticles * separationFactor;

            float sideLength = Mathf.Pow(volume, 1f / 3f);
            int sideCount = (int)Mathf.Pow(NumParticles, 1f / 3f);
            int extra = NumParticles - (int)Mathf.Pow(sideCount, 3);

            float delta = (float)sideLength / (float)sideCount;

            Vector3 origin = new Vector3(0, 0, 0); //new Vector3(-sideLength/2, 1.0f, -sideLength / 2);
            int n = 0;
            for (int i = 0; i < sideCount; i++)
            {
                for (int j = 0; j < sideCount; j++)
                {
                    for (int k = 0; k < sideCount; k++)
                    {
                        Vector3 pos =
                            origin
                            + Vector3.right * i * delta
                            + Vector3.forward * j * delta
                            + Vector3.up * k * delta;
                        if (n >= NumParticles)
                            return;
                        particles[n].Position = pos;
                        // particles[n].Position += Random.insideUnitSphere * 0.05f;
                        particles[n].Velocity = Vector3.zero;
                        n++;
                    }
                }
            }
        }

        /// <summary>
        /// Add to the constant buffer of ComputeShader
        /// </summary>
        /// <param name="cs"></param>
        protected override void AdditionalCSParams(ComputeShader cs)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isMouseDown = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isMouseDown = false;
            }

            if (isMouseDown)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                screenToWorldPointPos = Camera.main.ScreenToWorldPoint(mousePos);
                Debug.Log(0);
            }

            cs.SetVector("_MousePos", screenToWorldPointPos);
            cs.SetFloat("_MouseRadius", MouseInteractionRadius);
            cs.SetBool("_MouseDown", isMouseDown);
        }

        public void RestartSimulation()
        {
            if (ParticlesGameObjec != null)
                DestroyGameObjects();

            DeleteBuffers();
            Init();
            InitBuffers();
        }

        private void DestroyGameObjects()
        {
            foreach (GameObject particle in ParticlesGameObjec)
            {
                Destroy(particle);
            }
            ParticlesGameObjec.Clear();
        }

        public void OnGUI()
        {
            GUI.Box(new Rect(0, 0, 220, 420), "");

            GUI.Label(new Rect(11, 0, 80, 20), "25");

            if (GUI.Button(new Rect(10, 20, 200, 20), "Clear Particles")) { }
            if (Input.GetKeyDown(KeyCode.R))
            {
                 Application.LoadLevel(Application.loadedLevel);

            }

            // Constants.mRadialViscosityGain = GUI.HorizontalSlider(
            //     new Rect(10, 60, 200, 20),
            //     Constants.mRadialViscosityGain,
            //     0,
            //     10f
            // );
            // GUI.Label(
            //     new Rect(10, 40, 200, 20),
            //     "Radial Vicousity Gain: " + Constants.mRadialViscosityGain
            // );

            // particleSystem.MaxParticles = (int)
            //     GUI.HorizontalSlider(
            //         new Rect(10, 100, 200, 20),
            //         particleSystem.MaxParticles,
            //         0,
            //         Constants.MAX_PARTICLES
            //     );

            // int pcount = particleSystem.Particles.Count;
            // int pmax = particleSystem.MaxParticles;
            // GUI.Label(new Rect(10, 80, 200, 20), pcount + " of " + pmax + " Particles");

            // if (pcount > pmax)
            // {
            //     for (int i = pcount - 1; i >= pmax; i--)
            //     {
            //         particleSystem.Particles.RemoveAt(i);
            //     }

            // }

            // float pmass = Constants.ParticleMass;
            // pmass = GUI.HorizontalSlider(new Rect(10, 140, 200, 20), pmass, 0.001f, 50);
            // if (Constants.ParticleMass != pmass)
            // {
            //     Constants.ParticleMass = pmass;
            //     lGravity = Constants.Gravity * Constants.ParticleMass;
            //     ((ParticleEmitter)particleSystem.Emitters[0]).ParticleMass = Constants.ParticleMass;
            //     foreach (mParticle particle in particleSystem.Particles)
            //     {
            //         particle.Mass = Constants.ParticleMass;
            //     }
            // }
            // GUI.Label(new Rect(10, 120, 200, 20), "Particle Mass: " + pmass);

            // collisionSolver.Bounciness = GUI.HorizontalSlider(
            //     new Rect(10, 180, 200, 20),
            //     collisionSolver.Bounciness,
            //     0,
            //     10
            // );
            // GUI.Label(new Rect(10, 160, 200, 20), "Bounciness: " + collisionSolver.Bounciness);

            // float damp = GUI.HorizontalSlider(
            //     new Rect(10, 220, 200, 20),
            //     Constants.ParticleDamping,
            //     0,
            //     0.1f
            // );
            // GUI.Label(new Rect(10, 200, 200, 20), "Damping: " + Constants.ParticleDamping);
            // if (Constants.ParticleDamping != damp)
            // {
            //     foreach (mParticle particle in particleSystem.Particles)
            //     {
            //         particle.Solver.Damping = damp;
            //     }
            //     Constants.ParticleDamping = damp;
            // }

            // float gravity = Constants.Gravity.y * -1;
            // gravity = GUI.HorizontalSlider(new Rect(10, 260, 200, 20), gravity, 0, 20);
            // GUI.Label(new Rect(10, 240, 200, 20), "Gravity: " + gravity);
            // Constants.Gravity = new Vector2(0, gravity * -1);
            // lGravity = Constants.Gravity * Constants.ParticleMass;

            // Constants.GasConstant = GUI.HorizontalSlider(
            //     new Rect(10, 300, 200, 20),
            //     Constants.GasConstant,
            //     0,
            //     10
            // );
            // GUI.Label(new Rect(10, 280, 200, 20), "GasConstant: " + Constants.GasConstant);

            // Constants.Friction = GUI.HorizontalSlider(
            //     new Rect(10, 340, 200, 20),
            //     Constants.Friction,
            //     0,
            //     0.5f
            // );
            // GUI.Label(new Rect(10, 320, 200, 20), "Dissipitation: " + Constants.Friction);

            // GUI.Label(new Rect(10, 360, 200, 20), "Fill Type");
            // fillTypeIndex = GUI.SelectionGrid(
            //     new Rect(10, 380, 200, 20),
            //     fillTypeIndex,
            //     fillTypeNames,
            //     3
            // );
        }

        public float BallRadius
        {
            get { return ballRadius; }
            set { ballRadius = value; }
        }

        public float SeparationFactor
        {
            get { return separationFactor; }
            set { separationFactor = value; }
        }
        public float ParticleRadius
        {
            get { return particleRadius; }
            set { particleRadius = value; }
        }

        public float Volume
        {
            get { return volume; }
            set { volume = value; }
        }
    }
}
