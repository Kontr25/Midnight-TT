using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _Scripts.Jobs
{
    [BurstCompile]
    public struct AgentMoveBoundsJob: IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> AgentPositions;
        public NativeArray<bool> IsReproducingAgents;
        public NativeArray<Vector3> AgentAccelerations;
        public Vector3 AreaSize;
        public float BoundsThreshold;
        public float BoundsCompensationMultiplier;
        
        public void Execute(int index)
        {
            if (IsReproducingAgents[index])
            {
                return;
            }
            
            var position = AgentPositions[index];
            var size = AreaSize * 0.5f;
            AgentAccelerations[index] += Compensate(-size.x - position.x, Vector3.right)
                                    + Compensate(size.x - position.x, Vector3.left)
                                    + Compensate(-size.y - position.y, Vector3.up)
                                    + Compensate(size.y - position.y, Vector3.down)
                                    + Compensate(-size.z - position.z, Vector3.forward)
                                    + Compensate(size.z - position.z, Vector3.back);

        }

        private Vector3 Compensate(float delta, Vector3 direction)
        {
            delta = Mathf.Abs(delta);
            if (delta > BoundsThreshold)
            {
                return Vector3.zero;
            }
            else
            {
                return direction * (1 - delta / BoundsThreshold) * BoundsCompensationMultiplier;
            }
        }
    }
}