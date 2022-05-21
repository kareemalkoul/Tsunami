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

        private bool isMouseDown;
        private Vector3 screenToWorldPointPos;

        /// <summary>
        /// Particle initial position setting
        /// </summary>
        /// <param name="particles"></param>
        protected override void InitParticleData(ref FluidParticle3D[] particles)
        {
            // float origin =
            for (int i = 0; i < NumParticles; i++)
            {
                particles[i].Velocity = Vector3.zero;
                particles[i].Position = range / 2f + Random.insideUnitSphere * ballRadius;  // Initialize particles into a sphere
            }
            // float space = range.x * range.y * range.z;
            // float nums = NumParticles / space;

            // int sideCountX = (int)(range.x * nums);
            // int sideCountY = (int)(range.y * nums);
            // int sideCountZ = (int)(range.z * nums);
            // int count = 0;
            // Debug.Log(nums);
            // Debug.Log(sideCountX);
            // Debug.Log(sideCountY);
            // Debug.Log(sideCountZ);

            // for (int i = 0; i < sideCountX; i++)
            // {
            //     if (count >= NumParticles) break;
            //     for (int j = 0; j < sideCountY; j++)
            //     {
            //         if (count >= NumParticles) break;
            //         for (int k = 0; k < sideCountZ; k++)
            //         {
            //             if (count >= NumParticles)
            //             {
            //                 Debug.Log(NumParticles);
            //                 Debug.Log(count);
            //                 break;
            //             }

            //             float x = i / ballRadius, y = j / ballRadius, z = k / ballRadius;

            //             particles[count].Position = new Vector3(x, y, z);
            //             // Vector3 pos = origin + Vector3.right * i * delta + Vector3.forward * j * delta + Vector3.up * k * delta;
            //             // GameObject o = Instantiate(particlePrefab, pos, particlePrefab.transform.rotation, this.transform);
            //             // o.name="Particle "+i+" "+j+" "+k;
            //             // Particle p = o.GetComponent<Particle>();
            //             // p.position = pos;
            //             particles[count].Velocity = Vector3.zero;
            //             Debug.Log(count + " " + particles[count].Position);
            //             count++;

            //             // l.Add(p);

            //         }
            //     }
            // }
            // Debug.Log(count);

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