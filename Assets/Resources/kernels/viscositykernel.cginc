
/// Viscosity kernel implementation:
/// mass * (u_j - u_i) / density_j * Laplacian(W_viscosity)
/// Laplacian(W_viscosity) = 20 / (3 * pi * h^5) * (h - r)
inline float3 CalculateLapVelocity(float r, float3 P_velocity, float3 N_velocity, float N_density) {
	const float h = _Smoothlen;
	// _LapViscosityCoef= particleMass * 45f / (Mathf.PI * Mathf.Pow(smoothlen, 6))
	// N=j
	// P=i curr particle
	float3 vel_diff = (N_velocity - P_velocity);
	return _LapViscosityCoef / N_density * (h - r) * vel_diff;
}