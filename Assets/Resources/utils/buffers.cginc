RWStructuredBuffer<float>			_DebugBuffer;

// Buffer to hold particle position and velocity
StructuredBuffer  <Particle>        _ParticlesBufferRead;
RWStructuredBuffer<Particle>        _ParticlesBufferWrite;

// Buffer to hold particle density
StructuredBuffer  <ParticleDensity> _ParticlesDensityBufferRead;
RWStructuredBuffer<ParticleDensity> _ParticlesDensityBufferWrite;

// Buffer to hold particle pressure
StructuredBuffer<ParticlePressure>	_ParticlesPressureBufferRead;
RWStructuredBuffer<ParticlePressure> _ParticlesPressureBufferWrite;

// Buffer that holds particle acceleration (force)
StructuredBuffer  <ParticleForces>  _ParticlesForceBufferRead;
RWStructuredBuffer<ParticleForces>  _ParticlesForceBufferWrite;

// Buffer that holds particle old acceleration (force)
StructuredBuffer  <ParticleForcesOld>  _ParticlesForceOldBufferRead;
RWStructuredBuffer<ParticleForcesOld>  _ParticlesForceOldBufferWrite;




RWStructuredBuffer<int> _neighbourList; // Stores all neighbours of a particle aligned at 'particleIndex * maximumParticlesPerCell * 8'
RWStructuredBuffer<int> _neighbourTracker; // How many neighbours does each particle contain.
RWStructuredBuffer<uint> _hashGrid; // aligned at 'particleIndex * maximumParticlesPerCell * 8' + _hashGridTracker[particleIndex]
RWStructuredBuffer<uint> _hashGridTracker;   // How many particles at each cell.



// ▲ Structured buffer definition ---------------------
