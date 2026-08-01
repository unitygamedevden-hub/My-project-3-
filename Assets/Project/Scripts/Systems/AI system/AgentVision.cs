using Project.Scripts.Systems.AI_system.Core;
using Project.Scripts.Systems.AI_system.Core.Project.Scripts.Systems.AI_system.Core;
using UnityEngine;

namespace Project.Scripts.Systems.AI_system
{
    public class AgentVision : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float viewRadius = 10f;          // Радіус огляду
        [Range(0f, 360f)]
        [SerializeField] private float viewAngle = 90f;           // Кут огляду (у градусах)
        [SerializeField] private LayerMask targetMask;            // Що саме шукаємо (наприклад, Layer "Player")
        [SerializeField] private LayerMask obstructionMask;       // Перешкоди (стіни, перепони)

        [Header("Target Body Parts (Raycast Targets)")]
        [Tooltip("Перетягни сюди точки з префаба цілі: голова, руки, корпус, ноги тощо.")]
        [SerializeField] private Transform[] customTargetPoints;  // Список твоїх точок на тілі

        [Header("Memory & Investigation")]
        [SerializeField] private float memoryDuration = 30f;      // Час пам'яті про ціль після її зникнення (30 секунд)
        private float _timeSinceLastSeen = 0f;
        private Vector3 _lastKnownTargetPosition;                 // Останні відомі координати цілі

        [Header("Target Reference")]
        [SerializeField] private Transform currentVisibleTarget;  // Поточна помічена ціль
        
        private GoapAgent _agent;
        
        // Зберігаємо дані для дебагу
        private Vector3[] _lastDebugRayOrigins;
        private Vector3[] _lastDebugHitPoints; 
        private bool[] _lastDebugRayHits;

        private void Awake()
        {
            _agent = GetComponent<GoapAgent>();
        }

        private void Update()
        {
            ScanForTargets();
        }

        private void ScanForTargets()
        {
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

            Transform bestTarget = null;
            float minDistance = Mathf.Infinity;

            if (customTargetPoints != null && customTargetPoints.Length > 0)
            {
                int totalPoints = customTargetPoints.Length;

                foreach (var targetCollider in targetsInRadius)
                {
                    Transform target = targetCollider.transform;
                    Vector3 directionToTarget = (target.position - transform.position).normalized;
            
                    directionToTarget.y = 0;
                    Vector3 forward = transform.forward;
                    forward.y = 0;

                    if (Vector3.Angle(forward, directionToTarget) < viewAngle / 2f)
                    {
                        float distanceToTarget = Vector3.Distance(transform.position, target.position);
                        bool isVisible = false;

                        Vector3 agentEyePosition = transform.position + Vector3.up * 1.5f;

                        _lastDebugRayOrigins = new Vector3[totalPoints];
                        _lastDebugHitPoints = new Vector3[totalPoints];
                        _lastDebugRayHits = new bool[totalPoints];

                        for (int i = 0; i < totalPoints; i++)
                        {
                            if (customTargetPoints[i] == null) continue;

                            Vector3 targetPoint = customTargetPoints[i].position;
                            Vector3 dirToPoint = (targetPoint - agentEyePosition).normalized;
                            float distToPoint = Vector3.Distance(agentEyePosition, targetPoint);

                            _lastDebugRayOrigins[i] = agentEyePosition;

                            RaycastHit hit;
                            bool hitObstruction = Physics.Raycast(agentEyePosition, dirToPoint, out hit, distToPoint, obstructionMask);

                            if (hitObstruction)
                            {
                                _lastDebugHitPoints[i] = hit.point;
                                _lastDebugRayHits[i] = false; 
                            }
                            else
                            {
                                _lastDebugHitPoints[i] = targetPoint;
                                _lastDebugRayHits[i] = true; 
                                isVisible = true;
                            }
                        }

                        if (isVisible)
                        {
                            if (distanceToTarget < minDistance)
                            {
                                minDistance = distanceToTarget;
                                bestTarget = target;
                            }
                        }
                    }
                }
            }

            // --- ОБРОБКА ПАМ'ЯТІ ТА СТАНІВ АГЕНТА ---
            if (_agent != null)
            {
                if (bestTarget != null)
                {
                    // Ціль знайдено і вона на очних точках
                    currentVisibleTarget = bestTarget;
                    _lastKnownTargetPosition = bestTarget.position;
                    _timeSinceLastSeen = 0f;

                    // Оновлюємо пам'ять через enum WorldKeys
                    _agent.Memory.SetState(WorldKeys.HasTarget.ToString(), true);
                    _agent.Memory.SetState(WorldKeys.KnowsTargetLocation.ToString(), true);
                    _agent.Memory.SetState(WorldKeys.IsInvestigating.ToString(), false);
                }
                else
                {
                    // Ціль втрачено з поля зору
                    currentVisibleTarget = null;
                    _agent.Memory.SetState(WorldKeys.HasTarget.ToString(), false);

                    // Перевіряємо, чи діє наша пам'ять про останню позицію (до 30 секунд)
                    object knowsLocObj = _agent.Memory.GetState(WorldKeys.KnowsTargetLocation);
                    bool knowsLocation = knowsLocObj is bool val && val;

                    if (knowsLocation)
                    {
                        _timeSinceLastSeen += Time.deltaTime;

                        if (_timeSinceLastSeen <= memoryDuration)
                        {
                            // Агент переходить у стан розслідування/пошуку у відомій точці
                            _agent.Memory.SetState(WorldKeys.IsInvestigating.ToString(), true);
                        }
                        else
                        {
                            // Час вийшов (30 секунд минуло) — повністю забуваємо ціль
                            _agent.Memory.SetState(WorldKeys.KnowsTargetLocation.ToString(), false);
                            _agent.Memory.SetState(WorldKeys.IsInvestigating.ToString(), false);
                        }
                    }

                    _lastDebugRayOrigins = null;
                    _lastDebugHitPoints = null;
                    _lastDebugRayHits = null;
                }
            }
        }

        public Transform GetCurrentVisibleTarget()
        {
            return currentVisibleTarget;
        }

        public Vector3 GetLastKnownPosition()
        {
            return _lastKnownTargetPosition;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewRadius);

            Vector3 forward = transform.forward;
            forward.y = 0;

            Quaternion leftRot = Quaternion.Euler(0, -viewAngle / 2f, 0);
            Quaternion rightRot = Quaternion.Euler(0, viewAngle / 2f, 0);

            Vector3 leftDir = leftRot * forward;
            Vector3 rightDir = rightRot * forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, leftDir * viewRadius);
            Gizmos.DrawRay(transform.position, rightDir * viewRadius);

            if (_lastDebugRayOrigins != null && _lastDebugHitPoints != null)
            {
                for (int i = 0; i < _lastDebugRayOrigins.Length; i++)
                {
                    bool isHit = _lastDebugRayHits[i];

                    Gizmos.color = isHit ? Color.green : Color.red;
                    Gizmos.DrawLine(_lastDebugRayOrigins[i], _lastDebugHitPoints[i]);
                    Gizmos.DrawSphere(_lastDebugHitPoints[i], 0.08f);
                }
            }
        }
    }
}