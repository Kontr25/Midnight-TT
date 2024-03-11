using _Scripts.Boids;
using UnityEngine;

namespace _Scripts
{
    public class Agent : MonoBehaviour
    {
        private FlockController _flockController;

        public FlockController FlockController
        {
            get => _flockController;
            set => _flockController = value;
        }

        public void Multiply(bool state)
        {
            _flockController.Reproduce(state,this);
        }
    }
}