	
float3 normalWallboundray(float3 velocity,float3 position,float3 range,float velocityDamping){

     velocityDamping=0.8f;
	if ( position.x < 0)
	{
		position.x =0;
		velocity.x = -velocity.x * velocityDamping;
	}
	else if ( position.x > range.x)
	{
		position.x = range.x;
		velocity.x = -velocity.x * velocityDamping;
	}

	if ( position.y <0)
	{
		position.y = 0;
		velocity.y = -velocity.y * velocityDamping;
	}
	else if ( position.y > range.y)
	{
		position.y = range.y;
		velocity.y = -velocity.y * velocityDamping;
	}

	if ( position.z < 0)
	{
		position.z = 0;
		velocity.z = -velocity.z * velocityDamping;
	}
	else if ( position.z >range.z)
	{
		position.z = range.z;
		velocity.z = -velocity.z * velocityDamping;
	}
    return velocity;
}