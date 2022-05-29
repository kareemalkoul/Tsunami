using UnityEngine;

namespace Kareem.Fluid.SPH
{
    struct FluidParticleDensity
    {
        public float Density;
    };

    struct FluidParticlePressure
    {
        float pressure;
    };

    struct FluidParticleForces
    {
        public Vector2 Acceleration;
    };

    struct FluidParticleForces3D
    {
        public Vector3 Acceleration;
    };
}
