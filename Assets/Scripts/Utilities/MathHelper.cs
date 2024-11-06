using Unity.Mathematics;

namespace Utilities
{
    public static class MathHelper
    {
        public static float3 ClampMagnitude(float3 vector, float maxLength)
        {
            float length = math.length(vector);

            if (length > maxLength) return math.normalize(vector) * maxLength;

            return vector;
        }

        /// <summary>
        /// Generates a random position inside a unit box.
        /// </summary>
        /// <returns></returns>
        public static float3 RandomInsideUnitBox(Random random)
        {
            return new float3(random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f));
        }

        /// <summary>
        /// Generates a random position inside a unit sphere.
        /// </summary>
        /// <returns></returns>
        public static float3 RandomInsideUnitSphere(Random random)
        {
            float3 dir = math.normalize(RandomInsideUnitBox(random));
            float radius = random.NextFloat(0f, 1f);
            return dir * radius;
        }

        /// <summary>
        /// Generates a random rotation.
        /// </summary>
        /// <returns></returns>
        public static quaternion RandomRotation(Random random)
        {
            float3 direction = new float3(random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f));
            return quaternion.Euler(direction);
        }
    }
}