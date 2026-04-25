using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP
{
    [AddComponentMenu("Layout/Flow Layout Group")]
    public class FlowLayoutGroup : LayoutGroup
    {
        [SerializeField] private float spacingX = 10f;
        [SerializeField] private float spacingY = 10f;
        [SerializeField] private float preferredWidth = 900f;
        [SerializeField] private bool controlChildWidth;
        [SerializeField] private bool controlChildHeight;

        private float preferredHeight;

        public float SpacingX
        {
            get => spacingX;
            set
            {
                spacingX = value;
                SetDirty();
            }
        }

        public float SpacingY
        {
            get => spacingY;
            set
            {
                spacingY = value;
                SetDirty();
            }
        }

        public bool ControlChildWidth
        {
            get => controlChildWidth;
            set
            {
                controlChildWidth = value;
                SetDirty();
            }
        }

        public bool ControlChildHeight
        {
            get => controlChildHeight;
            set
            {
                controlChildHeight = value;
                SetDirty();
            }
        }

        public float PreferredWidth
        {
            get => preferredWidth;
            set
            {
                preferredWidth = value;
                SetDirty();
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            float width = padding.horizontal + Mathf.Max(0f, preferredWidth);
            SetLayoutInputForAxis(width, width, -1f, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            preferredHeight = CalculateLayoutHeight();
            SetLayoutInputForAxis(preferredHeight, preferredHeight, -1f, 1);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongFlow();
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongFlow();
        }

        private float CalculateLayoutHeight()
        {
            float availableWidth = GetAvailableWidth();
            float rowWidth = 0f;
            float rowHeight = 0f;
            float totalHeight = padding.top + padding.bottom;
            bool hasItemsInRow = false;

            foreach (RectTransform child in rectChildren)
            {
                Vector2 childSize = GetChildSize(child);
                bool shouldWrap = hasItemsInRow && rowWidth + spacingX + childSize.x > availableWidth;

                if (shouldWrap)
                {
                    totalHeight += rowHeight + spacingY;
                    rowWidth = 0f;
                    rowHeight = 0f;
                    hasItemsInRow = false;
                }

                rowWidth = hasItemsInRow ? rowWidth + spacingX + childSize.x : childSize.x;
                rowHeight = Mathf.Max(rowHeight, childSize.y);
                hasItemsInRow = true;
            }

            if (hasItemsInRow)
            {
                totalHeight += rowHeight;
            }

            return totalHeight;
        }

        private void SetChildrenAlongFlow()
        {
            float availableWidth = GetAvailableWidth();
            float x = padding.left;
            float y = padding.top;
            float rowHeight = 0f;
            bool hasItemsInRow = false;

            foreach (RectTransform child in rectChildren)
            {
                Vector2 childSize = GetChildSize(child);
                bool shouldWrap = hasItemsInRow && x + spacingX + childSize.x > padding.left + availableWidth;

                if (shouldWrap)
                {
                    x = padding.left;
                    y += rowHeight + spacingY;
                    rowHeight = 0f;
                    hasItemsInRow = false;
                }

                if (hasItemsInRow)
                {
                    x += spacingX;
                }

                SetChildAlongAxis(child, 0, x, childSize.x);
                SetChildAlongAxis(child, 1, y, childSize.y);

                x += childSize.x;
                rowHeight = Mathf.Max(rowHeight, childSize.y);
                hasItemsInRow = true;
            }
        }

        private float GetAvailableWidth()
        {
            float width = rectTransform.rect.width - padding.horizontal;
            if (width <= 0f)
            {
                width = preferredWidth;
            }

            return Mathf.Max(0f, width);
        }

        private Vector2 GetChildSize(RectTransform child)
        {
            float width = controlChildWidth
                ? Mathf.Max(0f, GetAvailableWidth())
                : LayoutUtility.GetPreferredSize(child, 0);

            float height = controlChildHeight
                ? LayoutUtility.GetPreferredSize(child, 1)
                : child.rect.height;

            if (width <= 0f)
            {
                width = child.rect.width;
            }

            if (height <= 0f)
            {
                height = child.rect.height;
            }

            return new Vector2(width, height);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            spacingX = Mathf.Max(0f, spacingX);
            spacingY = Mathf.Max(0f, spacingY);
            preferredWidth = Mathf.Max(0f, preferredWidth);
            SetDirty();
        }
#endif
    }
}
