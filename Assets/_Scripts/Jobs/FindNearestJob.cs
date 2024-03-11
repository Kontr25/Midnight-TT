using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _Scripts.Jobs
{
    [BurstCompile]
    public struct FindNearestJob : IJob
    {
        public NativeArray<Vector3> FoodPositions;
        public NativeArray<bool> FoodActivityStates;
        public NativeArray<Vector3> AgentsPositions;
        public NativeArray<Vector3> Result;

        public void Execute()
        {
            Vector3 averagePosition = CalculateAveragePosition();

            Vector3 nearestFoodPosition = Vector3.zero;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < FoodPositions.Length; i++)
            {
                if (FoodActivityStates[i])
                {
                    Vector3 foodPosition = FoodPositions[i];
                    float distance = Vector3.Distance(averagePosition, foodPosition);

                    if (distance < nearestDistance)
                    {
                        nearestFoodPosition = foodPosition;
                        nearestDistance = distance;
                    }
                }
            }

            Result[0] = nearestFoodPosition;
        }

        Vector3 CalculateAveragePosition()
        {
            if (AgentsPositions.Length == 0)
            {
                return Vector3.zero;
            }

            Vector3 sum = Vector3.zero;

            for (int i = 0; i < AgentsPositions.Length; i++)
            {
                sum += AgentsPositions[i];
            }

            return sum / AgentsPositions.Length;
        }
    }
}