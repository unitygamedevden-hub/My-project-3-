using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Systems.AI_system.Core
{
    public class GOAPPlanner
    {
        // Внутрішній клас Node (Вузол) для побудови дерева можливих рішень
        private class Node
        {
            public Node parent;          // Попередній крок (щоб потім розкрутити ланцюжок назад)
            public float runningCost;    // Загальна вартість усіх дій до цього кроку
            public WorldState state;     // Стан світу після виконання дії на цьому кроці
            public GOAPAction action;    // Сама дія

            public Node(Node parent, float runningCost, WorldState state, GOAPAction action)
            {
                this.parent = parent;
                this.runningCost = runningCost;
                this.state = state;
                this.action = action;
            }
        }

        /// <summary>
        /// Створює план (чергу дій) для досягнення цілі
        /// </summary>
        public Queue<GOAPAction> Plan(GameObject agent, List<GOAPAction> availableActions, WorldState startState, GOAPGoal goal)
        {
            // 1. Відфільтровуємо дії, які зараз неможливо виконати процедурно 
            // (наприклад, зброя зламана, ціль знищена тощо)
            List<GOAPAction> usableActions = new List<GOAPAction>();
            foreach (var a in availableActions)
            {
                if (a.CheckProceduralPrecondition(agent))
                {
                    usableActions.Add(a);
                }
            }

            // 2. Створюємо список для збереження всіх успішних ланцюжків дій
            List<Node> leaves = new List<Node>();

            // Початковий вузол - це поточний стан нашого агента (без дій і вартості)
            Node startNode = new Node(null, 0f, startState.Clone(), null);

            // 3. Будуємо граф (дерево можливих варіантів майбутнього)
            bool success = BuildGraph(startNode, leaves, usableActions, goal.GetDesiredEffects());

            // Якщо не знайшли жодного шляху до цілі
            if (!success)
            {
                Debug.LogWarning("GOAP: Не вдалося знайти план для цілі " + goal.goalName);
                return null;
            }

            // 4. Шукаємо найдешевший план серед усіх успішних
            Node cheapestLeaf = null;
            foreach (var leaf in leaves)
            {
                if (cheapestLeaf == null || leaf.runningCost < cheapestLeaf.runningCost)
                {
                    cheapestLeaf = leaf;
                }
            }

            // 5. Розкручуємо ланцюжок з кінця (від фінальної дії до першої)
            List<GOAPAction> result = new List<GOAPAction>();
            Node n = cheapestLeaf;
            while (n != null)
            {
                if (n.action != null)
                {
                    result.Insert(0, n.action); // Вставляємо на початок списку
                }
                n = n.parent; // Йдемо до попереднього кроку
            }

            // 6. Конвертуємо список у чергу (Queue), бо агент буде брати їх по черзі (First In - First Out)
            Queue<GOAPAction> queue = new Queue<GOAPAction>();
            foreach (var a in result)
            {
                queue.Enqueue(a);
            }

            return queue;
        }

        /// <summary>
        /// Рекурсивно будує дерево можливих дій
        /// </summary>
        private bool BuildGraph(Node parent, List<Node> leaves, List<GOAPAction> usableActions, WorldState goalState)
        {
            bool foundOne = false;

            // Перебираємо всі доступні дії
            foreach (var action in usableActions)
            {
                // Якщо передумови (Preconditions) дії співпадають із станом світу у поточному вузлі
                if (parent.state.InState(action.Preconditions))
                {
                    // Симулюємо майбутнє: створюємо копію світу і застосовуємо ефекти цієї дії
                    WorldState currentState = parent.state.Clone();
                    currentState.ApplyState(action.Effects);

                    // Створюємо новий вузол (наступний крок)
                    Node node = new Node(parent, parent.runningCost + action.cost, currentState, action);

                    // Перевіряємо, чи досягли ми кінцевої цілі у цьому новому майбутньому?
                    if (currentState.InState(goalState))
                    {
                        leaves.Add(node);
                        foundOne = true;
                    }
                    else
                    {
                        // Якщо ні, ми створюємо новий список доступних дій (БЕЗ тієї, яку щойно використали)
                        // Це щоб уникнути нескінченних циклів (наприклад: Взяти зброю -> Покласти зброю -> Взяти...)
                        List<GOAPAction> subset = ActionSubset(usableActions, action);
                        
                        // Занурюємося далі в дерево рекурсивно
                        bool found = BuildGraph(node, leaves, subset, goalState);
                        if (found)
                        {
                            foundOne = true;
                        }
                    }
                }
            }

            return foundOne;
        }

        /// <summary>
        /// Створює копію списку дій, виключаючи вказану (щоб не використовувати дію двічі в одному ланцюжку)
        /// </summary>
        private List<GOAPAction> ActionSubset(List<GOAPAction> actions, GOAPAction removeMe)
        {
            List<GOAPAction> subset = new List<GOAPAction>();
            foreach (var a in actions)
            {
                if (!a.Equals(removeMe))
                    subset.Add(a);
            }
            return subset;
        }
    }
}