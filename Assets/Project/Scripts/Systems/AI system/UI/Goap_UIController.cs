using UnityEngine;
using UnityEngine.UIElements;
using Project.Scripts.Systems.AI_system.Core;

namespace Project.Scripts.Systems.AI_system.UI
{
    public class GOAP_UIController : MonoBehaviour
    {
        [Header("References")]
        public GoapAgent agent;

        [Header("UI Settings")]
        [SerializeField] 
        private string labelText = "Поточний стан:"; 

        [SerializeField, Tooltip("Ім'я Label елемента у UI Builder")]
        private string targetLabelName = "LabelText";

        private Label _statusLabel;
        private PanelRenderer _panelRenderer;

        private void OnEnable()
        {
            _panelRenderer = GetComponent<PanelRenderer>();
            if (_panelRenderer == null)
            {
                _panelRenderer = GetComponentInChildren<PanelRenderer>();
            }

            if (_panelRenderer != null)
            {
                // Підписуємося на завантаження панелі через офіційний метод PanelRenderer
                _panelRenderer.RegisterUIReloadCallback(OnUIReload);
            }
            else
            {
                Debug.LogWarning("Не вдалося знайти компонент PanelRenderer!");
            }
        }

        private void OnDisable()
        {
            if (_panelRenderer != null)
            {
                _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            }
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            if (root == null) return;

            // Знаходимо наш Label за ім'ям із UI Builder
            _statusLabel = root.Q<Label>(targetLabelName);

            if (_statusLabel == null)
            {
                Debug.LogWarning($"Не вдалося знайти Label з іменем '{targetLabelName}' у PanelRenderer!");
            }
        }

        private void Update()
        {
            if (agent == null || _statusLabel == null) return;

            // Визначаємо поточну дію агента
            string currentActionInfo = agent.CurrentAction != null ? agent.CurrentAction.actionName : "Думає / Очікує";

            // Оновлюємо текст у Label
            _statusLabel.text = $"{labelText} {currentActionInfo}";
        }
    }
}