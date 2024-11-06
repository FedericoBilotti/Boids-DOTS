using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Limits
{
    public class BoidsLimitsAuthoring : MonoBehaviour
    {
        public float3 bounds;

        public class Baker : Baker<BoidsLimitsAuthoring>
        {
            public override void Bake(BoidsLimitsAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new BoundsLimits
                {
                    bounds = authoring.bounds
                });
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireCube(transform.position, bounds);
        }
    }

    public struct BoundsLimits : IComponentData
    {
        public float3 bounds;
    }
}