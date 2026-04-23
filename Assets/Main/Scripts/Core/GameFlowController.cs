using System.Collections.Generic;
using UnityEngine;
using NarrativeGP.Emails;

namespace NarrativeGP
{
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        [SerializeField] private SectionFlowDefinition flowDefinition;
        [SerializeField] private EmailsController emailsController;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.StateChanged += HandleGameStateChanged;
            }

            SyncSectionProviders();
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.StateChanged -= HandleGameStateChanged;
            }
        }

        public bool IsSectionInteractable(SectionId sectionId)
        {
            if (gameState == null || flowDefinition == null)
            {
                return false;
            }

            if (gameState.CurrentDay == 1)
            {
                return IsDayOneSectionInteractable(sectionId);
            }

            return IsLaterDaySectionInteractable(sectionId);
        }

        public SectionVisualState GetVisualState(SectionId sectionId)
        {
            if (!IsSectionInteractable(sectionId))
            {
                return SectionVisualState.Locked;
            }

            if (gameState.HasUnreadContent(sectionId))
            {
                return SectionVisualState.New;
            }

            return SectionVisualState.Available;
        }

        public bool AreAllDaytimeSectionsCompletedToday()
        {
            foreach (SectionId sectionId in flowDefinition.DaytimeSections)
            {
                if (!gameState.WasCompletedToday(sectionId))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsDayOneSectionInteractable(SectionId sectionId)
        {
            List<SectionId> dayOneOrder = new(flowDefinition.DaytimeSections) { flowDefinition.WrapUpSection };

            for (int i = 0; i < dayOneOrder.Count; i++)
            {
                if (dayOneOrder[i] != sectionId)
                {
                    continue;
                }

                for (int previousIndex = 0; previousIndex < i; previousIndex++)
                {
                    if (!gameState.WasCompletedToday(dayOneOrder[previousIndex]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private bool IsLaterDaySectionInteractable(SectionId sectionId)
        {
            if (sectionId == SectionId.Attendance)
            {
                return true;
            }

            if (sectionId == flowDefinition.WrapUpSection)
            {
                return AreAllDaytimeSectionsCompletedToday();
            }

            if (!gameState.WasCompletedToday(SectionId.Attendance))
            {
                return false;
            }

            foreach (SectionId daytimeSection in flowDefinition.DaytimeSections)
            {
                if (daytimeSection == sectionId)
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleGameStateChanged()
        {
            SyncSectionProviders();
        }

        private void SyncSectionProviders()
        {
            if (gameState == null)
            {
                return;
            }

            if (emailsController != null)
            {
                emailsController.SyncSectionProgressToGameState(gameState);
            }
        }
    }
}
