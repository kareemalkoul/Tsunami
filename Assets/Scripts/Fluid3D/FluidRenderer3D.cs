using UnityEngine;
using System.Collections;
using UnityEngine.Profiling;
using System.Collections.Generic;

namespace Kareem.Fluid.SPH
{
    [RequireComponent(typeof(Fluid3D))]
    public class FluidRenderer3D : MonoBehaviour
    {
        public List<LineRenderer> Lines;
        private CustomSampler Rendering;
        private CustomSampler GetDate;

        public Fluid3D solver;
        public Material RenderParticleMat;
        public Color WaterColor;
        public bool IsRenderInShader = true;
        public bool IsBoundsDrawed = true;

        void Start()
        {
            Rendering = CustomSampler.Create("kareem/rendering");
            GetDate = CustomSampler.Create("kareem/GetData");
            if (IsBoundsDrawed)
            {
                Lines = new List<LineRenderer>();
                DrawBounds();
            }
            DrawPlanes();
            // GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            // plane.transform.position
        }

        void OnRenderObject()
        {
            Rendering.Begin();
            DrawParticle();
            Rendering.End();
        }

        void DrawParticle()
        {
            float radius = 0.1f;
            RenderParticleMat.SetPass(0);
            RenderParticleMat.SetColor("_WaterColor", WaterColor);
            RenderParticleMat.SetBuffer("_ParticlesBuffer", solver.ParticlesBuffer);
            // RenderParticleMat.SetFloat("_ParticleRadius", solver.BallRadius);


            if (IsRenderInShader)
            {
                Graphics.DrawProceduralNow(MeshTopology.Points, solver.NumParticles);
            }
            else
            {
                DrawByExtension(radius);
            }
        }

        void DrawByExtension(float radius)
        {
            GetDate.Begin();
            FluidParticle3D[] particles = new FluidParticle3D[solver.NumParticles];
            solver.ParticlesBuffer.GetData(particles);
            GetDate.End();
            foreach (var item in particles)
            {
                // Debug.Log(item.Position);
                DebugExtension.DebugWireSphere(item.Position, WaterColor, radius, 0, false);
            }
        }

        void DrawBounds()
        {
            // Debug.DrawLine(Start, Start + EndX, colorBoundry);
            // Debug.DrawLine(Start, Start + EndY, colorBoundry);
            // Debug.DrawLine(Start, Start + EndZ, colorBoundry);

            // Debug.DrawLine(Start + EndXYZ, Start + EndXZ, colorBoundry);
            // Debug.DrawLine(Start + EndXYZ, Start + EndXY, colorBoundry);
            // Debug.DrawLine(Start + EndXYZ, Start + EndYZ, colorBoundry);

            // Debug.DrawLine(Start + EndXY, Start + EndX, colorBoundry);
            // Debug.DrawLine(Start + EndXY, Start + EndY, colorBoundry);

            // Debug.DrawLine(Start + EndXZ, Start + EndX, colorBoundry);
            // Debug.DrawLine(Start + EndXZ, Start + EndZ, colorBoundry);
            // Debug.DrawLine(Start + EndYZ, Start + EndY, colorBoundry);
            // Debug.DrawLine(Start + EndYZ, Start + EndZ, colorBoundry);
            DrawLines();
        }

        void DrawLines()
        {
            Vector3 offset = Vector3.zero;
            Vector3 range = solver.Range;

            Vector3 Start = new Vector3(offset.x, offset.y, offset.z);

            Vector3 EndX = new Vector3(range.x, 0, 0);
            Vector3 EndY = new Vector3(0, range.y, 0);
            Vector3 EndZ = new Vector3(0, 0, range.z);

            Vector3 EndXZ = EndX + EndZ;
            Vector3 EndXY = EndX + EndY;
            Vector3 EndYZ = EndZ + EndY;

            Vector3 EndXYZ = EndX + EndY + EndZ;

            Color colorBoundry = Color.blue;
            DrawLine(Start, Start + EndX, colorBoundry);
            DrawLine(Start, Start + EndY, colorBoundry);
            DrawLine(Start, Start + EndZ, colorBoundry);

            DrawLine(Start + EndXYZ, Start + EndXZ, colorBoundry);
            DrawLine(Start + EndXYZ, Start + EndXY, colorBoundry);
            DrawLine(Start + EndXYZ, Start + EndYZ, colorBoundry);

            DrawLine(Start + EndXY, Start + EndX, colorBoundry);
            DrawLine(Start + EndXY, Start + EndY, colorBoundry);

            DrawLine(Start + EndXZ, Start + EndX, colorBoundry);
            DrawLine(Start + EndXZ, Start + EndZ, colorBoundry);
            DrawLine(Start + EndYZ, Start + EndY, colorBoundry);
            DrawLine(Start + EndYZ, Start + EndZ, colorBoundry);
        }

        void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
        {
            GameObject myLine = new GameObject("Lines");
            myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.numCornerVertices = 10;
            // lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.SetColors(color, color);
            lr.SetWidth(0.1f, 0.1f);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            Lines.Add(lr);
            myLine.transform.parent = transform;
            // GameObject.Destroy(myLine, duration);
        }

        public void DrawPlanes()
        {
            Vector3 offset = Vector3.zero;
            Vector3 range = solver.Range;

            Vector3 Start = new Vector3(offset.x, offset.y, offset.z);

            Vector3 EndX = new Vector3(range.x, 0, 0);
            Vector3 EndY = new Vector3(0, range.y, 0);
            Vector3 EndZ = new Vector3(0, 0, range.z);

            Vector3 EndXZ = EndX + EndZ;
            Vector3 EndXY = EndX + EndY;
            Vector3 EndYZ = EndZ + EndY;

            Vector3 EndXYZ = EndX + EndY + EndZ;

            DrawPlane(Start, EndZ, EndXZ, EndX);
            DrawPlane(Start, EndY, EndYZ, EndZ);
            DrawPlane(EndZ, EndYZ, EndXYZ, EndXZ);
        }

        void DrawPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            GameObject plane = new GameObject("Plane" + p1.ToString());
            plane.transform.parent = transform;
            MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

            MeshFilter meshFilter = plane.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[4] { p1, p2, p3, p4 };
            mesh.vertices = vertices;

            int[] tris = new int[6]
            {
                // lower left triangle
                0,
                1,
                2,
                // upper right triangle
                2,
                3,
                0
            };
            mesh.triangles = tris;

            Vector3[] normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            mesh.normals = normals;
            meshFilter.mesh = mesh;
        }
    }
}
