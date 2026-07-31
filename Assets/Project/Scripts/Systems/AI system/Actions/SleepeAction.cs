using UnityEngine;
using Project.Scripts.Systems.AI_system.Core;
using Project.Scripts.Systems.AI_system.Needs;

namespace Project.Scripts.Systems.AI_system.Actions
{
    public class SleepAction : GOAPAction
    {
        [Header("Sleep Settings")]
        public float sleepDuration = 3f; // Скільки секунд агент буде спати
        private float _sleepTimer = 0f;
        
        private SleepNeed _sleepNeed;

        protected override void Awake()
        {
            // ВАЖЛИВО! Спочатку викликаємо Awake з базового класу, 
            // щоб він створив WorldState і переніс туди дані з Інспектора.
            base.Awake(); 

            _sleepNeed = GetComponent<SleepNeed>();
        }

        // Тут ми можемо скинути таймер перед початком сну
        public override void OnActivate()
        {
            _sleepTimer = 0f;
            Debug.Log("Агент лягає спати...");
        }

        public override bool Perform(GameObject agent)
        {
            _sleepTimer += Time.deltaTime;

            if (_sleepNeed != null)
            {
                _sleepNeed.currentValue -= 35f * Time.deltaTime;
                _sleepNeed.currentValue = Mathf.Max(0, _sleepNeed.currentValue);
            }

            if (_sleepTimer >= sleepDuration)
            {
                Debug.Log("<color=cyan>Агент виспався!</color>");
                return true; 
            }

            return false; 
        }
    }
}