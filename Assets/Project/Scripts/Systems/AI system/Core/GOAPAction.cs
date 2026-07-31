using System.Collections.Generic;
using Project.Scripts.Systems.AI_system.Core.Project.Scripts.Systems.AI_system.Core;
using UnityEngine;

namespace Project.Scripts.Systems.AI_system.Core
{
    public abstract class GOAPAction : MonoBehaviour
    {
        [Header("Action Info")]
        public string actionName = "Action";
        
        [Tooltip("Вартість дії. Планувальник шукає шлях з найменшою загальною вартістю.")]
        public float cost = 1f;

        [Header("Movement")]
        [Tooltip("Чи потрібно агенту підійти до цілі перед виконанням дії?")]
        public bool requiresInRange = false;
        public float stoppingDistance = 1.5f;
        
        // Посилання на ціль, до якої треба підійти (встановлюється динамічно)
        [HideInInspector] public Transform targetTransform; 

        // --- Умови та Ефекти (для налаштування в Інспекторі) ---
        [System.Serializable]
        public struct StateData
        {
            public WorldKeys key; // Тепер це enum!
            public bool value;
        }

        [Header("States")]
        [SerializeField] private List<StateData> inspectorPreconditions = new List<StateData>();
        [SerializeField] private List<StateData> inspectorEffects = new List<StateData>();

        // Кешовані об'єкти WorldState для швидкої роботи планувальника
        public WorldState Preconditions { get; private set; }
        public WorldState Effects { get; private set; }

        // --- ІНІЦІАЛІЗАЦІЯ ---

        protected virtual void Awake()
        {
            Preconditions = new WorldState();
            Effects = new WorldState();

            // Переносимо дані з Інспектора у WorldState
            foreach (var p in inspectorPreconditions)
            {
                Preconditions.SetState(p.key, p.value);
            }
            
            foreach (var e in inspectorEffects)
            {
                Effects.SetState(e.key, e.value);
            }
        }

        // --- ЛОГІКА ДІЇ ---

        /// <summary>
        /// Контекстна перевірка. Чи можемо ми виконати дію прямо зараз?
        /// Наприклад: чи є патрони в зброї? Чи жива ще наша ціль?
        /// </summary>
        public virtual bool CheckProceduralPrecondition(GameObject agent)
        {
            return true;
        }

        /// <summary>
        /// Викликається ОДИН РАЗ, коли планувальник вирішує почати цю дію.
        /// Тут зручно діставати зброю або вмикати початкові тригери анімацій.
        /// </summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>
        /// Викликається КОЖЕН КАДР (Update), поки дія активна.
        /// </summary>
        /// <returns>True - дія успішно завершена. False - дія ще триває.</returns>
        public virtual bool Perform(GameObject agent)
        {
            return true; 
        }

        /// <summary>
        /// Викликається, коли дія завершена успішно, АБО якщо її перервали.
        /// Тут треба ховати зброю, скидати параметри аніматора тощо.
        /// </summary>
        public virtual void OnDeactivate()
        {
        }

        /// <summary>
        /// Перевіряє, чи достатньо близько агент до цілі (якщо requiresInRange = true).
        /// </summary>
        public virtual bool IsInRange()
        {
            if (targetTransform == null) return true;
            
            float distance = Vector3.Distance(transform.position, targetTransform.position);
            return distance <= stoppingDistance;
        }
    }
}