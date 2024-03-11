using _Scripts.Boids;
using _Scripts.Food;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class ParametersManager: MonoBehaviour
    {
        [SerializeField] private Slider _reprodutionDurationSlider;
        [SerializeField] private Slider _flockSpeedSlider;
        [SerializeField] private TMP_Text _reprodutionSpeedText;
        [SerializeField] private TMP_Text _flockSpeedText;
        [SerializeField] private FoodManager _foodManager;
        [SerializeField] private FlockController flockController;

        private int _reproductionSpeedFactor;
        private int _flockSpeedFactor;

        private void Start()
        {
            _reprodutionDurationSlider.onValueChanged.AddListener(delegate { ReprodutionDurationChangeCheck();});
            _flockSpeedSlider.onValueChanged.AddListener(delegate { FlockSpeedChangeCheck();});
        }

        private void OnDestroy()
        {
            _reprodutionDurationSlider.onValueChanged.RemoveListener(delegate { ReprodutionDurationChangeCheck();});
            _flockSpeedSlider.onValueChanged.RemoveListener(delegate { FlockSpeedChangeCheck();});
        }

        private void ReprodutionDurationChangeCheck()
        {
            if (_reprodutionDurationSlider.value <= 0.1f)
            {
                _reproductionSpeedFactor = 1;
            }
            else
            {
                _reproductionSpeedFactor = Mathf.RoundToInt(_reprodutionDurationSlider.value * 10);
            }

            _foodManager.SetReproductionSpeed(_reproductionSpeedFactor);
            _reprodutionSpeedText.text = $"x{_reproductionSpeedFactor}";
        }
        
        private void FlockSpeedChangeCheck()
        {
            if (_flockSpeedSlider.value <= 0.1f)
            {
                _flockSpeedFactor = 1;
            }
            else
            {
                _flockSpeedFactor = Mathf.RoundToInt(_flockSpeedSlider.value * 10);
            }

            flockController.SetFlockSpeed(_flockSpeedFactor);
            _flockSpeedText.text = $"x{_flockSpeedFactor}";
        }
    }
}