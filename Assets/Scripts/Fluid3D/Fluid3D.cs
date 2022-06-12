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
        [SerializeField]
        private float MouseInteractionRadius = 1f; // Wide range of mouse interactions

        [SerializeField]
        public InitParticleWay initParticleWay = InitParticleWay.CUBE;

        private bool isMouseDown;
        private Vector3 screenToWorldPointPos;

        ///
        ///Hide fields
        [HideInInspector, SerializeField]
        public float ballRadius = 0.1f; // Circular radius at particle position initialization

        [HideInInspector, SerializeField]
        public float separationFactor = 1.4f;

        [HideInInspector, SerializeField]
        private float volume = 1;

        [HideInInspector, SerializeField]
        public float moveForce = 0.08f;

        ///finish Hide Fields
        ///

        public void MoveWall()
        {
            // if (addedForce)
            // {
            //     float x = 0.08f;

            //     if (range.x <= rangeCompare.x - 2)
            //     {
            //         wave = false;
            //     }
            //     if (range.x >= rangeCompare.x)
            //     {
            //         //  range.x -= 0.03f;
            //         addedForce = false;
            //         wave = true;
            //     }
            //     if (range.x > rangeCompare.x - 2 && wave)
            //     {
            //         range.x -= x;
            //     }
            //     if (range.x < rangeCompare.x && !wave)
            //     {
            //         range.x += x;
            //     }
            // }
        }

        /// <summary>
        /// Particle initial position setting
        /// </summary>
        /// <param name="particles"></param>
        protected override void InitParticleData(ref FluidParticle3D[] particles)
        {
            switch (initParticleWay)
            {
                case InitParticleWay.TSUNAMI:
                    initSphereMethod(ref particles);
                    break;
                case InitParticleWay.CUBE:
                    initCubeMethod2(ref particles, range);
                    break;
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

            float sideLength = Mathf.Pow(volume, 1f / 5f);
            int sideCount = Mathf.CeilToInt(Mathf.Pow(NumParticles, 1f / 5f));
            int extra = NumParticles - (int)Mathf.Pow(sideCount, 5);

            Debug.Log("extra is: " + extra);
            float delta = (float)sideLength / (float)sideCount;
            delta /= 2f;
            float s = sideCount * sideCount;

            Vector3 origin = new Vector3(0, 0, 0); //new Vector3(-sideLength/2, 1.0f, -sideLength / 2);
            int n = 0;
            for (int i = 0; i < s; i++)
            {
                for (int j = 0; j < s; j++)
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
                        particles[n].Velocity = Vector3.zero;
                        n++;
                    }
                }
            }
        }

        void initCubeMethod2(ref FluidParticle3D[] particles, Vector3 range)
        {
            int x = 0;
            int y = 0;
            int z = 0;
            for (int i = 0; i < NumParticles; i++)
            {
                if (x * 2 * particleRadius > range.x)
                {
                    x = 0;
                    y = y + 1;
                    if (y * 2 * particleRadius > range.y)
                    {
                        y = 0;
                        z = (z + 1) % (int)range.z;
                    }
                }

                Vector3 pos = new Vector3(
                    x * 2 * particleRadius,
                    y * 2 * particleRadius,
                    z * 2 * particleRadius
                );
                particles[i].Position = pos;
                //    Debug.Log(pos.x+" "+pos.y+" "+pos.z);
                x++;
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
            DeleteBuffers();
            Init();
            InitBuffers();
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

        public NumParticleEnum ParticleNum
        {
            get { return particleNum; }
            set { particleNum = value; }
        }

        public float PressureStiffness
        {
            get { return pressureStiffness; }
            set { pressureStiffness = value; }
        }
    }
}
