using UnityEngine;
using Project.Scripts.Systems.AI_system.Core;

namespace Project.Scripts.Systems.AI_system.Actions
{
    public class WaitAction : GOAPAction
    {
        [Header("Wait Settings")]
        public float waitDuration = 2.0f; // Скільки секунд стояти
        private float _timer;

        protected override void Awake()
        {
            base.Awake();
            actionName = "Очікування"; // Це буде відображатися в UI
        }

        public override void OnActivate()
        {
            base.OnActivate();
            _timer = waitDuration;
            Debug.Log("Агент зупинився і оглядається...");
        }

        public override bool Perform(GameObject agent)
        {
            _timer -= Time.deltaTime;
            
            // Якщо час вийшов, дія завершена
            return _timer <= 0f;
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Debug.Log("Очікування завершено, повертаюся до справ.");
        }
    }
}

