using System.Collections;
using TMPro;
using UnityEngine;

namespace NarrativeGP.Ending
{
    public class EndAnimController : MonoBehaviour
    {
        [Header("UI Stages")]
        [SerializeField] private GameObject blueScreenStage;
        [SerializeField] private GameObject blackScreenStage;
        [SerializeField] private GameObject slideshowStage;

        [Header("Blue Screen UI")]
        [SerializeField] private TMP_Text staticTextPrimary;
        [SerializeField] private TMP_Text staticTextSecondary;
        [SerializeField] private TMP_Text progressText;

        [Header("Timing")]
        [SerializeField] private float blueScreenDuration = 3f;
        [SerializeField] private float blackScreenDuration = 3f;

        private Coroutine activeSequence;

        private void Awake()
        {
            SetStageVisibility(showBlue: false, showBlack: false, showSlideshow: false);
        }

        public void PlaySequence()
        {
            if (activeSequence != null)
            {
                return;
            }

            activeSequence = StartCoroutine(RunSequence());
        }

        private IEnumerator RunSequence()
        {
            SetStageVisibility(showBlue: true, showBlack: false, showSlideshow: false);

            float elapsed = 0f;
            while (elapsed < blueScreenDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = blueScreenDuration > 0f ? Mathf.Clamp01(elapsed / blueScreenDuration) : 1f;
                int percent = Mathf.RoundToInt(progress * 100f);
                UpdateProgressText(percent);
                yield return null;
            }

            UpdateProgressText(100);
            SetStageVisibility(showBlue: false, showBlack: true, showSlideshow: false);
            yield return new WaitForSecondsRealtime(blackScreenDuration);

            SetStageVisibility(showBlue: false, showBlack: false, showSlideshow: false);
            activeSequence = null;
            Debug.Log("EndAnim phase 1 complete. Ready for slideshow.");
        }

        private void UpdateProgressText(int percent)
        {
            if (progressText != null)
            {
                progressText.text = $"{percent}% complete";
            }
        }

        private void SetStageVisibility(bool showBlue, bool showBlack, bool showSlideshow)
        {
            if (blueScreenStage != null)
            {
                blueScreenStage.SetActive(showBlue);
            }

            if (blackScreenStage != null)
            {
                blackScreenStage.SetActive(showBlack);
            }

            if (slideshowStage != null)
            {
                slideshowStage.SetActive(showSlideshow);
            }

            if (!showBlue && progressText != null)
            {
                progressText.text = string.Empty;
            }
        }
    }
}
