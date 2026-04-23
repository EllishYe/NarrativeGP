using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.Emails
{
    public class EmailListItemView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text subjectText;
        [SerializeField] private Color unreadTextColor = Color.red;
        [SerializeField] private Color readTextColor = Color.black;
        [SerializeField] private Color unreadSelectedBackgroundColor = Color.red;
        [SerializeField] private Color unreadSelectedTextColor = Color.white;
        [SerializeField] private Color readSelectedBackgroundColor = Color.black;
        [SerializeField] private Color readSelectedTextColor = Color.white;
        [SerializeField] private Color normalBackgroundColor = Color.white;

        private EmailsController controller;
        private string emailId;

        public string EmailId => emailId;

        private void Reset()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            subjectText = GetComponentInChildren<TMP_Text>();
        }

        public void Initialize(EmailsController owner)
        {
            controller = owner;
        }

        public void Bind(string nextEmailId, string subject)
        {
            emailId = nextEmailId;

            if (subjectText != null)
            {
                subjectText.text = subject;
            }
        }

        public void SetState(bool isRead, bool isSelected)
        {
            if (subjectText != null)
            {
                if (isSelected)
                {
                    subjectText.color = isRead ? readSelectedTextColor : unreadSelectedTextColor;
                }
                else
                {
                    subjectText.color = isRead ? readTextColor : unreadTextColor;
                }
            }

            if (background != null)
            {
                if (isSelected)
                {
                    background.color = isRead ? readSelectedBackgroundColor : unreadSelectedBackgroundColor;
                }
                else
                {
                    background.color = normalBackgroundColor;
                }
            }
        }

        public void OnClick()
        {
            controller?.SelectEmail(emailId);
        }
    }
}
