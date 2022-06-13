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
        public GameObject Planes;
        public Fluid3D solver;
        public Material RenderParticleMat;

        // public Color WaterColor;
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
            ScalePlanes();
            DrawBuilding(8,3, 30, 20);
            DrawBuilding(16,12,30,20);
            // DrawPlanes();
            // GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            // plane.transform.position
        }

        void OnRenderObject()
        {
            Rendering.Begin();
            DrawParticle();
            changePostionLines();
            Rendering.End();
        }

        void ScalePlanes()
        {
            Planes.transform.localScale = solver.range * (1 / 20f);
        }

        void DrawParticle()
        {
            float radius = 0.1f;
            Color WaterColor = Color.blue;
            RenderParticleMat.SetPass(0);
            // RenderParticleMat.SetColor("_WaterColor", WaterColor);
            RenderParticleMat.SetBuffer("_ParticlesBuffer", solver.ParticlesBuffer);
            // RenderParticleMat.SetFloat("_ParticleRadius", solver.BallRadius);


            if (IsRenderInShader)
            {
                Graphics.DrawProceduralNow(MeshTopology.Points, solver.NumParticles);
            }
            else
            {
                DrawByExtension(radius, WaterColor);
            }
        }

        void DrawByExtension(float radius, Color WaterColor)
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
            DrawLine(Start, colorBoundry);
            DrawLine(Start, colorBoundry);
            DrawLine(Start, colorBoundry);

            DrawLine(Start + EndXY, colorBoundry);
            DrawLine(Start + EndXY, colorBoundry);
            DrawLine(Start + EndXY, colorBoundry);

            DrawLine(Start + EndXY, colorBoundry);
            DrawLine(Start + EndXY, colorBoundry);

            DrawLine(Start + EndXZ, colorBoundry);
            DrawLine(Start + EndXZ, colorBoundry);
            DrawLine(Start + EndYZ, colorBoundry);
            DrawLine(Start + EndYZ, colorBoundry);
        }

        void DrawLine(Vector3 pos, Color color)
        {
            GameObject myLine = new GameObject("Lines");
            myLine.transform.position = pos;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.numCornerVertices = 10;
            lr.SetColors(color, color);
            lr.SetWidth(0.1f, 0.1f);
            Lines.Add(lr);
            myLine.transform.parent = transform;
        }

        void SetLinePosition(Vector3 start, Vector3 end, LineRenderer lr)
        {
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        public void changePostionLines()
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

            SetLinePosition(Start, Start + EndX, Lines[0]);
            SetLinePosition(Start, Start + EndY, Lines[1]);
            SetLinePosition(Start, Start + EndZ, Lines[2]);

            SetLinePosition(Start + EndXYZ, Start + EndXZ, Lines[3]);
            SetLinePosition(Start + EndXYZ, Start + EndXY, Lines[4]);
            SetLinePosition(Start + EndXYZ, Start + EndYZ, Lines[5]);

            SetLinePosition(Start + EndXY, Start + EndX, Lines[6]);
            SetLinePosition(Start + EndXY, Start + EndY, Lines[7]);

            SetLinePosition(Start + EndXZ, Start + EndX, Lines[8]);
            SetLinePosition(Start + EndXZ, Start + EndZ, Lines[9]);
            SetLinePosition(Start + EndYZ, Start + EndY, Lines[10]);
            SetLinePosition(Start + EndYZ, Start + EndZ, Lines[11]);
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

        void DrawBuilding(float zmax, float zmin, float xmax, float xmin)
        {
            Vector3 range = solver.Range;

            Vector3 Start = new Vector3(xmin, 0, zmin);
            Vector3 End = new Vector3(xmax, range.y, zmax);

            Vector3 EndX = new Vector3(xmax, 0, zmin);
            Vector3 EndY = new Vector3(xmin, range.y, zmin);
            Vector3 EndZ = new Vector3(xmin, 0, zmax);

            Vector3 EndXZ = new Vector3(End.x, 0, zmax);
            Vector3 EndXY = new Vector3(xmax, range.y, zmin);
            Vector3 EndYZ =new Vector3(xmin, range.y, zmax);

            DrawPlane(Start,EndZ,EndYZ,EndY);
            DrawPlane(Start,EndY,EndXY,EndX);

            DrawPlane(EndZ,EndXZ,End,EndYZ);
            DrawPlane(EndX,EndXY,End,EndXZ);



           
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
