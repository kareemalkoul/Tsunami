// Wall boundary 
// (penalty method)
float3 wallboundray(float3 acceleration,float3 position){
	// Wall boundary (penalty method)
	float dist;
	//x in 0
	dist = dot(float4(position, 1), float4(1, 0, 0, 0));
	acceleration += min(dist, 0) * -_WallStiffness * float3(1, 0, 0);
	//y in 0
	dist = dot(float4(position, 1), float4(0, 1, 0, 0));
	acceleration += min(dist, 0) * -_WallStiffness * float3(0, 1, 0);
	//z in 0
	dist = dot(float4(position, 1), float4(0, 0, 1, 0));
	acceleration += min(dist, 0) * -_WallStiffness * float3(0, 0, 1);
	//x in max
	dist = dot(float4(position, 1), float4(-1, 0, 0, _Range.x));
	acceleration += min(dist, 0) * -_WallStiffness * float3(-1, 0, 0);
	//y in max
	dist = dot(float4(position, 1), float4(0, -1, 0, _Range.y));
	acceleration += min(dist, 0) * -_WallStiffness * float3(0, -1, 0);
	//z in max
	dist = dot(float4(position, 1), float4(0, 0, -1, _Range.z));
	acceleration += min(dist, 0) * -_WallStiffness * float3(0, 0, -1);

	return acceleration;
}