using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NarrativeGP.Desk
{
    public class DeskTaskController : MonoBehaviour
    {
        public event Action TaskSubmitted;

        [Header("Task Data")]
        [SerializeField] private DeskTaskData currentTask;

        [Header("UI")]
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private Image captchaImage;
        [SerializeField] private Button submitButton;
        [SerializeField] private List<DeskGridCellView> cellViews = new();

        [Header("Events")]
        [SerializeField] private UnityEvent onTaskSubmitted;

        private readonly bool[] currentSelection = new bool[9];

        private void Awake()
        {
            foreach (DeskGridCellView cellView in cellViews)
            {
                if (cellView != null)
                {
                    cellView.Initialize(this);
                }
            }

            RefreshView();
        }

        public void SetTask(DeskTaskData taskData)
        {
            currentTask = taskData;
            ResetSelection();
            RefreshView();
        }

        public void ToggleCell(int cellIndex)
        {
            if (!IsTaskInteractive())
            {
                return;
            }

            if (cellIndex < 0 || cellIndex >= currentSelection.Length)
            {
                return;
            }

            currentSelection[cellIndex] = !currentSelection[cellIndex];
            RefreshCells();
            RefreshSubmitButton();
        }

        public void SubmitCurrentTask()
        {
            if (!IsTaskInteractive())
            {
                return;
            }

            if (!IsCurrentSelectionCorrect())
            {
                return;
            }

            TaskSubmitted?.Invoke();
            onTaskSubmitted?.Invoke();
            ResetSelection();
            RefreshView();
        }

        [ContextMenu("Refresh Desk Task")]
        public void RefreshView()
        {
            if (promptText != null)
            {
                promptText.text = currentTask != null ? currentTask.prompt : string.Empty;
            }

            if (captchaImage != null)
            {
                captchaImage.sprite = currentTask != null ? currentTask.image : null;
                captchaImage.enabled = currentTask != null && currentTask.image != null;
            }

            RefreshCells();
            RefreshSubmitButton();
        }

        private void RefreshCells()
        {
            foreach (DeskGridCellView cellView in cellViews)
            {
                if (cellView == null)
                {
                    continue;
                }

                int index = cellView.CellIndex;
                bool isSelected = index >= 0 && index < currentSelection.Length && currentSelection[index];
                cellView.SetSelected(isSelected);
                cellView.SetInteractable(IsTaskInteractive());
            }
        }

        private void RefreshSubmitButton()
        {
            if (submitButton != null)
            {
                submitButton.interactable = IsTaskInteractive() && IsCurrentSelectionCorrect();
            }
        }

        private bool IsCurrentSelectionCorrect()
        {
            if (currentTask == null || string.IsNullOrWhiteSpace(currentTask.correctPattern))
            {
                return false;
            }

            if (currentTask.correctPattern.Length != currentSelection.Length)
            {
                return false;
            }

            string currentPattern = BuildCurrentPattern();
            return string.Equals(currentPattern, currentTask.correctPattern, StringComparison.Ordinal);
        }

        private string BuildCurrentPattern()
        {
            StringBuilder builder = new(currentSelection.Length);

            for (int i = 0; i < currentSelection.Length; i++)
            {
                builder.Append(currentSelection[i] ? '1' : '0');
            }

            return builder.ToString();
        }

        private void ResetSelection()
        {
            for (int i = 0; i < currentSelection.Length; i++)
            {
                currentSelection[i] = false;
            }
        }

        private bool IsTaskInteractive()
        {
            return currentTask != null && currentTask.isInteractive;
        }
    }
}
