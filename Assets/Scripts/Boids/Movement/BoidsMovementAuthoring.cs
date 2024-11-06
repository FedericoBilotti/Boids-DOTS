using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Boids.Movement
{
    public class BoidsMovementAuthoring : MonoBehaviour
    {
        [Header("Forces")] 
        public float maxSpeed = 5f;
        public float maxForce = 2.5f;
        public float smoothing = 5f;
        
        [Header("Vision Radius")] 
        public float radiusSeparation;
        public float radiusAlignment;
        public float radiusCohesion;
        public float radiusHunter;

        [Header("Weights")] 
        [Range(0f, 25f)] public float weightSeparation;
        [Range(0f, 25f)] public float weightAlignment;
        [Range(0f, 25f)] public float weightCohesion;

        private class Baker : Baker<BoidsMovementAuthoring>
        {
            public override void Bake(BoidsMovementAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new BoidsVelocity());

                AddComponent(entity, new BoidsMaxVelocity 
                { 
                    maxSpeed = authoring.maxSpeed, 
                    maxForce = authoring.maxForce,
                    smoothing = authoring.smoothing
                });
            
                AddComponent(entity, new BoidsVisionRadius
                {
                    radiusSeparation = authoring.radiusSeparation,
                    radiusAlignment = authoring.radiusAlignment,
                    radiusCohesion = authoring.radiusCohesion,
                    radiusHunter = authoring.radiusHunter
                });

                AddComponent(entity, new BoidsWeight
                {
                    weightSeparation = authoring.weightSeparation,
                    weightAlignment = authoring.weightAlignment,
                    weightCohesion = authoring.weightCohesion,
                });
                
                AddBuffer<BoidsVelocityBuffer>(entity);
            }
        }
    }

    // Se usaria para velocidad del boid.
    public struct BoidsVelocity : IComponentData
    {
        public float3 velocity;
    }

    public struct BoidsMaxVelocity : IComponentData
    {
        public float maxSpeed;
        public float maxForce;
        public float smoothing;
    }

    public struct BoidsVisionRadius : IComponentData
    {
        public float radiusSeparation;
        public float radiusAlignment;
        public float radiusCohesion;
        public float radiusHunter;
    }

    public struct BoidsWeight : IComponentData
    {
        public float weightSeparation;
        public float weightAlignment;
        public float weightCohesion;
    }
}