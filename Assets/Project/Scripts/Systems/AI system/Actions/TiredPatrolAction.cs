using Project.Scripts.Systems.AI_system.Core;
using UnityEngine;
using UnityEngine.AI; // Не забудь підключити, якщо використовуєш NavMesh

namespace Project.Scripts.Systems.AI_system.Actions
{
    public class TiredPatrolAction : GOAPAction // або твій базовий клас дії
    {
        [Header("Tired Patrol Settings")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float tiredSpeed = 1.5f;
        [SerializeField] private float normalSpeed = 3.5f; // Задай свою стандартну швидкість

        private NavMeshAgent navMeshAgent;

        public override void OnActivate()
        {
            base.OnActivate();

            // 1. Отримуємо NavMeshAgent з об'єкта (gameObject доступний у будь-якому MonoBehaviour)
            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
                if (navMeshAgent == null)
                {
                    navMeshAgent = GetComponentInParent<NavMeshAgent>();
                }
            }

            // 2. Змінюємо швидкість на втомлену
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = tiredSpeed;
            }

            // 3. Вибираємо точку для патруля
            if (waypoints != null && waypoints.Length > 0)
            {
                targetTransform = waypoints[Random.Range(0, waypoints.Length)];
            }
        }

        public override bool Perform(GameObject agentObj)
        {
            if (targetTransform == null)
            {
                Debug.LogWarning("TiredPatrolAction: Немає цілі для руху!");
                return true; 
            }

            float distance = Vector3.Distance(agentObj.transform.position, targetTransform.position);
            if (distance <= 1.5f)
            {
                return true; // Дія виконана
            }

            return false; // Ще йдемо
        }

        public override void OnDeactivate()
        {
            Debug.Log("Дивимся чи є navagent");
            base.OnDeactivate();

            // Повертаємо нормальну швидкість при виході з дії
            if (navMeshAgent != null)
            {
            Debug.Log("Міняємо швидкість назад");
                navMeshAgent.speed = normalSpeed;
            }
        }
    }
}