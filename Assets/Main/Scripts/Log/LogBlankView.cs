using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NarrativeGP.Logs
{
    public class LogBlankView : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private string placeholderText = "Select...";

        private int correctIndex;
        private UnityAction<int> valueChangedCallback;

        private void Reset()
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        public void Bind(List<string> options, int nextCorrectIndex, int initialValue, UnityAction<int> onValueChanged)
        {
            correctIndex = nextCorrectIndex;
            valueChangedCallback = onValueChanged;

            if (dropdown == null)
            {
                return;
            }

            dropdown.onValueChanged.RemoveAllListeners();

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
            dropdown.SetValueWithoutNotify(Mathf.Clamp(initialValue, 0, optionData.Count - 1));
            dropdown.RefreshShownValue();
            dropdown.onValueChanged.AddListener(HandleValueChanged);
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

        private void HandleValueChanged(int value)
        {
            valueChangedCallback?.Invoke(value);
        }
    }
}
