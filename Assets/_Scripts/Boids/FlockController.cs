using System.Collections.Generic;
using _Scripts.Jobs;
using _Scripts.UI;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

namespace _Scripts.Boids
{
    public class FlockController: MonoBehaviour
    {
        [SerializeField] private int _startAgentsCount;
        [SerializeField] private Agent _agentPrefab;
        [SerializeField] private float _accelerationDestinationThreshold;
        [SerializeField] private float _boundsThreshold;
        [SerializeField] private float _boundsCompensationMultiplier;
        [SerializeField] private float _defaultVelocityLimit;
        [SerializeField] private Vector4 _accelerationWeights;
        [SerializeField] private AgentCounter _agentCounter;

        private NativeArray<Vector3> _agentPositions;
        private NativeArray<Vector3> _agentVelocities;
        private NativeArray<Vector3> _agentAccelerations;
        private TransformAccessArray _agentTransformAccessArray;
        private NativeArray<bool> _isReproducingAgents;
        private Vector3 _areaSize;
        private bool _gameIsStarted;
        private List<Agent> _allAgents = new List<Agent>();
        private List<Food.Food> _allFood = new List<Food.Food>();
        private NativeArray<Vector3> _foodPositions;
        private NativeArray<bool> _foodActivityStates;
        private NativeArray<Vector3> _targetFood;
        private Transform[] _agentTransforms;
        private int _currentAgentCount;
        private float _currentVelocityLimit;

        public Vector3 AreaSize
        {
            get => _areaSize;
            set => _areaSize = value;
        }

        public List<Food.Food> AllFood
        {
            get => _allFood;
            set => _allFood = value;
        }

        public int CurrentAgentCount
        {
            get => _currentAgentCount;
            set
            {
                _agentCounter.UpdateAgentCount(value);
                _currentAgentCount = value;
            }
        }

        public void StartGame()
        {
            _currentVelocityLimit = _defaultVelocityLimit;
            _foodPositions = new NativeArray<Vector3>(_allFood.Count, Allocator.Persistent);
            _foodActivityStates = new NativeArray<bool>(_allFood.Count, Allocator.Persistent);
            _targetFood = new NativeArray<Vector3>(1, Allocator.Persistent);

            SpawnAgents(_startAgentsCount, Vector3.zero);
            _gameIsStarted = true;
        }

        private void Update()
        {
            if(!_gameIsStarted) return;

            FlockMovement();
        }

        private void FlockMovement()
        {
            var findNearestJob = new FindNearestJob()
            {
                AgentsPositions = _agentPositions,
                FoodActivityStates = _foodActivityStates,
                FoodPositions = _foodPositions,
                Result = _targetFood
            };

            var findNearestFoodHandle = findNearestJob.Schedule();
            findNearestFoodHandle.Complete();

            var boundsJob = new AgentMoveBoundsJob()
            {
                AgentPositions = _agentPositions,
                AgentAccelerations = _agentAccelerations,
                AreaSize = _areaSize,
                BoundsThreshold = _boundsThreshold,
                BoundsCompensationMultiplier = _boundsCompensationMultiplier,
                IsReproducingAgents = _isReproducingAgents
            };

            var accelerationJob = new AgentAccelerationJob()
            {
                AgentPositions = _agentPositions,
                AgentVelocities = _agentVelocities,
                AgentAccelerations = _agentAccelerations,
                AccelerationDestinationThreshold = _accelerationDestinationThreshold,
                Weights = _accelerationWeights,
                TargetPosition = _targetFood[0],
                IsReproducingAgents = _isReproducingAgents
            };

            var moveJob = new AgentMoveJob()
            {
                AgentPositions = _agentPositions,
                AgentVelocities = _agentVelocities,
                AgentAccelerations = _agentAccelerations,
                DeltaTime = Time.deltaTime,
                VelocityLimit = _currentVelocityLimit,
                IsReproducingAgents = _isReproducingAgents
            };
            var boundsHandle = boundsJob.Schedule(CurrentAgentCount, 0);
            var accelerationHandle = accelerationJob.Schedule(CurrentAgentCount, 0, boundsHandle);
            var moveHandle = moveJob.Schedule(_agentTransformAccessArray, accelerationHandle);
            moveHandle.Complete();

            for (int i = 0; i < CurrentAgentCount; i++)
            {
                _agentPositions[i] = _agentTransformAccessArray[i].position;
            }
        }

        private void SpawnAgents(int agentCount, Vector3 spawnPosition)
        {
            CreateOrResizeAgentsArrays(agentCount);

            for (int i = 0; i < agentCount; i++)
            {
                Agent unit = Instantiate(_agentPrefab, spawnPosition, Quaternion.LookRotation(Random.insideUnitSphere));
                unit.FlockController = this;
                _allAgents.Add(unit);
                AddAgentTransform(unit.transform);
                
                int index = CurrentAgentCount - agentCount + i;
                _agentPositions[index] = unit.transform.position;
                _agentVelocities[index] = Random.insideUnitSphere.normalized;
                _isReproducingAgents[index] = false;
                _agentAccelerations[index] = Vector3.zero;
            }

            UpdateFoodData(_allFood.Count);
        }

