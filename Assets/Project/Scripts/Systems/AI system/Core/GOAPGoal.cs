using System.Collections.Generic;
using Project.Scripts.Systems.AI_system.Core.Project.Scripts.Systems.AI_system.Core;
using UnityEngine;

namespace Project.Scripts.Systems.AI_system.Core
{
    [CreateAssetMenu(fileName = "NewGOAPGoal", menuName = "GOAP/Goal")]
    public class GOAPGoal : ScriptableObject
    {
        [Header("Goal Identity")]
        public string goalName = "New Goal";
        
        [Header("Priority")]
        [Tooltip("Базовий пріоритет цілі. Чим вище число, тим важливіша ціль.")]
        public int basePriority = 1;

        // Початкові умови через WorldKeys enum
        [SerializeField] 
        private List<StateData> preconditionsList = new List<StateData>();

        // Бажані ефекти через WorldKeys enum
        [SerializeField] 
        private List<StateData> desiredEffectsList = new List<StateData>();

        private WorldState _preconditions;
        private WorldState _desiredEffects;

        // --- Структура-обгортка з використанням enum WorldKeys ---
        [System.Serializable]
        public struct StateData
        {
            public WorldKeys key;
            public bool value;
        }

        // --- ІНІЦІАЛІЗАЦІЯ ---

        private void OnEnable()
        {
            InitStates();
        }

        public void InitStates()
        {
            _preconditions = new WorldState();
            foreach (var state in preconditionsList)
            {
                // Конвертуємо enum у рядок, якщо твій WorldState приймає string, 
                // або передаємо напряму, залежно від того, як реалізований WorldState.
                _preconditions.SetState(state.key.ToString(), state.value);
            }

            _desiredEffects = new WorldState();
            foreach (var state in desiredEffectsList)
            {
                _desiredEffects.SetState(state.key.ToString(), state.value);
            }
        }

        // --- ОСНОВНІ МЕТОДИ ДЛЯ ПЛАНУВАЛЬНИКА ---

        public WorldState GetDesiredEffects()
        {
            if (_desiredEffects == null) InitStates();
            return _desiredEffects;
        }

        public bool CanBeActivated(WorldState currentWorldState)
        {
            if (_preconditions == null) InitStates();
            return currentWorldState.InState(_preconditions);
        }

        public virtual int GetPriority(WorldState currentWorldState)
        {
            return basePriority;
        }
    }
}