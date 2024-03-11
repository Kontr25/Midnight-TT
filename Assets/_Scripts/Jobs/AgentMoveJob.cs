using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace _Scripts.Jobs
{
    [BurstCompile]
    public struct AgentMoveJob : IJobParallelForTransform
    {
        public NativeArray<Vector3> AgentPositions;
        public NativeArray<Vector3> AgentVelocities;
        public NativeArray<bool> IsReproducingAgents;
        public NativeArray<Vector3> AgentAccelerations;
        public float DeltaTime;
        public float VelocityLimit;
        
        public void Execute(int index, TransformAccess transform)
        {
            if (IsReproducingAgents[index])
            {
                return;
            }
            
            var velocity = AgentVelocities[index] + AgentAccelerations[index] * DeltaTime;
            var direction = velocity.normalized;
            velocity = direction * Mathf.Clamp(velocity.magnitude, 1, VelocityLimit);

            transform.position += velocity * DeltaTime;
            transform.rotation = Quaternion.LookRotation(velocity);

            AgentPositions[index] = transform.position;
            AgentVelocities[index] = velocity;
            AgentAccelerations[index] = Vector3.zero;
        }
    }
}