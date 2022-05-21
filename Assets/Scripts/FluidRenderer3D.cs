using UnityEngine;
using System.Collections;

namespace Kodai.Fluid.SPH {

    [RequireComponent(typeof(Fluid3D))]
    public class FluidRenderer3D : MonoBehaviour {

        public Fluid3D solver;
        public Material RenderParticleMat;
        public Color WaterColor;

        void OnRenderObject() {
            DrawParticle();
        }

        void DrawParticle() {
            // float radius=0.1f;
            RenderParticleMat.SetPass(0);
            RenderParticleMat.SetColor("_WaterColor", WaterColor);
            RenderParticleMat.SetBuffer("_ParticlesBuffer", solver.ParticlesBufferRead);
            FluidParticle3D[] particles=new FluidParticle3D[solver.NumParticles];
            solver.ParticlesBufferRead.GetData(particles);
            // foreach (var item in particles)
            // {
            //     // Debug.Log(item.Position);
            //     DebugExtension.DebugWireSphere(item.Position,WaterColor,radius,0,false);
            // }
           Graphics.DrawProceduralNow(MeshTopology.Points, solver.NumParticles);
            
        }
    }
}