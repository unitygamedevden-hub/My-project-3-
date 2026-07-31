using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Systems.AI_system.Core
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GOAPAgent : MonoBehaviour
    {
        [Header("Agent Data")]
        public WorldState Memory = new WorldState();
        public List<GOAPGoal> AvailableGoals;
        
        // Змінні для внутрішнього стану
        private List<GOAPAction> _availableActions;
        private Queue<GOAPAction> _actionQueue;
        private GOAPAction _currentAction;
        private GOAPGoal _currentGoal;
        
        // --- ВЛАСТИВОСТІ ДЛЯ UI (Саме їх не вистачало) ---
        public string CurrentStateName => _currentState.ToString();
        public GOAPGoal CurrentGoal => _currentGoal;
        public GOAPAction CurrentAction => _currentAction;
        // ------------------------------------------------
        
        // Компоненти
        public NavMeshAgent NavAgent { get; private set; }
        public Animator AgentAnimator { get; private set; }
        private GOAPPlanner _planner;

        // FSM (Стани агента)
        private enum AgentState { Idle, MovingToTarget, PerformingAction }
        private AgentState _currentState = AgentState.Idle;

        private void Start()
        {
            NavAgent = GetComponent<NavMeshAgent>();
            AgentAnimator = GetComponentInChildren<Animator>();
            _planner = new GOAPPlanner();
            
            // Знаходимо всі дії (GOAPAction), які висять на цьому об'єкті
            _availableActions = GetComponents<GOAPAction>().ToList();
            
            // --- ДОДАЙ ЦЕЙ БЛОК ІНІЦІАЛІЗАЦІЇ ---
            Memory.SetState("IsExhausted", false); // Початково агент не втомлений
            Memory.SetState("AtWaypoint", false);
        }

        private void Update()
        {
            // Оновлюємо анімацію швидкості (щоб Animator знав, чи ми біжимо)
            // Припускаємо, що в Аніматорі є параметр "Speed" типу Float
            AgentAnimator.SetFloat("Speed", NavAgent.velocity.magnitude);

            switch (_currentState)
            {
                case AgentState.Idle:
                    UpdateIdleState();
                    break;
                case AgentState.MovingToTarget:
                    UpdateMovingState();
                    break;
                case AgentState.PerformingAction:
                    UpdatePerformingState();
                    break;
            }
        }

        // --- ЛОГІКА СТАНІВ (FSM) ---

        private void UpdateIdleState()
        {
            if (_actionQueue == null || _actionQueue.Count == 0)
            {
                CalculatePlan();
                return;
            }

            _currentAction = _actionQueue.Dequeue();

            if (_currentAction.CheckProceduralPrecondition(gameObject))
            {
                // Викликаємо OnActivate ОДИН РАЗ при старті дії
                _currentAction.OnActivate(); 

                if (_currentAction.requiresInRange)
                {
                    _currentState = AgentState.MovingToTarget;
                    
                    // --- НОВИЙ КОД ДЛЯ РУХУ ---
                    if (_currentAction.targetTransform != null)
                    {
                        NavAgent.SetDestination(_currentAction.targetTransform.position);
                    }
                    else
                    {
                        Debug.LogWarning($"[GOAP] Дія {_currentAction.GetType().Name} вимагає підійти, але targetTransform порожній!");
                    }
                    // ---------------------------
                }
                else
                {
                    _currentState = AgentState.PerformingAction;
                }
            }
            else
            {
                _actionQueue.Clear();
                _currentState = AgentState.Idle;
            }
        }

        private void UpdateMovingState()
        {
            if (_currentAction == null || _currentAction.targetTransform == null) return;

            Vector3 targetPosition = _currentAction.targetTransform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Ігноруємо вертикаль, щоб агент не задирав голову

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
        
                // 1. Якщо ми ще не дивимося на ціль (наприклад, кут більше 5 градусів)
                if (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
                {
                    // Вимикаємо стандартний поворот NavMeshAgent, щоб він не їхав одночасно
                    NavAgent.updateRotation = false;
                    NavAgent.isStopped = true; // Зупиняємо рух на місці

                    // Плавне розвертання на місці
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, 
                        targetRotation, 
                        NavAgent.angularSpeed * Time.deltaTime
                    );
                    return; // Поки не розгорнемося — далі не їдемо
                }
            }

            // 2. Коли розгорнулися — вмикаємо рух і стандартний поворот назад
            NavAgent.isStopped = false;
            NavAgent.updateRotation = true;
            NavAgent.SetDestination(targetPosition);

            // Перевірка на прибуття до точки
            if (!NavAgent.pathPending && NavAgent.remainingDistance <= NavAgent.stoppingDistance)
            {
                _currentState = AgentState.PerformingAction;
            }
        }
        private void UpdatePerformingState()
        {
            // Виконуємо дію. Якщо вона повертає true - вона завершена
            bool isComplete = _currentAction.Perform(gameObject);

            if (isComplete)
            {
                // --- ДОДАЙ ЦЕЙ ВИКЛИК ---
                if (_currentAction != null)
                {
                    _currentAction.OnDeactivate();
                }
                // ------------------------

                // Накладаємо ефекти дії на нашу пам'ять (світ змінився)
                Memory.ApplyState(_currentAction.Effects);
                
                _currentAction = null; // Очищаємо поточну дію
                _currentState = AgentState.Idle; // Переходимо в Idle, щоб взяти наступну дію
            }
        }

        // --- МЕТОД ПЛАНУВАННЯ ---

        private void CalculatePlan()
        {
            // Шукаємо найпріоритетнішу ціль, яку зараз можна виконати
            GOAPGoal bestGoal = null;
            int highestPriority = int.MinValue;

            foreach (var goal in AvailableGoals)
            {
                if (goal.CanBeActivated(Memory))
                {
                    int currentPriority = goal.GetPriority(Memory);
                    if (currentPriority > highestPriority)
                    {
                        highestPriority = currentPriority;
                        bestGoal = goal;
                    }
                }
            }

            if (bestGoal != null)
            {
                _currentGoal = bestGoal;
                // Запускаємо мозок!
                _actionQueue = _planner.Plan(gameObject, _availableActions, Memory, bestGoal);

                if (_actionQueue != null && _actionQueue.Count > 0)
                {
                    Debug.Log($"<color=green>Знайдено план для цілі: {bestGoal.goalName}</color>");
                }
                else
                {
                    Debug.Log($"<color=red>Не вдалося побудувати план для: {bestGoal.goalName}</color>");
                }
            }
        }
    }
}