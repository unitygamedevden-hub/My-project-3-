using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Systems.AI_system.Core;

namespace Project.Scripts.Systems.AI_system.Actions
{
    public class PatrolAction : GOAPAction
    {
        [Header("Patrol Settings")]
        public List<Transform> waypoints; // Точки патрулювання

        protected override void Awake()
        {
            base.Awake();
            
            // Дія вимагає наближення до цілі
            requiresInRange = true; 
            actionName = "Патрулювання";
        }

        public override void OnActivate()
        {
            // Обираємо випадкову точку для патрулювання
            if (waypoints != null && waypoints.Count > 0)
            {
                targetTransform = waypoints[Random.Range(0, waypoints.Count)];
            }
        }

        public override bool Perform(GameObject agent)
        {
            // Перевіряємо відстань до цілі (наприклад, менше 1.5 метра)
            float distance = Vector3.Distance(agent.transform.position, targetTransform.position);
            if (distance <= 1.5f)
            {
                return true; // Ми дійшли до точки! Дія Patrol завершена.
            }

            return false; // Ще йдемо
        }
    }
}