using _Scripts.Boids;
using _Scripts.Food;
using UnityEngine;

namespace _Scripts
{
    public class GameInitializer: MonoBehaviour
    {
        [SerializeField] private Vector3 _areaSize;
        [SerializeField] private FoodManager _foodManager;
        [SerializeField] private FlockController _flockController;

        private void Start()
        {
            _foodManager.AreaSize = _areaSize;
            _flockController.AreaSize = _areaSize;
            _foodManager.StartGame();
            _flockController.StartGame();
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Vector3.zero, _areaSize);
        }
    }
}