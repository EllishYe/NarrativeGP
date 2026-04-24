using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.Desk
{
    public class DeskGridCellView : MonoBehaviour
    {
        [SerializeField] private int cellIndex;
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Sprite selectedSprite;

        private DeskTaskController controller;
        private bool isSelected;

        public int CellIndex => cellIndex;

        private void Reset()
        {
            button = GetComponent<Button>();
            image = GetComponent<Image>();
        }

        public void Initialize(DeskTaskController owner)
        {
            controller = owner;
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (image != null)
            {
                image.sprite = isSelected ? selectedSprite : unselectedSprite;
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        public void OnClick()
        {
            controller?.ToggleCell(cellIndex);
        }
    }
}
