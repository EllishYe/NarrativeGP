using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NarrativeGP.Logs
{
    public class LogController : MonoBehaviour
    {
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
        [SerializeField] private float saveDelaySeconds = 5f;
        [SerializeField] private string mismatchFeedbackText = "Entries do not match today's records";
        [SerializeField] private string successFeedbackText = "Saving...\nSynchronizing...";

        [Header("Events")]
        [SerializeField] private UnityEvent onSaveSequenceCompleted;

        private readonly List<LogBlankView> spawnedBlankViews = new();
        private readonly List<GameObject> spawnedSentenceObjects = new();
        private Coroutine saveSequenceCoroutine;
        private bool isSaving;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void OnEnable()
        {
            RebuildCurrentDayLog();
        }

        private void OnDisable()
        {
            StopActiveSaveSequence();
        }

        [ContextMenu("Rebuild Current Day Log")]
        public void RebuildCurrentDayLog()
        {
            StopActiveSaveSequence();
            ClearGeneratedContent();

            LogDayData currentDayData = GetCurrentDayData();
            if (currentDayData == null || logContentRoot == null || sentencePrefab == null)
            {
                SetFeedbackText(string.Empty);
                return;
            }

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

                    CreateBlankSegment(sentenceRoot, segmentData);
                }
            }

            SetFeedbackText(string.Empty);
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
            blankInstance.Bind(segmentData.options, segmentData.correctIndex);
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
            int currentDay = gameState != null ? Mathf.Max(1, gameState.CurrentDay) : 1;

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
    }
}
