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
// ▲ Structured buffer definition ---------------------
