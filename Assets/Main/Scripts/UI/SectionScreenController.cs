using System;
using System.Collections.Generic;
using UnityEngine;

namespace NarrativeGP
{
    public class SectionScreenController : MonoBehaviour
    {
        [Serializable]
        private class SectionPanel
        {
            public SectionId sectionId;
            public GameObject root;
        }

        [SerializeField] private GameState gameState;
        [SerializeField] private GameFlowController flowController;
        [SerializeField] private List<SectionPanel> panels = new();
        [SerializeField] private SectionId defaultSection = SectionId.Attendance;

        private readonly Dictionary<SectionId, GameObject> panelLookup = new();

        private void Awake()
        {
            panelLookup.Clear();
            foreach (SectionPanel panel in panels)
            {
                if (panel.root != null)
                {
                    panelLookup[panel.sectionId] = panel.root;
                }
            }

            OpenSection(defaultSection);
        }

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
            flowController = FindFirstObjectByType<GameFlowController>();
        }

        public void OpenSection(SectionId sectionId)
        {
            if (flowController != null && !flowController.IsSectionInteractable(sectionId))
            {
                return;
            }

            foreach (KeyValuePair<SectionId, GameObject> pair in panelLookup)
            {
                pair.Value.SetActive(pair.Key == sectionId);
            }

            if (gameState != null)
            {
                gameState.MarkSectionOpened(sectionId);
            }
        }
    }
}
