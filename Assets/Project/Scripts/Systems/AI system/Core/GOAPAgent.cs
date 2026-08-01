using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Systems.AI_system.Core.Project.Scripts.Systems.AI_system.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Project.Scripts.Systems.AI_system.Core
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GoapAgent : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");

        [Header("Agent Data")]
        public readonly WorldState Memory = new WorldState();
        [FormerlySerializedAs("AvailableGoals")] public List<GOAPGoal> availableGoals;
        
        // Змінні для внутрішнього стану
        private List<GOAPAction> _availableActions;
        private Queue<GOAPAction> _actionQueue;
        private GOAPAction _currentAction;
        private GOAPGoal _currentGoal;
        
        // Кеш для відстеження змін зору та уникнення спаму переривань
        private bool _wasTargetVisible;
        
        // --- ВЛАСТИВОСТІ ДЛЯ UI ---
        public string CurrentStateName => _currentState.ToString();
        public GOAPGoal CurrentGoal => _currentGoal;
        public GOAPAction CurrentAction => _currentAction;
        // -------------------------
        
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
            
            // Початкові стани пам'яті
            Memory.SetState(WorldKeys.HasTarget.ToString(), false);
            Memory.SetState("IsExhausted", false);
            Memory.SetState("AtWaypoint", false);
        }

        private void Update()
        {
            if (AgentAnimator != null)
            {
                AgentAnimator.SetFloat(Speed, NavAgent.velocity.magnitude);
            }

            // Безпечно перевіряємо стан HasTarget з пам'яті агента
            bool isTargetVisible = false;
            object targetState = Memory.GetState(WorldKeys.HasTarget);
            if (targetState is bool targetVal)
            {
                isTargetVisible = targetVal;
            }

            // --- ОДНОРАЗОВЕ РЕАКТИВНЕ ПЕРЕРИВАННЯ ---
            // Спрацьовує рівно один раз, коли агент вперше помічає ціль
            if (isTargetVisible && !_wasTargetVisible)
            {
                if (_currentGoal == null || _currentGoal.goalName != "Переслідування")
                {
                    Debug.Log("<color=orange>[GOAP] Помічено ціль! Перериваємо поточну рутину для погоні.</color>");
                    InterruptAction();
                }
            }
            _wasTargetVisible = isTargetVisible;
            // ----------------------------------------

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
                _currentAction.OnActivate(); 

                if (_currentAction.requiresInRange)
                {
                    _currentState = AgentState.MovingToTarget;
                    
                    if (_currentAction.targetTransform != null)
                    {
                        NavAgent.SetDestination(_currentAction.targetTransform.position);
                    }
                    else
                    {
                        Debug.LogWarning($"[GOAP] Дія {_currentAction.GetType().Name} вимагає підійти, але targetTransform порожній!");
                    }
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
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
        
                if (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
                {
                    NavAgent.updateRotation = false;
                    NavAgent.isStopped = true; 

                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, 
                        targetRotation, 
                        NavAgent.angularSpeed * Time.deltaTime
                    );
                    return; 
                }
            }

            NavAgent.isStopped = false;
            NavAgent.updateRotation = true;
            NavAgent.SetDestination(targetPosition);

            if (!NavAgent.pathPending && NavAgent.remainingDistance <= NavAgent.stoppingDistance)
            {
                _currentState = AgentState.PerformingAction;
            }
        }

        private void UpdatePerformingState()
        {
            bool isComplete = _currentAction.Perform(gameObject);

            if (isComplete)
            {
                if (_currentAction != null)
                {
                    _currentAction.OnDeactivate();
                }

                Memory.ApplyState(_currentAction.Effects);
                
                _currentAction = null; 
                _currentState = AgentState.Idle; 
            }
        }

        // --- МЕТОД ПЕРЕРИВАННЯ ---

        private void InterruptAction()
        {
            if (_currentAction != null)
            {
                _currentAction.OnDeactivate();
            }

            _currentAction = null;
            _currentGoal = null;
            
            if (_actionQueue != null)
            {
                _actionQueue.Clear();
            }
            
            if (NavAgent.isActiveAndEnabled)
            {
                NavAgent.ResetPath();
            }
            
            _currentState = AgentState.Idle; 
        }

        // --- МЕТОД ПЛАНУВАННЯ ---

        private void CalculatePlan()
        {
            GOAPGoal bestGoal = null;
            int highestPriority = int.MinValue;

            foreach (var goal in availableGoals)
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