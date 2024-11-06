using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Utilities;

namespace Boids.Movement
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct BoidsMovementSystem : ISystem
    {
        private ComponentLookup<BoidsVelocity> _boidsVelocityLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();

            _boidsVelocityLookup = state.GetComponentLookup<BoidsVelocity>(isReadOnly: false);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _boidsVelocityLookup.Update(ref state);

            float deltaTime = SystemAPI.Time.DeltaTime;

            var physWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var flockingJob = new FlockingJob
            {
                physWorld = physWorld,
                boidsVelocityLookup = _boidsVelocityLookup,
                ecb = ecb
            }.ScheduleParallel(state.Dependency);

            var applyVelocityJob = new ApplyVelocityJob
            {
                deltaTime = deltaTime,
                boidsVelocityLookup = _boidsVelocityLookup
            }.Schedule(flockingJob);

            state.Dependency = applyVelocityJob;
        }
    }

    [BurstCompile]
    public partial struct ApplyVelocityJob : IJobEntity
    {
        public float deltaTime;
        public ComponentLookup<BoidsVelocity> boidsVelocityLookup;

        [BurstCompile]
        public void Execute(BoidsMovementAspect boid)
        {
            if (boid.boidsVelocityBuffer.Length == 0) return;
            
            BoidsVelocity boidVelocity = boidsVelocityLookup[boid.self];
            boidVelocity.velocity = AddForce(boidVelocity.velocity, boid.boidsVelocityBuffer[0].desiredVelocity, boid.MaxSpeed);
            boidsVelocityLookup[boid.self] = boidVelocity;
            
            boid.boidsVelocityBuffer.Clear();

            boid.localTransform.ValueRW = boid.localTransform.ValueRW.Translate(boidsVelocityLookup[boid.self].velocity * deltaTime);
            if (math.all(boidsVelocityLookup[boid.self].velocity == float3.zero)) return; // Prevenir rotación innecesaria.
            quaternion targetRotation = quaternion.LookRotationSafe(math.normalize(boidsVelocityLookup[boid.self].velocity), math.up());
            boid.localTransform.ValueRW.Rotation = math.slerp(boid.localTransform.ValueRW.Rotation, targetRotation, deltaTime * boid.Smoothing);
        }
        
        public float3 AddForce(float3 velocity, float3 force, float maxSpeed) => MathHelper.ClampMagnitude(velocity + force, maxSpeed);
    }

    [BurstCompile]
    public partial struct FlockingJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorldSingleton physWorld;
        [ReadOnly] public ComponentLookup<BoidsVelocity> boidsVelocityLookup;
        public EntityCommandBuffer.ParallelWriter ecb;

        [BurstCompile]
        public void Execute(BoidsMovementAspect boid, [ChunkIndexInQuery] int sortKey)
        {
            NativeList<DistanceHit> separationBoids = new NativeList<DistanceHit>(Allocator.TempJob);
            NativeList<DistanceHit> alignmentBoids = new NativeList<DistanceHit>(Allocator.TempJob);
            NativeList<DistanceHit> cohesionBoids = new NativeList<DistanceHit>(Allocator.TempJob);

            physWorld.OverlapSphere(boid.localTransform.ValueRO.Position, boid.RadiusAlignment, ref alignmentBoids, CollisionFilter.Default);
            physWorld.OverlapSphere(boid.localTransform.ValueRO.Position, boid.RadiusSeparation, ref separationBoids, CollisionFilter.Default);
            physWorld.OverlapSphere(boid.localTransform.ValueRO.Position, boid.RadiusCohesion, ref cohesionBoids, CollisionFilter.Default);

            float3 desiredForce = Separation(ref separationBoids, ref boid) * boid.WeightSeparation;
            desiredForce += Alignment(ref alignmentBoids, ref boid) * boid.WeightAlignment;
            desiredForce += Cohesion(ref cohesionBoids, ref boid) * boid.WeightCohesion;

            var boidVelocityBuffer = new BoidsVelocityBuffer { desiredVelocity = desiredForce };
            ecb.AppendToBuffer(sortKey, boid.self, boidVelocityBuffer);

            separationBoids.Dispose();
            alignmentBoids.Dispose();
            cohesionBoids.Dispose();
        }

        private float3 Separation(ref NativeList<DistanceHit> separationBoids, ref BoidsMovementAspect boid)
        {
            float3 desired = float3.zero;

            foreach (DistanceHit hit in separationBoids)
            {
                if (hit.Entity == boid.self) continue;

                float3 distBoids = hit.Position - boid.localTransform.ValueRO.Position;

                desired += distBoids;
            }

            desired *= -1;

            return math.all(desired == float3.zero) ? desired : Steering(boid, desired);
        }

        private float3 Alignment(ref NativeList<DistanceHit> alignmentBoids, ref BoidsMovementAspect boid)
        {
            float3 desired = float3.zero;
            int countBoids = 0;

            foreach (DistanceHit hit in alignmentBoids)
            {
                if (hit.Entity == boid.self) continue;

                desired += boidsVelocityLookup[hit.Entity].velocity;
                countBoids++;
            }

            if (countBoids == 0) return desired;
            desired /= countBoids;

            return Steering(boid, desired);
        }

        private float3 Cohesion(ref NativeList<DistanceHit> cohesionBoids, ref BoidsMovementAspect boid)
        {
            float3 desired = float3.zero;
            int countBoids = 0;

            foreach (DistanceHit hit in cohesionBoids)
            {
                if (hit.Entity == boid.self) continue;

                desired += boid.localTransform.ValueRO.Position;
                countBoids++;
            }


            if (countBoids == 0) return desired;

            desired /= countBoids;
            desired -= boid.localTransform.ValueRO.Position;

            return Steering(boid, desired);
        }

        public float3 Steering(BoidsMovementAspect boid, float3 desired) => MathHelper.ClampMagnitude(desired * boid.MaxSpeed - boidsVelocityLookup[boid.self].velocity, boid.MaxForce);
    }
}

