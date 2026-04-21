using System;
using System.Collections.Generic;

namespace NarrativeGP
{
    [Serializable]
    public class ConditionSet
    {
        public bool requireAll = true;
        public List<StateCondition> conditions = new();

        public bool Evaluate(GameState gameState)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return true;
            }

            if (requireAll)
            {
                foreach (StateCondition condition in conditions)
                {
                    if (!condition.Evaluate(gameState))
                    {
                        return false;
                    }
                }

                return true;
            }

            foreach (StateCondition condition in conditions)
            {
                if (condition.Evaluate(gameState))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
