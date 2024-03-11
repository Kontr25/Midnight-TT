using _Scripts.Food;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class FoodCountController: MonoBehaviour
    {
        [SerializeField] private Button _addFoodButton;
        [SerializeField] private TMP_Text _allFoodCount;
        [SerializeField] private FoodManager _foodManager;

        private void Start()
        {
            UpdateFoodCount();
            _addFoodButton.onClick.AddListener(AddFood);
        }

        private void OnDestroy()
        {
            _addFoodButton.onClick.RemoveListener(AddFood);
        }

        private void AddFood()
        {
            _foodManager.AddFood();
            UpdateFoodCount();
        }

        private void UpdateFoodCount()
        {
            _allFoodCount.text = $"Food count = {_foodManager.FoodCount}";
        }
    }
}