// [BurstCompile]
// public partial struct CalculateForcesJob : IJobEntity
// {
//     public float deltaTime;
//     [ReadOnly] public PhysicsWorldSingleton physWorld;
//
//     [BurstCompile]
//     public void Execute(BoidsMovementAspect boid)
//     {
//         float3 force = boid.Separation(physWorld) * boid.WeightSeparation;
//         force += boid.Alignment(physWorld) * boid.WeightAlignment;
//         force += boid.Cohesion(physWorld) * boid.WeightCohesion;
//
//         boid.AddForce(force * boid.MaxForce);
//         boid.MoveAndRotateBoid(deltaTime);
//     }
// }

// #region Separation
//
// foreach (DistanceHit hit in separationBoids)
// {
//     if (hit.Entity == boid.self) continue;
//
//     float3 distBoids = hit.Position - boid.localTransform.ValueRO.Position;
//
//     desired += distBoids;
// }
//
// desired *= -1;
//
// if (math.all(desired != float3.zero)) desired = Steering(boid, desired);
//
// desired *= boid.WeightSeparation;
//
// #endregion

// #region Alignment
//
// // Alignment
//
// int countBoids = 0;
//
// foreach (DistanceHit hit in alignmentBoids)
// {
//     if (hit.Entity == boid.self) continue;
//
//     desired += boidsVelocityLookup[hit.Entity].velocity;
//     countBoids++;
// }
//
// if (countBoids != 0) desired /= countBoids;
// if (math.all(desired != float3.zero)) desired = Steering(boid, desired);
//
// desired *= boid.RadiusAlignment;
//
// #endregion

// #region Cohesion
// foreach (DistanceHit hit in cohesionBoids)
// {
//     if (hit.Entity == boid.self) continue;
//
//     desired += boid.localTransform.ValueRO.Position;
//     countBoids++;
// }
//
//
// if (countBoids != 0)
// {
//     desired /= countBoids;
//     desired -= boid.localTransform.ValueRO.Position;
// }
//
// if (math.all(desired != float3.zero)) desired = Steering(boid, desired);
//
// desired *= boid.WeightCohesion;
//
// #endregion