// ▼ Structures definition 
struct Particle {
	float3 position;
	float3 velocity;
};

struct ParticlePressure {
	float pressure;
};

struct ParticleForces {
	float3 acceleration;
};

struct ParticleDensity {
	float density;
};

struct ColorField {
	float colorField;
	float colorLaplacian;
	float3 colorGradient;
};

// ▲ Structures definition 