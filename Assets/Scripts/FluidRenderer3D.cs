using UnityEngine;
using System.Collections;

namespace Kodai.Fluid.SPH {

    [RequireComponent(typeof(Fluid3D))]
    public class FluidRenderer3D : MonoBehaviour {

        public Fluid3D solver;
        public Material RenderParticleMat;
        public Color WaterColor;

        void OnRenderObject() {
            Debug.Log("asdas");
            DrawParticle();
        }

        void DrawParticle() {

            RenderParticleMat.SetPass(0);
            RenderParticleMat.SetColor("_WaterColor", WaterColor);
            RenderParticleMat.SetBuffer("_ParticlesBuffer", solver.ParticlesBufferRead);
            Graphics.DrawProceduralNow(MeshTopology.Points, solver.NumParticles);
        }
    }
}