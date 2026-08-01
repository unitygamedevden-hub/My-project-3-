using Project.Scripts.Systems.AI_system.Core;
using Project.Scripts.Systems.AI_system.Core.Project.Scripts.Systems.AI_system.Core;
using UnityEngine;

namespace Project.Scripts.Systems.AI_system.Needs
{
    public class SleepNeed : MonoBehaviour
    {
        public GoapAgent agent;
        public float currentValue = 0f;

        [Header("Thresholds (Порогові значення)")]
        public float tiredThreshold = 60f;     // Початок втомленого патруля
        public float sleepThreshold = 85f;     // Потреба йти спати

        private void Update()
        {
            // Накопичення втоми (наприклад)
            currentValue += Time.deltaTime * 3f;

            if (agent != null && agent.Memory != null)
            {
                // Рівень 1: Вже втомлений ( >= 60 )
                bool isTired = currentValue >= tiredThreshold;
                agent.Memory.SetState(WorldKeys.IsTired, isTired);

                // Рівень 2: Повністю виснажений, критичний стан ( >= 85 )
                bool isExhausted = currentValue >= sleepThreshold;
                agent.Memory.SetState(WorldKeys.IsExhausted, isExhausted);
            }
        }
    }
}