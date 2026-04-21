using System;
using System.Collections.Generic;

namespace NarrativeGP
{
    [Serializable]
    public class StateCondition
    {
        public enum ConditionType
        {
            CurrentDayEquals,
            CurrentDayAtLeast,
            SectionOpenedToday,
            SectionCompletedToday,
            SectionCompletedEver,
            SectionHasUnreadContent,
            FlagIsSet,
            AllListedSectionsCompletedToday
        }

        public ConditionType type;
        public int dayValue = 1;
        public string flagKey;
        public SectionId sectionId = SectionId.Attendance;
        public List<SectionId> sections = new();

        public bool Evaluate(GameState gameState)
        {
            if (gameState == null)
            {
                return false;
            }

            return type switch
            {
                ConditionType.CurrentDayEquals => gameState.CurrentDay == dayValue,
                ConditionType.CurrentDayAtLeast => gameState.CurrentDay >= dayValue,
                ConditionType.SectionOpenedToday => gameState.WasOpenedToday(sectionId),
                ConditionType.SectionCompletedToday => gameState.WasCompletedToday(sectionId),
                ConditionType.SectionCompletedEver => gameState.WasCompletedEver(sectionId),
                ConditionType.SectionHasUnreadContent => gameState.HasUnreadContent(sectionId),
                ConditionType.FlagIsSet => gameState.HasFlag(flagKey),
                ConditionType.AllListedSectionsCompletedToday => AreSectionsCompletedToday(gameState, sections),
                _ => false
            };
        }

        private static bool AreSectionsCompletedToday(GameState gameState, List<SectionId> sectionIds)
        {
            foreach (SectionId id in sectionIds)
            {
                if (!gameState.WasCompletedToday(id))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
