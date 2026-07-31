using UnityEngine;
using Project.Scripts.Systems.AI_system.Core;

namespace Project.Scripts.Systems.AI_system.UI
{
    public class GOAPMemoryDebug : MonoBehaviour
    {
        [Header("References")]
        public GOAPAgent agent;

        [Header("UI Position & Style")]
        public Vector2 windowPosition = new Vector2(10, 150);
        public Vector2 windowSize = new Vector2(280, 220);

        private void Start()
        {
            if (agent == null)
            {
                agent = GetComponent<GOAPAgent>();
            }
        }

        private void OnGUI()
        {
            if (agent == null || agent.Memory == null) return;

            Rect windowRect = new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y);
            GUILayout.BeginArea(windowRect, GUI.skin.box);

            GUILayout.Label("<color=cyan><b>GOAP Memory (World State)</b></color>");
            GUILayout.Space(5);

            // Використовуємо новий метод
            var states = agent.Memory.GetAllStates();

            if (states != null && states.Count > 0)
            {
                foreach (var kvp in states)
                {
                    string keyName = kvp.Key;
                    object val = kvp.Value;

                    // Гарне форматування для кольору, якщо значення це boolean
                    string displayVal = val != null ? val.ToString() : "null";
                    string color = "white";

                    if (val is bool b)
                    {
                        color = b ? "green" : "red";
                        displayVal = b.ToString();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{keyName}:");
                    GUILayout.Label($"<color={color}><b>{displayVal}</b></color>");
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("Пам'ять порожня.");
            }

            GUILayout.EndArea();
        }
    }
}