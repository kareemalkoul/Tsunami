
/// Implementation of Spiky kernel
/// mass * ((pressure_i + pressure_j)/(2 * density_j)) * Grad(W_Spiky)
/// Grad(W_Spiky) = -30 / (pi * h^5) * (h - r)^2
inline float3 CalculateGradPressure(float r, float P_pressure, float N_pressure, float N_density, float3 diff) {
	const float h = _Smoothlen;
	float avg_pressure = 0.5f * (N_pressure + P_pressure);
	// _GradPressureCoef = particleMass * -45.0f / (Mathf.PI * Mathf.Pow(smoothlen, 6))
	// N=j
	// P=i curr particle
	// float3 Grad=W_Spiky=_GradPressureCoef*(h - r) * (h - r) * diff / r;
	return _GradPressureCoef * avg_pressure / N_density * (h - r) * (h - r) / r * (diff);
}