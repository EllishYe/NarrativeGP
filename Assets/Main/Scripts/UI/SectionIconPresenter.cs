using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP
{
    public class SectionIconPresenter : MonoBehaviour
    {
        [SerializeField] private SectionId sectionId;
        [SerializeField] private GameFlowController flowController;
        [SerializeField] private GameState gameState;
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color availableColor = Color.black;
        [SerializeField] private Color newContentColor = Color.red;

        private void Reset()
        {
            flowController = FindFirstObjectByType<GameFlowController>();
            gameState = FindFirstObjectByType<GameState>();
            button = GetComponent<Button>();
            iconImage = GetComponent<Image>();
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
            if (flowController == null)
            {
                return;
            }

            SectionVisualState state = flowController.GetVisualState(sectionId);

            if (button != null)
            {
                button.interactable = state != SectionVisualState.Locked;
            }

            if (iconImage != null)
            {
                iconImage.color = state switch
                {
                    SectionVisualState.Available => availableColor,
                    SectionVisualState.New => newContentColor,
                    _ => lockedColor
                };
            }
        }
    }
}
