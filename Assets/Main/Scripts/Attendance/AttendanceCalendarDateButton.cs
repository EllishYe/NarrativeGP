using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.Attendance
{
    public class AttendanceCalendarDateButton : MonoBehaviour
    {
        [SerializeField] private string dateIso;
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text dayLabel;

        public string DateIso => dateIso;

        private AttendanceController controller;

        private void Reset()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            dayLabel = GetComponentInChildren<TMP_Text>();
        }

        public void Initialize(AttendanceController owner)
        {
            controller = owner;
            UpdateLabel();
        }

        public void OnClick()
        {
            controller?.SelectDate(dateIso);
        }

        public void SetVisuals(Color backgroundColor, Color textColor, bool interactable)
        {
            if (background != null)
            {
                background.color = backgroundColor;
            }

            if (dayLabel != null)
            {
                dayLabel.color = textColor;
            }

            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void UpdateLabel()
        {
            if (dayLabel == null || string.IsNullOrWhiteSpace(dateIso))
            {
                return;
            }

            if (System.DateTime.TryParse(dateIso, out System.DateTime parsedDate))
            {
                dayLabel.text = parsedDate.Day.ToString();
                return;
            }

            dayLabel.text = dateIso;
        }
    }
}
