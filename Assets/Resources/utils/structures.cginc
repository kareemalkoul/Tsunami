// ▼ Structured buffer definition ---------------------
struct Particle {
	float3 position;
	float3 velocity;
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
