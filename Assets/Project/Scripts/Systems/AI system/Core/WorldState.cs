using System.Collections.Generic;
using Project.Scripts.Systems.AI_system.Core.Project.Scripts.Systems.AI_system.Core;

namespace Project.Scripts.Systems.AI_system.Core
{
    public class WorldState
    {
        // Головне сховище наших "фактів".
        // Ключ (string) - назва факту, Значення (object) - стан (bool, int, Transform тощо)
        private Dictionary<string, object> _states;

        // Конструктор
        public WorldState()
        {
            _states = new Dictionary<string, object>();
        }

        // --- БАЗОВІ ОПЕРАЦІЇ З ДАНИМИ ---

        // Додай ці методи у свій клас WorldState.cs
        public bool HasState(WorldKeys key) => HasState(key.ToString());
        public object GetState(WorldKeys key) => GetState(key.ToString());
        public bool GetBool(WorldKeys key) => GetBool(key.ToString());
        public void SetState(WorldKeys key, object value) => SetState(key.ToString(), value);
        public void RemoveState(WorldKeys key) => RemoveState(key.ToString());
        
        public bool HasState(string key)
        {
            return _states.ContainsKey(key);
        }

        public object GetState(string key)
        {
            if (_states.ContainsKey(key))
                return _states[key];
            
            return null;
        }

        public bool GetBool(string key)
        {
            if (_states.ContainsKey(key) && _states[key] is bool boolValue)
                return boolValue;
            
            return false;
        }

        public void SetState(string key, object value)
        {
            if (_states.ContainsKey(key))
                _states[key] = value;
            else
                _states.Add(key, value);
        }

        public void RemoveState(string key)
        {
            if (_states.ContainsKey(key))
                _states.Remove(key);
        }

        // --- МЕТОДИ ДЛЯ ПЛАНУВАЛЬНИКА (GOAP PLANNER) ---

        public void ApplyState(WorldState otherState)
        {
            if (otherState == null || otherState._states == null) return;

            foreach (var state in otherState._states)
            {
                SetState(state.Key, state.Value);
            }
        }
        
        

        public bool InState(WorldState conditionState)
        {
            if (conditionState == null || conditionState._states == null) return true;

            foreach (var condition in conditionState._states)
            {
                if (!_states.ContainsKey(condition.Key))
                    return false;
                
                if (!object.Equals(_states[condition.Key], condition.Value))
                    return false;
            }
            
            return true;
        }

        public WorldState Clone()
        {
            WorldState clone = new WorldState();
            foreach (var state in _states)
            {
                clone.SetState(state.Key, state.Value);
            }
            return clone;
        }
        
        // Додай цей метод в кінець класу WorldState
        public Dictionary<string, object> GetAllStates()
        {
            return _states;
        }
    }
}
