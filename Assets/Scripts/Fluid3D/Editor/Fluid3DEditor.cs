using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Kareem.Fluid.SPH
{

    //this class for make custom filed in monobehavoir
    [CustomEditor(typeof(Fluid3D), true)]
    public class Fluid3DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //this method for create default monoBehavior fields
            //that i make it in base class (Fluid3D)
            DrawDefaultInspector();

            EditorUtility.SetDirty(target);
            Fluid3D fluid3D = (Fluid3D)target;

            switch (fluid3D.initParticleWay)
            {
                case InitParticleWay.SPHERE:
                    Sphere(ref fluid3D);
                    break;
                case InitParticleWay.CUBE:
                    Cube(ref fluid3D);
                    break;
            }

            if (GUILayout.Button("Restart Simulation"))
            {
                fluid3D.RestartSimulation();
            }
        }

        void Sphere(ref Fluid3D fluid3D)
        {
            fluid3D.BallRadius = EditorGUILayout.FloatField("Sphere Radius", fluid3D.BallRadius);
        }

        void Cube(ref Fluid3D fluid3D)
        {
            fluid3D.ParticleRadius = EditorGUILayout.FloatField(
                "Particle Radius",
                fluid3D.ParticleRadius
            );
            fluid3D.separationFactor = EditorGUILayout.FloatField(
                "Sepreation Factor",
                fluid3D.separationFactor
            );
            EditorGUILayout.LabelField("Volume", fluid3D.Volume.ToString());
        }
    }
}
