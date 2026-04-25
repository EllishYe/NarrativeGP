using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.DayTransition
{
    public class PowerDownUpController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameState gameState;
        [SerializeField] private SignInController signInController;

        [Header("UI")]
        [SerializeField] private Graphic blackBackground;
        [SerializeField] private TMP_Text poweringDownText;
        [SerializeField] private TMP_Text poweringUpText;

        [Header("Timing")]
        [SerializeField] private float powerDownDuration = 3f;
        [SerializeField] private float powerUpDuration = 3f;

        [Header("Flow")]
        [SerializeField] private int maxDay = 3;

        private Coroutine activeSequence;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void Awake()
        {
            SetPanelVisible(false);
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
            SetPanelVisible(true);
            SetStage(isPoweringDown: true);
            yield return new WaitForSecondsRealtime(powerDownDuration);

            SetStage(isPoweringDown: false);
            yield return new WaitForSecondsRealtime(powerUpDuration);

            SetPanelVisible(false);
            activeSequence = null;

            int currentDay = gameState != null ? Mathf.Max(1, gameState.CurrentDay) : 1;
            if (currentDay < maxDay)
            {
                Debug.Log("PowerDownUp complete. Go to SignIn.");
                if (signInController != null)
                {
                    signInController.Show();
                }
                yield break;
            }

            Debug.Log("PowerDownUp complete. Go to ending SignIn.");
            if (signInController != null)
            {
                signInController.ShowEnding();
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (blackBackground != null)
            {
                blackBackground.gameObject.SetActive(visible);
            }

            if (!visible)
            {
                if (poweringDownText != null)
                {
                    poweringDownText.gameObject.SetActive(false);
                }

                if (poweringUpText != null)
                {
                    poweringUpText.gameObject.SetActive(false);
                }
            }
        }

        private void SetStage(bool isPoweringDown)
        {
            if (poweringDownText != null)
            {
                poweringDownText.gameObject.SetActive(isPoweringDown);
            }

            if (poweringUpText != null)
            {
                poweringUpText.gameObject.SetActive(!isPoweringDown);
            }
        }
    }
}
