void normalWallboundray(inout  float3 velocity,inout float3 position){

  if ( position.x < 0)
  {
    position.x =0;
    velocity.x = -velocity.x * _Damping;
  }
  else if ( position.x > _Range.x)
  {
    position.x = _Range.x;
    velocity.x = -velocity.x * _Damping;
  }

  if ( position.y <0)
  {
    position.y = 0;
    velocity.y = -velocity.y * _Damping;
  }
  else if ( position.y > _Range.y)
  {
    position.y = _Range.y;
    velocity.y = -velocity.y * _Damping;
  }

  if ( position.z < 0)
  {
    position.z = 0;
    velocity.z = -velocity.z * _Damping;
  }
  else if ( position.z >_Range.z)
  {
    position.z = _Range.z;
    velocity.z = -velocity.z * _Damping;
  }
}