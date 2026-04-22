using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.Emails
{
    public class EmailListItemView : MonoBehaviour
    {
        [SerializeField] private string emailId;
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text subjectText;
        [SerializeField] private Color unreadTextColor = Color.red;
        [SerializeField] private Color readTextColor = Color.black;
        [SerializeField] private Color normalBackgroundColor = Color.white;
        [SerializeField] private Color selectedBackgroundColor = new(0.85f, 0.85f, 0.85f, 1f);

        private EmailsController controller;

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

        public void SetTitle(string subject)
        {
            if (subjectText != null)
            {
                subjectText.text = subject;
            }
        }

        public void SetState(bool isRead, bool isSelected)
        {
            if (subjectText != null)
            {
                subjectText.color = isRead ? readTextColor : unreadTextColor;
            }

            if (background != null)
            {
                background.color = isSelected ? selectedBackgroundColor : normalBackgroundColor;
            }
        }

        public void OnClick()
        {
            controller?.SelectEmail(emailId);
        }
    }
}
