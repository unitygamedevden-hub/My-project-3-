using Project.Scripts.Systems.AI_system.Core;
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

            // Якщо точок на тілі немає, виходимо, щоб не було помилки
            if (customTargetPoints == null || customTargetPoints.Length == 0) return;

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

                    // Пускаємо рейкасти в кожну задану точку на тілі цілі
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
                            _lastDebugRayHits[i] = false; // Перешкода заблокувала промінь
                        }
                        else
                        {
                            _lastDebugHitPoints[i] = targetPoint;
                            _lastDebugRayHits[i] = true;  // Промінь успішно дійшов до точки
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

            // Оновлюємо стан цілі в пам'яті агента
            if (bestTarget != null)
            {
                currentVisibleTarget = bestTarget;
                if (_agent != null)
                {
                    _agent.Memory.SetState("HasTarget", true);
                }
            }
            else
            {
                if (currentVisibleTarget != null)
                {
                    currentVisibleTarget = null;
                    if (_agent != null)
                    {
                        _agent.Memory.SetState("HasTarget", false);
                    }
                }
                _lastDebugRayOrigins = null;
                _lastDebugHitPoints = null;
                _lastDebugRayHits = null;
            }
        }

        public Transform GetCurrentVisibleTarget()
        {
            return currentVisibleTarget;
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

            // Візуалізація променів до кастомних точок тіла
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