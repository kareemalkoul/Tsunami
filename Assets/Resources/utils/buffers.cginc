
// Buffer to hold particle position and velocity
RWStructuredBuffer<Particle> _ParticlesBuffer;

// Buffer to hold particle density
RWStructuredBuffer<ParticleDensity> _ParticlesDensityBuffer;

// Buffer to hold particle pressure
RWStructuredBuffer<ParticlePressure> _ParticlesPressureBuffer;

// Buffer that holds particle acceleration (force)
RWStructuredBuffer<ParticleForces>  _ParticlesForceBuffer;



RWStructuredBuffer<int> _neighbourList; // Stores all neighbours of a particle aligned at 'particleIndex * maximumParticlesPerCell * 8'
RWStructuredBuffer<int> _neighbourTracker; // How many neighbours does each particle contain.
RWStructuredBuffer<uint> _hashGrid; // aligned at 'particleIndex * maximumParticlesPerCell * 8' + _hashGridTracker[particleIndex]
RWStructuredBuffer<uint> _hashGridTracker;   // How many particles at each cell.



// ▲ Structured buffer definition ---------------------
