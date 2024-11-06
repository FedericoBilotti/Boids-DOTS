using Boids.Movement;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Utilities;
using Random = Unity.Mathematics.Random;

namespace Spawner
{
    public readonly partial struct SpawnerAspect : IAspect
    {
        public readonly Entity self;
        private readonly RefRO<SpawnerFields> _spawnerFields;
        private readonly RefRW<SpawnerTimer> _spawnerTimer;
        private readonly RefRW<SpawnerActualBoids> _spawnerTotalBoids;

        private float ActualTime { get => _spawnerTimer.ValueRO.actualTime; set => _spawnerTimer.ValueRW.actualTime = value; }
        private float TotalTime => _spawnerTimer.ValueRO.totalTime;

        private Entity EntityBoid => _spawnerFields.ValueRO.boidEntity;
        private float RadiusSpawn => _spawnerFields.ValueRO.radiusSpawn;
        private int AmountToSpawn => _spawnerFields.ValueRO.amountToSpawn;
        private int MaxBoids => _spawnerFields.ValueRO.maxBoids;
        private int ActualBoids { get => _spawnerTotalBoids.ValueRO.actualBoids; set => _spawnerTotalBoids.ValueRW.actualBoids = value; }

        public void SpawnBoids(EntityCommandBuffer.ParallelWriter ecb, int sortKey, Random random)
        {
            for (int i = 0; i < AmountToSpawn; i++)
            {
                Entity boid = ecb.Instantiate(sortKey, EntityBoid);
                ecb.SetComponent(sortKey, boid, new LocalTransform
                {
                    Position = RandomPositionAtSpawn(random, RadiusSpawn),
                    Rotation = RandomRotation(random),
                    Scale = 1f
                });
                
                float3 initialVelocity = new float3(random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f));
                
                ecb.SetComponent(sortKey, boid, new BoidsVelocity
                {
                    velocity = initialVelocity
                });
            }

            ActualBoids += AmountToSpawn;
        }

        private static float3 RandomPositionAtSpawn(Random random, float radius) => MathHelper.RandomInsideUnitBox(random) * radius;
        private static quaternion RandomRotation(Random random) => MathHelper.RandomRotation(random);

        public bool CanSpawnBoids(float deltaTime)
        {
            if (ActualTime > TotalTime && ActualBoids < MaxBoids) return true;

            ActualTime += deltaTime;
            return false;
        }

        public void ResetTimer() => ActualTime = 0f;
    }
}