using Boids.Movement;
using Unity.Entities;
using UnityEngine;

namespace Spawner
{
    public class SpawnerAuthoring : MonoBehaviour
    {
        public float totalTime = 5f;
        public float radiusSpawn = 50f;
        public int amountToSpawn = 20;
        public int maxBoids = 100;
        
        public BoidsMovementAuthoring boidAuthoring;
        
        private class Baker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.None);
                
                AddComponent(entity, new SpawnerTimer
                {
                    totalTime = authoring.totalTime,
                });
                
                AddComponent(entity, new SpawnerFields
                { 
                    boidEntity = GetEntity(authoring.boidAuthoring, TransformUsageFlags.Dynamic),
                    radiusSpawn = authoring.radiusSpawn,
                    amountToSpawn = authoring.amountToSpawn,
                    maxBoids = authoring.maxBoids
                });
                
                AddComponent(entity, new SpawnerActualBoids());
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.72f, 0.84f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2 * radiusSpawn);
        }
    }

    public struct SpawnerTimer : IComponentData
    {
        public float totalTime;
        public float actualTime;
    }

    public struct SpawnerActualBoids : IComponentData
    {
        public int actualBoids;
    }

    public struct SpawnerFields : IComponentData
    {
        public Entity boidEntity;
        public float radiusSpawn;
        public int amountToSpawn;
        public int maxBoids;
    }
}