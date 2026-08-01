using Project.Scripts.Systems.AI_system.Core;
using Project.Scripts.Systems.AI_system.Core.Project.Scripts.Systems.AI_system.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Systems.AI_system.Actions
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AgentVision))]
    public class AttackAction : GOAPAction
    {
        private NavMeshAgent _navAgent;
        private AgentVision _agentVision;
        private Animator _animator;
        private GoapAgent _goapAgent; // Кешуємо посилання на агента

        [SerializeField] private float attackRange = 2f; 
        [SerializeField] private float attackCooldown = 1.5f; 
        private float _nextAttackTime = 0f;

        protected override void Awake()
        {
            base.Awake();
            
            requiresInRange = false; 

            _navAgent = GetComponent<NavMeshAgent>();
            _agentVision = GetComponent<AgentVision>();
            _animator = GetComponentInChildren<Animator>();
            _goapAgent = GetComponent<GoapAgent>(); // Отримуємо компонент агента
        }

        public override bool CheckProceduralPrecondition(GameObject agentGO)
        {
            Transform target = _agentVision.GetCurrentVisibleTarget();
            return target != null;
        }

        public override void OnActivate()
        {
            if (_navAgent.isActiveAndEnabled)
            {
                _navAgent.ResetPath();
                _navAgent.isStopped = true;
            }
        }

        public override bool Perform(GameObject agentGO)
        {
            Transform target = _agentVision.GetCurrentVisibleTarget();
            
            // Безпечно перевіряємо пам'ять через кешований _goapAgent
            bool hasTarget = false;
            if (_goapAgent != null && _goapAgent.Memory.GetState(WorldKeys.HasTarget) is bool val)
            {
                hasTarget = val;
            }

            if (target == null || !hasTarget)
            {
                return true; // Якщо ціль зникла — дія завершується
            }

            // Повертаємося обличчям до цілі
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Перевіряємо дистанцію до цілі
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > attackRange + 0.5f)
            {
                return true; 
            }

            // Виконуємо атаку за кулдауном
            if (Time.time >= _nextAttackTime)
            {
                _nextAttackTime = Time.time + attackCooldown;
                
                Debug.Log($"<color=red>[GOAP] Агент атакує ціль! (Лог-атака)</color>");

                if (_animator != null)
                {
                    _animator.SetTrigger("Attack");
                }
            }

            return false; 
        }

        public override void OnDeactivate()
        {
            if (_navAgent.isActiveAndEnabled)
            {
                _navAgent.isStopped = false;
            }
        }
    }
}