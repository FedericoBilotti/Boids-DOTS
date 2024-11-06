using Unity.Entities;
using Unity.Transforms;

namespace Boids.Movement
{
    public readonly partial struct BoidsMovementAspect : IAspect
    {
        public readonly Entity self;

        public readonly RefRW<LocalTransform> localTransform;
        public readonly DynamicBuffer<BoidsVelocityBuffer> boidsVelocityBuffer;
        private readonly RefRO<BoidsMaxVelocity> _boidsMaxVelocity;
        private readonly RefRO<BoidsVisionRadius> _boidsVisionRadius;
        private readonly RefRO<BoidsWeight> _boidsWeight;

        public float RadiusSeparation => _boidsVisionRadius.ValueRO.radiusSeparation;
        public float RadiusAlignment => _boidsVisionRadius.ValueRO.radiusAlignment;
        public float RadiusCohesion => _boidsVisionRadius.ValueRO.radiusCohesion;
        public float RadiusHunter => _boidsVisionRadius.ValueRO.radiusHunter;

        public float WeightSeparation => _boidsWeight.ValueRO.weightSeparation;
        public float WeightAlignment => _boidsWeight.ValueRO.weightAlignment;
        public float WeightCohesion => _boidsWeight.ValueRO.weightCohesion;

        public float MaxSpeed => _boidsMaxVelocity.ValueRO.maxSpeed;
        public float MaxForce => _boidsMaxVelocity.ValueRO.maxForce;
        public float Smoothing => _boidsMaxVelocity.ValueRO.smoothing;
    }
}

// public float3 Separation(PhysicsWorldSingleton physWorld)
// {
//     float3 desired = float3.zero;
//     
//     NativeList<DistanceHit> separationBoids = new NativeList<DistanceHit>(Allocator.TempJob);
//
//     physWorld.OverlapSphere(localTransform.ValueRO.Position, RadiusSeparation, ref separationBoids, CollisionFilter.Default);
//
//     foreach (DistanceHit hit in separationBoids)
//     {
//         if (hit.Entity == self) continue;
//
//         float3 distBoids = hit.Position - localTransform.ValueRO.Position;
//
//         desired += distBoids;
//     }
//
//     desired *= -1;
//     
//     separationBoids.Dispose();
//
//     return math.all(desired == float3.zero) ? desired : Steering(desired);
// }
//
// public float3 Alignment(PhysicsWorldSingleton physWorld) // , ComponentLookup<BoidsVelocity> boidsVelocityLookup
// {
//     float3 desired = float3.zero;
//     int countBoids = 0;
//
//     NativeList<DistanceHit> alignmentBoids = new NativeList<DistanceHit>(Allocator.TempJob);
//
//     physWorld.OverlapSphere(localTransform.ValueRO.Position, RadiusAlignment, ref alignmentBoids, CollisionFilter.Default);
//
//     foreach (DistanceHit hit in alignmentBoids)
//     {
//         if (hit.Entity == self) continue;
//
//         // desired += boidsVelocityLookup[hit.Entity].velocity;
//         countBoids++;
//     }
//
//     alignmentBoids.Dispose();
//
//     if (countBoids == 0) return desired;
//     desired /= countBoids;
//
//     return Steering(desired);
// }
//
// public float3 Cohesion(PhysicsWorldSingleton physWorld)
// {
//     float3 desired = float3.zero;
//     int countBoids = 0;
//
//     NativeList<DistanceHit> cohesionBoids = new NativeList<DistanceHit>(Allocator.TempJob);
//
//     physWorld.OverlapSphere(localTransform.ValueRO.Position, RadiusCohesion, ref cohesionBoids, CollisionFilter.Default);
//
//     foreach (DistanceHit hit in cohesionBoids)
//     {
//         if (hit.Entity == self) continue;
//
//         desired += localTransform.ValueRO.Position;
//         countBoids++;
//     }
//
//
//     if (countBoids == 0) return desired;
//
//     desired /= countBoids;
//     desired -= localTransform.ValueRO.Position;
//     
//     cohesionBoids.Dispose();
//
//     return Steering(desired);
// }
//
//
// public void MoveAndRotateBoid(float deltaTime)
// {
//     localTransform.ValueRW = localTransform.ValueRW.Translate(_velocity.ValueRO.velocity * deltaTime);
//
//     if (math.all(_velocity.ValueRO.velocity == float3.zero)) return; // Prevenir rotación innecesaria.
//
//     quaternion targetRotation = quaternion.LookRotationSafe(math.normalize(_velocity.ValueRO.velocity), math.up());
//
//     localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRW.Rotation, targetRotation, deltaTime * Smoothing);
// }
//
// public float3 AddForce(float3 force) => _velocity.ValueRW.velocity = MathHelper.ClampMagnitude(_velocity.ValueRO.velocity + force, MaxSpeed);
// public float3 Steering(float3 desired) => MathHelper.ClampMagnitude(desired * MaxSpeed - _velocity.ValueRO.velocity, MaxForce);