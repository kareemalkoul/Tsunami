using UnityEngine;
using System.Runtime.InteropServices;

namespace Kareem.Fluid.SPH
{
    public struct FluidParticle3D
    {
        public Vector3 Position;
        public Vector3 Velocity;
    };

    public class Fluid3D : FluidBase3D<FluidParticle3D>
    {

        [SerializeField] private float ballRadius = 0.1f;           // Circular radius at particle position initialization
        [SerializeField] private float MouseInteractionRadius = 1f; // Wide range of mouse interactions


        [SerializeField] public float separationFactor = 1.4f;
        private float particleScale;

        [SerializeField] private float volume = 1;

        private bool isMouseDown;
        private Vector3 screenToWorldPointPos;

        /// <summary>
        /// Particle initial position setting
        /// </summary>
        /// <param name="particles"></param>
        protected override void InitParticleData(ref FluidParticle3D[] particles)
        {
            // initCubeMethod(ref particles);

            initSphereMethod(ref particles);

        }

        void initSphereMethod(ref FluidParticle3D[] particles)
        {
            for (int i = 0; i < NumParticles; i++)
            {
                particles[i].Velocity = Vector3.zero;
                particles[i].Position = range / 2f + Random.insideUnitSphere * ballRadius;  // Initialize particles into a sphere
            }
        }


        void initCubeMethod(ref FluidParticle3D[] particles)
        {
            particleScale = 2 * ballRadius;
            volume = Mathf.Pow(particleScale, 3) * NumParticles * separationFactor;

            Debug.Log("volume; " + volume);
            Debug.Log("count: " + NumParticles);

            float sideLength = Mathf.Pow(volume, 1f / 3f);
            int sideCount = (int)Mathf.Pow(NumParticles, 1f / 3f);
            int extra = NumParticles - (int)Mathf.Pow(sideCount, 3);

            Debug.Log("sideLength: " + sideLength);
            Debug.Log("sideCount: " + sideCount);
            Debug.Log("extra: " + extra);

            float delta = (float)sideLength /( (float)sideCount*150);

            Debug.Log("delta: " + delta);


            Vector3 origin = new Vector3(0, 0, 0);//new Vector3(-sideLength/2, 1.0f, -sideLength / 2);
            int n = 0;
            for (int i = 0; i < sideCount; i++)
            {
                for (int j = 0; j < sideCount; j++)
                {
                    for (int k = 0; k < sideCount; k++)
                    {
                        Vector3 pos = origin + Vector3.right * i * delta + Vector3.forward * j * delta + Vector3.up * k * delta;
                        // GameObject o = Instantiate(particlePrefab, pos, particlePrefab.transform.rotation, this.transform);
                        // Particle p = o.GetComponent<Particle>();
                        if (n >= NumParticles)
                            return;
                        Debug.Log("particle " + n + " " + pos);
                        particles[n].Position = pos;
                        // particles[n].Position += Random.insideUnitSphere * 0.05f;
                        particles[n].Velocity = Vector3.zero;
                        n++;
                    }
                }
            }
            Debug.Log("n: " + n);
        }

        public float BallRadius
        {
            get { return ballRadius; }
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

    }
}