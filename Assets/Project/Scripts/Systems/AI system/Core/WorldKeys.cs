namespace Project.Scripts.Systems.AI_system.Core
{
    namespace Project.Scripts.Systems.AI_system.Core
    {
        public enum WorldKeys
        {
            IsTired,
            IsExhausted,
            IsPatrolling,
            AtWaypoint,
            HasTarget,
            IsTargetReached,
            IsDead,
            TargetEliminated,
            TargetInRange,
            KnowsTargetLocation, // Чи знає агент, де останній раз був гравець
            IsInvestigating      // Чи шукає агент ворога в тій точці
        }
    }
}