using Unity.Entities;
using Unity.Mathematics;

namespace Boids.Movement
{
    public struct BoidsVelocityBuffer : IBufferElementData
    {
        public float3 desiredVelocity;
    }
}