        private void AddAgentTransform(Transform unit)
        {
            if (_agentTransforms != null)
            {
                Transform[] newAgentTransforms = new Transform[_agentTransforms.Length + 1];
                for (int i = 0; i < _agentTransforms.Length; i++)
                {
                    newAgentTransforms[i] = _agentTransforms[i];
                }

                newAgentTransforms[newAgentTransforms.Length - 1] = unit;
                _agentTransforms = newAgentTransforms;
            }
            else
            {
                _agentTransforms = new Transform[1];
                _agentTransforms[0] = unit.transform;
            }
            
            if (_agentTransformAccessArray.isCreated)
            {
                TransformAccessArray newTransformAccessArray = new TransformAccessArray(_agentTransforms);
                _agentTransformAccessArray.Dispose();
                _agentTransformAccessArray = newTransformAccessArray;
            }
            else
            {
                _agentTransformAccessArray = new TransformAccessArray(_agentTransforms);
            }
        }

        private void CreateOrResizeAgentsArrays(int arraySize)
        {
            CurrentAgentCount += arraySize;
            
            if (_agentPositions.IsCreated)
            {
                Vector3[] newPositions = new Vector3[CurrentAgentCount];
                Vector3[] newVelocities = new Vector3[CurrentAgentCount];
                bool[] newIsMultiplying = new bool[CurrentAgentCount];
                Vector3[] newAccelerations = new Vector3[CurrentAgentCount];
                
                for (int i = 0; i < Mathf.Min(_agentPositions.Length, CurrentAgentCount); i++)
                {
                    newPositions[i] = _agentPositions[i];
                    newVelocities[i] = _agentVelocities[i];
                    newIsMultiplying[i] = _isReproducingAgents[i];
                    newAccelerations[i] = _agentAccelerations[i];
                }
            
                _agentPositions.Dispose();
                _agentVelocities.Dispose();
                _isReproducingAgents.Dispose();
                _agentAccelerations.Dispose();
                
                _agentPositions = new NativeArray<Vector3>(CurrentAgentCount, Allocator.Persistent);
                _agentVelocities = new NativeArray<Vector3>(CurrentAgentCount, Allocator.Persistent);
                _agentAccelerations = new NativeArray<Vector3>(CurrentAgentCount, Allocator.Persistent);
                _isReproducingAgents = new NativeArray<bool>(CurrentAgentCount, Allocator.Persistent);

                for (int i = 0; i < CurrentAgentCount; i++)
                {
                    _agentPositions[i] = newPositions[i];
                    _agentVelocities[i] = newVelocities[i];
                    _isReproducingAgents[i] = newIsMultiplying[i];
                    _agentAccelerations[i] = newAccelerations[i];
                }
            }
            else
            {
                _agentPositions = new NativeArray<Vector3>(CurrentAgentCount, Allocator.Persistent);
                _agentVelocities = new NativeArray<Vector3>(CurrentAgentCount, Allocator.Persistent);
                _agentAccelerations = new NativeArray<Vector3>(CurrentAgentCount, Allocator.Persistent);
                _isReproducingAgents = new NativeArray<bool>(CurrentAgentCount, Allocator.Persistent);
            }
        }

        private void OnDestroy()
        {
            _agentPositions.Dispose();
            _agentVelocities.Dispose();
            _agentAccelerations.Dispose();
            _isReproducingAgents.Dispose();
            _agentTransformAccessArray.Dispose();
            _foodPositions.Dispose();
            _foodActivityStates.Dispose();
            _targetFood.Dispose();
        }

        public void SpawnNewAgent(Vector3 spawnPosition)
        {
            SpawnAgents(1, spawnPosition);
            UpdateFoodData(_allFood.Count);
        }

        public void Reproduce(bool state, Agent unit)
        {
            int index = _allAgents.IndexOf(unit);
            _isReproducingAgents[index] = state;
        }
        
        public void UpdateFoodData(int foodCount)
        {
            CreateOrResizeFoodArrays(foodCount);
            
            for (int i = 0; i < foodCount; i++)
            {
                _foodPositions[i] = _allFood[i].transform.position;
                _foodActivityStates[i] = _allFood[i].ActivityState;
            }

            _targetFood[0] = Vector3.zero;
        }
        
        private void CreateOrResizeFoodArrays(int arraySize)
        {
            if (_foodPositions.IsCreated)
            {
                Vector3[] newFoodPositions = new Vector3[arraySize];
                bool[] newFoodActivityStates = new bool[arraySize];

                for (int i = 0; i < Mathf.Min(_foodPositions.Length, arraySize); i++)
                {
                    newFoodPositions[i] = _foodPositions[i];
                    newFoodActivityStates[i] = _foodActivityStates[i];
                }

                _foodPositions.Dispose();
                _foodActivityStates.Dispose();

                _foodPositions = new NativeArray<Vector3>(arraySize, Allocator.Persistent);
                _foodActivityStates = new NativeArray<bool>(arraySize, Allocator.Persistent);

                for (int i = 0; i < arraySize; i++)
                {
                    _foodPositions[i] = newFoodPositions[i];
                    _foodActivityStates[i] = newFoodActivityStates[i];
                }
            }
        }

        public void SetFlockSpeed(int flockSpeedFactor)
        {
            _currentVelocityLimit = _defaultVelocityLimit * flockSpeedFactor;
        }
    }
}