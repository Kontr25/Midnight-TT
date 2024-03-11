using TMPro;
using UnityEngine;

namespace _Scripts.UI
{
    public class AgentCounter: MonoBehaviour
    {
        [SerializeField] private TMP_Text _agentCountText;

        public void UpdateAgentCount(int agentCount)
        {
            _agentCountText.text = $"Agent count: {agentCount}";
        }
    }
}