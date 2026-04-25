using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NarrativeGP.Logs
{
    public class LogBlankView : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private string placeholderText = "Select...";

        private int correctIndex;

        private void Reset()
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        public void Bind(List<string> options, int nextCorrectIndex)
        {
            correctIndex = nextCorrectIndex;

            if (dropdown == null)
            {
                return;
            }

            dropdown.ClearOptions();

            List<TMP_Dropdown.OptionData> optionData = new()
            {
                new TMP_Dropdown.OptionData(placeholderText)
            };

            if (options != null)
            {
                foreach (string option in options)
                {
                    optionData.Add(new TMP_Dropdown.OptionData(option));
                }
            }

            dropdown.AddOptions(optionData);
            dropdown.value = 0;
            dropdown.RefreshShownValue();
        }

        public bool IsCorrect()
        {
            if (dropdown == null)
            {
                return false;
            }

            if (dropdown.value <= 0)
            {
                return false;
            }

            return dropdown.value - 1 == correctIndex;
        }
    }
}
