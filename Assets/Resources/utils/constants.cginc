
// ▼ Definition of shader constants -----------------------
cbuffer CB {
	int   _NumParticles;		// Number of particles
	float _TimeStep;			// Time step width (dt)
	float _Smoothlen;			// Particle radius
	float _PressureStiffness;	// Becker's coefficient
	float _RestDensity;			// resting density
	float _DensityCoef;			// Coefficient when calculating density
	float _GradPressureCoef;	// Coefficient when calculating pressure
	float _LapViscosityCoef;	// Coefficient when calculating viscosity
	float _GradTensionCoef;		// Coefficient when calculating Tension
	float _LapTensionCoef;		// Coefficient when calculating Tension
	float _WallStiffness;		// The pushing force of the penalty method
	float _Viscosity;			// Viscosity coefficient
	float3 _Gravity;			// gravity
	float3 _Range;				// Simulation space
	float _tensionThreshold;	//Tension Threshold
	float _tensionCoefficient;	//Tension Coefficient

	float3 _MousePos;			// Mouse position
	float _MouseRadius;			// Radius of mouse interaction
	bool _MouseDown;			// Is the mouse pressed
};
// ▲ Definition of shader constants -----------------------
