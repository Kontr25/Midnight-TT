using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Food
{
    public class Food : MonoBehaviour
    {
        [SerializeField] private int _requiredAgentsForReproduction;
        [SerializeField] private Transform _modelTransform;
        [SerializeField] private float _appearDuration;
        [SerializeField] private Ease _appearEase;
        [SerializeField] private Collider _mainCollider;

        private FoodManager _foodManager;
        private Sequence _sequence;
        private List<Agent> _currentAgentForReproduction = new List<Agent>();
        private bool _activityState;

        private float ReproductionDuration => _foodManager.ReproductionDuration;

        public FoodManager FoodManager
        {
            get => _foodManager;
            set => _foodManager = value;
        }

        public bool ActivityState => _activityState;

        private void OnEnable()
        {
            _mainCollider.enabled = true;
            _currentAgentForReproduction.Clear();
            _activityState = true;
            ResetAndCreateSequence();
            _sequence.Append(_modelTransform.DOScale(Vector3.one, _appearDuration)).SetEase(_appearEase).onComplete = () =>
            {
                _modelTransform.localScale = Vector3.one;
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Agent unit) 
                && !_currentAgentForReproduction.Contains(unit)
                && _currentAgentForReproduction.Count < _requiredAgentsForReproduction)
            {
                unit.Multiply(true);
                _currentAgentForReproduction.Add(unit);
                
                TryEatFood();
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Agent unit) && _currentAgentForReproduction.Contains(unit))
            {
                unit.Multiply(false);
                _currentAgentForReproduction.Remove(unit);
            }
        }

        private void TryEatFood()
        {
            if (_currentAgentForReproduction.Count < _requiredAgentsForReproduction)
            {
                return;
            }
            
            _mainCollider.enabled = false;
            
            ResetAndCreateSequence();
            _sequence.Append(_modelTransform.DOScale(Vector3.zero, ReproductionDuration)).onComplete = () =>
            {
                for (int i = 0; i < _currentAgentForReproduction.Count; i++)
                {
                    _currentAgentForReproduction[i].Multiply(false);
                }
                _activityState = false;
                FoodManager.EatFood(transform.position);
                gameObject.SetActive(false);
            };
        }

        private void OnDisable()
        {
            _activityState = false;
            _modelTransform.transform.localScale = Vector3.zero;
        }

        private void ResetAndCreateSequence()
        {
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }
            _sequence = DOTween.Sequence();
        }
    }
}