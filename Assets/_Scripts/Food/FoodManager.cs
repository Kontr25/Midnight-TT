using _Scripts.Boids;
using _Scripts.PoolObject;
using UnityEngine;

namespace _Scripts.Food
{
    public class FoodManager : MonoBehaviour
    {
        [SerializeField] private float _defaultReproductionDuration;
        [SerializeField] private int _maxFoodCount;
        [SerializeField] private Food _foodPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _container;
        [SerializeField] private int _pollCapacity;
        [SerializeField] private FlockController flockController;
        
        private Pool<Food> _pool;
        private Vector3 _areaSize;
        private int _lastFoodCount;
        private Transform[] _foodTransforms;
        private float _currentReproductionDuration;

        public int FoodCount
        {
            get
            {
                int foodCount = 0;
                for (int i = 0; i < _pool.PoolList.Count; i++)
                {
                    if (_pool.PoolList[i].ActivityState)
                    {
                        foodCount++;
                    }
                }

                return foodCount;
            }
        }

        public Vector3 AreaSize
        {
            get => _areaSize;
            set => _areaSize = value;
        }

        public float ReproductionDuration
        {
            get => _currentReproductionDuration;
            set => _currentReproductionDuration = value;
        }

        private void Awake()
        {
            _pool = new Pool<Food>(_foodPrefab, _pollCapacity, _container)
            {
                AutoExpand = true
            };

            flockController.AllFood = _pool.PoolList;
        }

        public void StartGame()
        {
            _currentReproductionDuration = _defaultReproductionDuration;
            for (int i = 0; i < _maxFoodCount; i++)
            {
                SpawnFood();
            }
        }

        private void SpawnFood()
        {
            var food = _pool.GetFreeElement();
            food.FoodManager = this;
            food.transform.position = RandomPosition();
            flockController.AllFood = _pool.PoolList;
        }
        
        private Vector3 RandomPosition()
        {
            Vector3 randomPosition = new Vector3(Random.Range(-AreaSize.x / 2, AreaSize.x / 2),
                Random.Range(-AreaSize.y / 2, AreaSize.y / 2),
                Random.Range(-AreaSize.z / 2, AreaSize.z / 2));
            return randomPosition;
        }

        public void EatFood(Vector3 spawnPosition)
        {
            SpawnFood();
            flockController.SpawnNewAgent(spawnPosition);
        }

        public void SetReproductionSpeed(int reproductionSpeedFactor)
        {
            _currentReproductionDuration = _defaultReproductionDuration / reproductionSpeedFactor;
        }
        
        public void AddFood()
        {
            SpawnFood();
            flockController.UpdateFoodData(_pool.PoolList.Count);
        }
    }
}