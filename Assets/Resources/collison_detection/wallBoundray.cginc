// Wall boundary 
// (penalty method)
float3 penaltyWallboundray(float3 acceleration,float3 position){
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



// Wall boundary 
// (penalty method)
float3 buildings(float3 acceleration,float3 position,int zmin,int zmax,int xmin,int xmax) {
  // Wall boundary (penalty method)
  float dist;

    if(position.z <= zmax && position.z >= zmin && position.x <= xmax && position.x > xmin) {

        int disx = xmin - position.x;
        int disz = zmin - position.z;
        int disxm = position.x  - xmax;
        int diszm = position.z - zmax;

        
        if(position.x >= xmin && position.x < xmax && disx > disz && disx > disxm){
           
            acceleration += -1 * -_WallStiffness * float3(-1, 0, 0);
        }
        else if(position.z >= zmin && position.z < zmax && disz > disx && disz > diszm) {
        
            acceleration += -1 * -_WallStiffness * float3(0, 0, -1);
        } else if(disx < disxm && disz < diszm){
             acceleration += 0.1 * -_WallStiffness * float3(-1, 0, -1);
        }
        
        if(position.x > xmin && position.x <= xmax && disxm > diszm && disxm > disx){
         
            acceleration += -1 * -_WallStiffness * float3(1, 0, 0);
        }
        else if(position.z > zmin && position.z <= zmax && diszm > disxm && diszm > disz) {
         
            acceleration += -1 * -_WallStiffness * float3(0, 0, 1);
        } else if(disx > disxm && disz > diszm){
             acceleration += 0.1 * -_WallStiffness * float3(1, 0, 1);
        }
        
        //y in 0
        dist = dot(float4(position, 1), float4(0, 1, 0, 0));
        acceleration += min(dist, 0) * -_WallStiffness * float3(0, 1, 0);
    }

  return acceleration;

}