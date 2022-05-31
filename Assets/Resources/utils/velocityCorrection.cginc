inline float3 VelocityCorrection(float r_sq, float3 P_velocity, float3 N_velocity, float N_density,float P_density)
{
	const float h_sq = _Smoothlen * _Smoothlen;
	return (P_velocity - N_velocity) *_DensityCoef * (h_sq - r_sq) * (h_sq - r_sq) * (h_sq - r_sq)/(P_density+ N_density);
}