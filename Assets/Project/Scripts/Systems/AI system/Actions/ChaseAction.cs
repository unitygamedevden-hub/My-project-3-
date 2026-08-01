using Project.Scripts.Systems.AI_system.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Systems.AI_system.Actions
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AgentVision))]
    public class ChaseAction : GOAPAction
    {
        private NavMeshAgent _navAgent;
        private AgentVision _agentVision;
        private Animator _animator;

        [SerializeField] private float chaseSpeed = 4.5f;

        protected override void Awake()
        {
            base.Awake(); // Обов'язково викликаємо базовий Awake(), щоб заповнились Preconditions та Effects з Інспектора!
            
            _navAgent = GetComponent<NavMeshAgent>();
            _agentVision = GetComponent<AgentVision>();
            _animator = GetComponentInChildren<Animator>();
        }

        public override bool CheckProceduralPrecondition(GameObject agentGO)
        {
            // Перевіряємо, чи є видима ціль перед початком дії
            Transform target = _agentVision.GetCurrentVisibleTarget();
            return target != null;
        }

        public override void OnActivate()
        {
            _navAgent.speed = chaseSpeed;
            _navAgent.stoppingDistance = stoppingDistance;

            if (_animator != null)
            {
                _animator.SetBool("IsRunning", true);
            }
        }

        public override bool Perform(GameObject agentGO)
        {
            // Якщо ціль зникла з поля зору — перериваємо дію
            Transform target = _agentVision.GetCurrentVisibleTarget();
            if (target == null)
            {
                return false; // Повертаємо false, щоб планувальник перерахував шлях
            }

            // Оновлюємо пункт призначення за рухомою ціллю
            _navAgent.SetDestination(target.position);

            // Якщо дійшли на дистанцію зупинки — дія вважається виконаною
            if (!_navAgent.pathPending && _navAgent.remainingDistance <= stoppingDistance)
            {
                return true; 
            }

            return false; // Дія ще триває (переслідування активне)
        }

        public override void OnDeactivate()
        {
            if (_navAgent.isActiveAndEnabled)
            {
                _navAgent.ResetPath();
            }

            if (_animator != null)
            {
                _animator.SetBool("IsRunning", false);
            }
        }
    }
}