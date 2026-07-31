// ... existing code ...

using Project.Scripts.Systems.AI_system.Core;
using Project.Scripts.Systems.AI_system.Needs;
using UnityEngine;

namespace Project.Scripts.Systems.AI_system.UI
{
    public class GOAPDebugUI : MonoBehaviour
    {
        [Header("References")]
        public GOAPAgent agent;
        public SleepNeed sleepNeed; // Поки що слідкуємо тільки за сном

        [Header("UI Style Settings")]
        public int headerFontSize = 36; // Розмір шрифту для заголовка
        public int normalFontSize = 24; // Розмір шрифту для тексту
        public int windowWidth = 400; // Ширина вікна (трохи збільшимо)
        public int windowHeight = 200; // Висота вікна

        private GUIStyle headerStyle;
        private GUIStyle normalStyle;

        private void OnGUI()
        {
            if (agent == null) return;

            // Ініціалізація стилів, якщо вони ще не створені.
            // Ми беремо налаштування за замовчуванням (з GUI.skin) і змінюємо розмір тексту та підтримку RichText
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.richText = true; // Дозволяємо використовувати <color> і <b>
                headerStyle.fontSize = headerFontSize;
            }

            if (normalStyle == null)
            {
                normalStyle = new GUIStyle(GUI.skin.label);
                normalStyle.richText = true;
                normalStyle.fontSize = normalFontSize;
            }

            // Оновлюємо розмір шрифту, якщо ти змінив його в Інспекторі під час гри
            headerStyle.fontSize = headerFontSize;
            normalStyle.fontSize = normalFontSize;


            // Створюємо чорне напівпрозоре віконце в лівому верхньому кутку
            GUILayout.BeginArea(new Rect(10, 10, windowWidth, windowHeight), GUI.skin.box);
            
            // Заголовок (використовуємо наш headerStyle)
            GUILayout.Label("<color=yellow><b>GOAP Agent Brain</b></color>", headerStyle);
            GUILayout.Space(5);

            // Показуємо рівень втоми
            if (sleepNeed != null)
            {
                string color = sleepNeed.currentValue >= sleepNeed.sleepThreshold ? "red" : "white";
                GUILayout.Label($"Втома: <color={color}>{sleepNeed.currentValue:F1}%</color>", normalStyle);
            }

            GUILayout.Space(5);

            // Показуємо стан FSM (Що робить тіло)
            GUILayout.Label($"Стан тіла: {agent.CurrentStateName}", normalStyle);

            // Показуємо ціль (Що хоче зробити мозок)
            string goalName = agent.CurrentGoal != null ? agent.CurrentGoal.goalName : "Немає цілі / Думає";
            GUILayout.Label($"Поточна ціль: {goalName}", normalStyle);

            // Показуємо поточну дію
            string actionName = agent.CurrentAction != null ? agent.CurrentAction.GetType().Name : "---";
            GUILayout.Label($"Виконує дію: {actionName}", normalStyle);

            GUILayout.EndArea();
        }
    }
}
// ... existing code ...