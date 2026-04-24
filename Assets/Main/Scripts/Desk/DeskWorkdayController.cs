using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NarrativeGP.Desk
{
    public class DeskWorkdayController : MonoBehaviour
    {
        [Header("Day Tasks")]
        [SerializeField] private int tasksPerDay = 5;
        [SerializeField] private List<DeskTaskData> currentDayTasks = new();
        [SerializeField] private DeskTaskData completedStateTask;

        [Header("Dependencies")]
        [SerializeField] private DeskTaskController deskTaskController;

        [Header("Progress UI")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TMP_Text progressLabel;

        [Header("Events")]
        [SerializeField] private UnityEvent onWorkdayCompleted;

        private int currentTaskIndex;
        private int completedTaskCount;

        private void Awake()
        {
            if (deskTaskController != null)
            {
                deskTaskController.TaskSubmitted += HandleTaskSubmitted;
            }

            StartWorkday();
        }

        private void OnDestroy()
        {
            if (deskTaskController != null)
            {
                deskTaskController.TaskSubmitted -= HandleTaskSubmitted;
            }
        }

        public void SetCurrentDayTasks(List<DeskTaskData> taskDataList)
        {
            currentDayTasks = taskDataList ?? new List<DeskTaskData>();
            StartWorkday();
        }

        [ContextMenu("Start Workday")]
        public void StartWorkday()
        {
            currentTaskIndex = 0;
            completedTaskCount = 0;

            LoadCurrentTask();
            RefreshProgress();
        }

        private void HandleTaskSubmitted()
        {
            if (completedTaskCount >= GetTaskTargetCount())
            {
                return;
            }

            completedTaskCount++;
            currentTaskIndex++;

            RefreshProgress();

            if (completedTaskCount >= GetTaskTargetCount())
            {
                deskTaskController?.SetTask(completedStateTask);
                onWorkdayCompleted?.Invoke();
                return;
            }

            LoadCurrentTask();
        }

        private void LoadCurrentTask()
        {
            if (deskTaskController == null)
            {
                return;
            }

            DeskTaskData nextTask = currentTaskIndex >= 0 && currentTaskIndex < currentDayTasks.Count
                ? currentDayTasks[currentTaskIndex]
                : null;

            deskTaskController.SetTask(nextTask);
        }

        private void RefreshProgress()
        {
            int taskTargetCount = GetTaskTargetCount();
            float progress = taskTargetCount > 0 ? (float)completedTaskCount / taskTargetCount : 0f;

            if (progressSlider != null)
            {
                progressSlider.minValue = 0f;
                progressSlider.maxValue = 1f;
                progressSlider.value = progress;
            }

            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = progress;
            }

            if (progressLabel != null)
            {
                progressLabel.text = $"{completedTaskCount}/{taskTargetCount}";
            }
        }

        private int GetTaskTargetCount()
        {
            if (tasksPerDay <= 0)
            {
                return 0;
            }

            return Mathf.Min(tasksPerDay, currentDayTasks.Count);
        }
    }
}
