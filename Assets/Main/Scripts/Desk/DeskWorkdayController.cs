using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NarrativeGP.Desk
{
    public class DeskWorkdayController : MonoBehaviour
    {
        [System.Serializable]
        private class DayTaskSet
        {
            public int day = 1;
            public List<DeskTaskData> tasks = new();
        }

        [Header("Day Tasks")]
        [SerializeField] private int tasksPerDay = 5;
        [SerializeField] private List<DeskTaskData> currentDayTasks = new();
        [SerializeField] private List<DayTaskSet> dayTaskSets = new();
        [SerializeField] private DeskTaskData completedStateTask;

        [Header("Dependencies")]
        [SerializeField] private GameState gameState;
        [SerializeField] private DeskTaskController deskTaskController;

        [Header("Progress UI")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TMP_Text progressLabel;

        [Header("Events")]
        [SerializeField] private UnityEvent onWorkdayCompleted;

        private int currentTaskIndex;
        private int completedTaskCount;
        private int loadedDay = -1;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void Awake()
        {
            if (deskTaskController != null)
            {
                deskTaskController.TaskSubmitted += HandleTaskSubmitted;
            }

            RefreshWorkdayForCurrentGameDay(true);
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.StateChanged += HandleGameStateChanged;
            }

            RefreshWorkdayForCurrentGameDay(false);
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.StateChanged -= HandleGameStateChanged;
            }
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
            SyncSectionProgress();
        }

        [ContextMenu("Reload Day Tasks")]
        public void ReloadDayTasks()
        {
            RefreshWorkdayForCurrentGameDay(true);
        }

        public void SyncSectionProgressToGameState(GameState targetGameState)
        {
            if (targetGameState == null)
            {
                return;
            }

            RefreshWorkdayForCurrentGameDay(false);

            bool hasIncompleteTasks = completedTaskCount < GetTaskTargetCount();
            targetGameState.SetSectionDailyProgress(
                SectionId.Desk,
                hasIncompleteTasks,
                !hasIncompleteTasks);
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
            SyncSectionProgress();

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

        private void SyncSectionProgress()
        {
            SyncSectionProgressToGameState(gameState);
        }

        private void HandleGameStateChanged()
        {
            RefreshWorkdayForCurrentGameDay(false);
        }

        private void RefreshWorkdayForCurrentGameDay(bool forceReload)
        {
            int currentDay = gameState != null ? Mathf.Max(1, gameState.CurrentDay) : 1;
            if (!forceReload && loadedDay == currentDay)
            {
                return;
            }

            loadedDay = currentDay;

            if (dayTaskSets != null && dayTaskSets.Count > 0)
            {
                DayTaskSet matchedSet = null;
                foreach (DayTaskSet taskSet in dayTaskSets)
                {
                    if (taskSet != null && taskSet.day == currentDay)
                    {
                        matchedSet = taskSet;
                        break;
                    }
                }

                currentDayTasks = matchedSet != null
                    ? new List<DeskTaskData>(matchedSet.tasks)
                    : new List<DeskTaskData>();
            }

            StartWorkday();
        }
    }
}
