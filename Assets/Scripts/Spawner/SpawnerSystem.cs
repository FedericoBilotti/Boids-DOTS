using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Spawner
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct SpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SpawnerFields>();
            state.RequireForUpdate<SpawnerTimer>();
            state.RequireForUpdate<SpawnerActualBoids>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
                    
            var random = new Random((uint)SystemAPI.Time.ElapsedTime + 1000);
            
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();

            state.Dependency = new SpawnerJob
            {
                deltaTime = deltaTime,
                ecb = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                random = random,
            }.ScheduleParallel(state.Dependency);
        }
    }
    
    [BurstCompile]
    public partial struct SpawnerJob : IJobEntity
    {
        public float deltaTime;
        public EntityCommandBuffer.ParallelWriter ecb;
        public Random random;
        
        [BurstCompile]
        public void Execute(SpawnerAspect spawner, [ChunkIndexInQuery] int sortKey)
        {
            if (!spawner.CanSpawnBoids(deltaTime)) return;
            
            spawner.SpawnBoids(ecb, sortKey, random);
            spawner.ResetTimer();
        }
    }
}