using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NarrativeGP.Logs
{
    public class LogController : MonoBehaviour
    {
        [System.Serializable]
        private class LogDayRuntimeState
        {
            public int day;
            public List<int> blankValues = new();
            public bool isSaved;
        }

        [Header("Dependencies")]
        [SerializeField] private GameState gameState;

        [Header("Data")]
        [SerializeField] private List<LogDayData> dayLogs = new();

        [Header("UI")]
        [SerializeField] private Transform logContentRoot;
        [SerializeField] private Transform sentencePrefab;
        [SerializeField] private TMP_Text textSegmentPrefab;
        [SerializeField] private LogBlankView blankSegmentPrefab;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private float saveDelaySeconds = 3f;
        [SerializeField] private string mismatchFeedbackText = "Entries do not match today's records";
        [SerializeField] private string successFeedbackText = "Saving...\nSynchronizing...";

        [Header("Events")]
        [SerializeField] private UnityEvent onSaveSequenceCompleted;

        [Header("Runtime State")]
        [SerializeField] private List<LogDayRuntimeState> runtimeStates = new();

        private readonly List<LogBlankView> spawnedBlankViews = new();
        private readonly List<GameObject> spawnedSentenceObjects = new();
        private readonly Dictionary<int, LogDayRuntimeState> runtimeLookup = new();
        private Coroutine saveSequenceCoroutine;
        private bool isSaving;
        private int loadedDay = -1;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void Awake()
        {
            BuildRuntimeLookup();
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.StateChanged += HandleGameStateChanged;
            }

            RebuildCurrentDayLog();
        }

        private void OnDisable()
        {
            StopActiveSaveSequence();

            if (gameState != null)
            {
                gameState.StateChanged -= HandleGameStateChanged;
            }
        }

        [ContextMenu("Rebuild Current Day Log")]
        public void RebuildCurrentDayLog()
        {
            StopActiveSaveSequence();
            SetContentRootVisible(false);
            ClearGeneratedContent();

            LogDayData currentDayData = GetCurrentDayData();
            loadedDay = GetCurrentDay();
            if (currentDayData == null || logContentRoot == null || sentencePrefab == null)
            {
                SetFeedbackText(string.Empty);
                SetContentRootVisible(true);
                return;
            }

            LogDayRuntimeState runtimeState = GetOrCreateRuntimeState(loadedDay);
            int blankIndex = 0;

            foreach (LogSentenceData sentenceData in currentDayData.sentences)
            {
                if (sentenceData == null)
                {
                    continue;
                }

                Transform sentenceRoot = Instantiate(sentencePrefab, logContentRoot);
                spawnedSentenceObjects.Add(sentenceRoot.gameObject);

                foreach (LogSegmentData segmentData in sentenceData.segments)
                {
                    if (segmentData == null)
                    {
                        continue;
                    }

                    if (segmentData.segmentType == LogSegmentType.Text)
                    {
                        CreateTextSegment(sentenceRoot, segmentData.text);
                        continue;
                    }

                    EnsureRuntimeBlankSlot(runtimeState, blankIndex);
                    CreateBlankSegment(sentenceRoot, segmentData, blankIndex, runtimeState.blankValues[blankIndex]);
                    blankIndex++;
                }
            }

            SetFeedbackText(string.Empty);
            ForceImmediateLayout();
            SetContentRootVisible(true);
        }

        public bool AreAllEntriesCorrect()
        {
            foreach (LogBlankView blankView in spawnedBlankViews)
            {
                if (blankView == null || !blankView.IsCorrect())
                {
                    return false;
                }
            }

            return true;
        }

        public void OnSaveLog()
        {
            if (isSaving)
            {
                return;
            }

            if (!AreAllEntriesCorrect())
            {
                SetFeedbackText(mismatchFeedbackText);
                return;
            }

            saveSequenceCoroutine = StartCoroutine(RunSaveSequence());
        }

        public void SetFeedbackText(string value)
        {
            if (feedbackText != null)
            {
                feedbackText.text = value;
            }
        }

        private void CreateTextSegment(Transform sentenceRoot, string value)
        {
            if (textSegmentPrefab == null)
            {
                return;
            }

            TMP_Text textInstance = Instantiate(textSegmentPrefab, sentenceRoot);
            textInstance.text = value ?? string.Empty;
        }

        private void CreateBlankSegment(Transform sentenceRoot, LogSegmentData segmentData)
        {
            if (blankSegmentPrefab == null)
            {
                return;
            }

            LogBlankView blankInstance = Instantiate(blankSegmentPrefab, sentenceRoot);
            blankInstance.Bind(segmentData.options, segmentData.correctIndex, 0, null);
            spawnedBlankViews.Add(blankInstance);
        }

        private void ClearGeneratedContent()
        {
            for (int i = spawnedSentenceObjects.Count - 1; i >= 0; i--)
            {
                if (spawnedSentenceObjects[i] != null)
                {
                    Destroy(spawnedSentenceObjects[i]);
                }
            }

            spawnedSentenceObjects.Clear();
            spawnedBlankViews.Clear();
        }

        private LogDayData GetCurrentDayData()
        {
            int currentDay = GetCurrentDay();

            foreach (LogDayData dayData in dayLogs)
            {
                if (dayData != null && dayData.day == currentDay)
                {
                    return dayData;
                }
            }

            return null;
        }

        private IEnumerator RunSaveSequence()
        {
            isSaving = true;
            SetFeedbackText(successFeedbackText);
            yield return new WaitForSeconds(saveDelaySeconds);

            GetOrCreateRuntimeState(GetCurrentDay()).isSaved = true;
            SyncSectionProgress();
            isSaving = false;
            saveSequenceCoroutine = null;
            Debug.Log("Log save sequence completed.");
            onSaveSequenceCompleted?.Invoke();
        }

        private void StopActiveSaveSequence()
        {
            if (saveSequenceCoroutine != null)
            {
                StopCoroutine(saveSequenceCoroutine);
                saveSequenceCoroutine = null;
            }

            isSaving = false;
        }

        private void HandleGameStateChanged()
        {
            int currentDay = GetCurrentDay();
            if (currentDay != loadedDay)
            {
                RebuildCurrentDayLog();
                return;
            }

            SyncSectionProgress();
        }

        private void BuildRuntimeLookup()
        {
            runtimeLookup.Clear();

            foreach (LogDayRuntimeState runtimeState in runtimeStates)
            {
                if (runtimeState != null)
                {
                    runtimeLookup[runtimeState.day] = runtimeState;
                }
            }
        }

        private LogDayRuntimeState GetOrCreateRuntimeState(int day)
        {
            if (runtimeLookup.TryGetValue(day, out LogDayRuntimeState runtimeState))
            {
                return runtimeState;
            }

            runtimeState = new LogDayRuntimeState
            {
                day = day
            };

            runtimeStates.Add(runtimeState);
            runtimeLookup.Add(day, runtimeState);
            return runtimeState;
        }

        private static void EnsureRuntimeBlankSlot(LogDayRuntimeState runtimeState, int blankIndex)
        {
            while (runtimeState.blankValues.Count <= blankIndex)
            {
                runtimeState.blankValues.Add(0);
            }
        }

        private void CreateBlankSegment(Transform sentenceRoot, LogSegmentData segmentData, int blankIndex, int initialValue)
        {
            if (blankSegmentPrefab == null)
            {
                return;
            }

            LogBlankView blankInstance = Instantiate(blankSegmentPrefab, sentenceRoot);
            blankInstance.Bind(
                segmentData.options,
                segmentData.correctIndex,
                initialValue,
                value => UpdateRuntimeBlankValue(blankIndex, value));
            spawnedBlankViews.Add(blankInstance);
        }

        private void UpdateRuntimeBlankValue(int blankIndex, int value)
        {
            LogDayRuntimeState runtimeState = GetOrCreateRuntimeState(GetCurrentDay());
            EnsureRuntimeBlankSlot(runtimeState, blankIndex);
            runtimeState.blankValues[blankIndex] = value;
        }

        private int GetCurrentDay()
        {
            return gameState != null ? Mathf.Max(1, gameState.CurrentDay) : 1;
        }

        public void SyncSectionProgressToGameState(GameState targetGameState)
        {
            if (targetGameState == null)
            {
                return;
            }

            int currentDay = Mathf.Max(1, targetGameState.CurrentDay);
            LogDayRuntimeState runtimeState = GetOrCreateRuntimeState(currentDay);
            bool hasIncompleteLog = !runtimeState.isSaved;

            targetGameState.SetSectionDailyProgress(
                SectionId.Log,
                hasIncompleteLog,
                !hasIncompleteLog);
        }

        private void SyncSectionProgress()
        {
            SyncSectionProgressToGameState(gameState);
        }

        private void SetContentRootVisible(bool visible)
        {
            if (logContentRoot != null)
            {
                logContentRoot.gameObject.SetActive(visible);
            }
        }

        private void ForceImmediateLayout()
        {
            if (logContentRoot is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}
