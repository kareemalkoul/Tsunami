
/// Implementation of Poly6 Kernel 
/// W_poly6(r, h) = DensityCoef * (h^2 - r^2)^3
/// -> DensityCoef = ParticleMass * 4 / (PI * Smoothlen^8)
inline float CalculateDensity(float r_sq) {
	const float h_sq = _Smoothlen * _Smoothlen;
	return _DensityCoef * (h_sq - r_sq) * (h_sq - r_sq) * (h_sq - r_sq);
}
