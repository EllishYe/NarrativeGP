using System;
using System.Collections.Generic;
using UnityEngine;

namespace NarrativeGP
{
    public class ConditionalStatePresenter : MonoBehaviour
    {
        [Serializable]
        private class ViewState
        {
            public string stateId;
            public ConditionSet conditions;
            public List<GameObject> targets = new();
        }

        [SerializeField] private GameState gameState;
        [SerializeField] private List<ViewState> states = new();
        [SerializeField] private bool useLastMatch = true;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.StateChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.StateChanged -= Refresh;
            }
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            int activeIndex = -1;

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].conditions != null && states[i].conditions.Evaluate(gameState))
                {
                    activeIndex = i;
                    if (!useLastMatch)
                    {
                        break;
                    }
                }
            }

            for (int i = 0; i < states.Count; i++)
            {
                bool shouldShow = i == activeIndex;
                foreach (GameObject target in states[i].targets)
                {
                    if (target != null)
                    {
                        target.SetActive(shouldShow);
                    }
                }
            }
        }
    }
}
