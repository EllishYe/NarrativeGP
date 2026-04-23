using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.News
{
    public class NewsListItemView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Color unreadTextColor = Color.red;
        [SerializeField] private Color readTextColor = Color.black;
        [SerializeField] private Color unreadSelectedBackgroundColor = Color.red;
        [SerializeField] private Color unreadSelectedTextColor = Color.white;
        [SerializeField] private Color readSelectedBackgroundColor = Color.black;
        [SerializeField] private Color readSelectedTextColor = Color.white;
        [SerializeField] private Color normalBackgroundColor = Color.white;

        private NewsController controller;
        private string newsId;

        public string NewsId => newsId;

        private void Reset()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            titleText = GetComponentInChildren<TMP_Text>();
        }

        public void Initialize(NewsController owner)
        {
            controller = owner;
        }

        public void Bind(string nextNewsId, string title)
        {
            newsId = nextNewsId;

            if (titleText != null)
            {
                titleText.text = title;
            }
        }

        public void SetState(bool isCompleted, bool isSelected)
        {
            if (titleText != null)
            {
                if (isSelected)
                {
                    titleText.color = isCompleted ? readSelectedTextColor : unreadSelectedTextColor;
                }
                else
                {
                    titleText.color = isCompleted ? readTextColor : unreadTextColor;
                }
            }

            if (background != null)
            {
                if (isSelected)
                {
                    background.color = isCompleted ? readSelectedBackgroundColor : unreadSelectedBackgroundColor;
                }
                else
                {
                    background.color = normalBackgroundColor;
                }
            }
        }

        public void OnClick()
        {
            controller?.SelectNews(newsId);
        }
    }
}
