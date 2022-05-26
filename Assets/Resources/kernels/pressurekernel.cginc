
/// Becker2007 Implementation of uncompressed conditions:
/// Pressure = B * ((rho / rho_0)^gamma  - 1)
/// The pressure constant B should be calculated accurately, but it is not suitable for real time, so set it to an appropriate value.
inline float CalculatePressure(float density) {
	return _PressureStiffness * max(pow(abs(density / _RestDensity), 7) - 1, 0);
}
