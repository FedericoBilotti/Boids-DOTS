using Boids.Movement;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Limits
{
    [BurstCompile]
    [UpdateAfter(typeof(BoidsMovementSystem))]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct BoidsLimitSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoundsLimits>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var boundsEntity = SystemAPI.GetSingletonEntity<BoundsLimits>();
            var boundsLimits = SystemAPI.GetComponent<BoundsLimits>(boundsEntity);

            state.Dependency = new BoundsLimitsJob
            {
                boundsLimits = boundsLimits
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct BoundsLimitsJob : IJobEntity
    {
        [ReadOnly] public BoundsLimits boundsLimits;

        [BurstCompile]
        public void Execute(BoidsMovementAspect boids)
        {
            boids.localTransform.ValueRW.Position = RespectLimits(boids.localTransform.ValueRO.Position, boundsLimits.bounds.x, boundsLimits.bounds.y, boundsLimits.bounds.z);
        }

        public static float3 RespectLimits(float3 pos, float x, float y, float z)
        {
            if (pos.x < -x / 2) pos.x = x / 2;
            if (pos.x > x / 2) pos.x = -x / 2;
            if (pos.y < -y / 2) pos.y = y / 2;
            if (pos.y > y / 2) pos.y = -y / 2;
            if (pos.z < -z / 2) pos.z = z / 2;
            if (pos.z > z / 2) pos.z = -z / 2;

            return pos;
        }
    }
}