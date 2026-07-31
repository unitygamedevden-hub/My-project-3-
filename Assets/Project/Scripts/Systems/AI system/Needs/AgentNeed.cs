using UnityEngine;
using Project.Scripts.Systems.AI_system.Core;

namespace Project.Scripts.Systems.AI_system.Needs
{
    // Це базовий клас для всіх потреб (Голод, Сон, Здоров'я тощо)
    public abstract class AgentNeed : MonoBehaviour
    {
        [Header("Need Settings")]
        public float currentValue = 0f;
        public float increaseRate = 2f; // Швидкість зростання потреби

        // Кожна потреба сама вирішує, що їй записувати в пам'ять агента
        public abstract void UpdateNeed(GOAPAgent agent);
    }
}