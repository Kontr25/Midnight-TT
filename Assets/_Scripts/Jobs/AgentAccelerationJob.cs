using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _Scripts.Jobs
{
    [BurstCompile]
    public struct AgentAccelerationJob: IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> AgentPositions;
        [ReadOnly]
        public NativeArray<Vector3> AgentVelocities;
        public NativeArray<bool> IsReproducingAgents;
        public NativeArray<Vector3> AgentAccelerations;
        public float AccelerationDestinationThreshold;
        public Vector4 Weights;
        public Vector3 TargetPosition;
        
        private int _agentCount => AgentPositions.Length - 1;
        
        public void Execute(int index)
        {
            if (IsReproducingAgents[index])
            {
                return;
            }
            
            Vector3 averageSpread = Vector3.zero;
            Vector3 averageVelocity = Vector3.zero;
            Vector3 averagePosition = Vector3.zero;

            for (int i = 0; i < _agentCount; i++)
            {
                if (i == index)
                {
                    continue;
                }

                var targetPosition = AgentPositions[i];
                var positionDifference = AgentPositions[index] - targetPosition;
                if (positionDifference.magnitude > AccelerationDestinationThreshold)
                {
                    continue;
                }

                averageSpread += positionDifference.normalized;
                averageVelocity += AgentVelocities[i];
                averagePosition += targetPosition;
            }

            AgentAccelerations[index] += (averageSpread / _agentCount) * Weights.x
                + (averageVelocity / _agentCount) * Weights.y
                + (averagePosition / _agentCount - AgentPositions[index]) * Weights.z;
            
            var targetDirection = (TargetPosition - AgentPositions[index]).normalized;
            AgentAccelerations[index] += targetDirection * Weights.w;
        }
    }
}