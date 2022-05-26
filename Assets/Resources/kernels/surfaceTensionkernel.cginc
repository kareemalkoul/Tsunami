inline ColorField CalculateSurfaceTension(float r_sq , float N_density, float3 diff)
{
	//diff =r=ri-rj
	//N=j is neighbors
	//p=i
	ColorField color;
	float colorField = 0.0f;
	float3 colorGradient = float3(0,0,0);
	float colorLaplacian = 0.0f;
	float tensionCoefficient = 0.0728f;
	const float h = _Smoothlen;
	const float h_sq = _Smoothlen * _Smoothlen;
	// const float r_sq = r * r;

	// colorField += j.mass / j.density * Kernel.Poly6(i.position - j.position, h);
	colorGradient=_GradTensionCoef*diff * (h_sq - r_sq) *(h_sq - r_sq) /N_density;
  
	// colorGradient += j.mass / j.density * Kernel.Poly6Grad(i.position - j.position, h);


	colorLaplacian=_LapTensionCoef * (h_sq - r_sq) *(3 *(h_sq)-7*r_sq)/N_density;

	// colorLaplacian += j.mass / j.density * Kernel.Poly6Lap(i.position - j.position, h);



	float sqrMagnitude=colorGradient.x*colorGradient.x+colorGradient.y*colorGradient.y+colorGradient.z*colorGradient.z;
	if (sqrMagnitude < _tensionThreshold * _tensionThreshold)
	{
		// This particle isn't close to the surface
		return color;
	}

	color.colorField=colorField;
	color.colorLaplacian=colorLaplacian;
	color.colorGradient=colorGradient;
	// float tension=-tensionCoefficient*colorLaplacian*colorGradient/colorField

	return color;
}
