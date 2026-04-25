using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace NarrativeGP.DayTransition
{
    public class SignInController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameState gameState;
        [SerializeField] private SectionScreenController sectionScreenController;

        [Header("Scenario")]
        [SerializeField] private SignInScenarioData defaultScenario = new();
        [SerializeField] private SignInScenarioData endingScenario = new()
        {
            scenarioId = "ending",
            field1Label = "User Name",
            field1TargetText = "Ben",
            field2Label = "Password",
            field2TargetText = "****",
            checkboxLabel = "I'm not human",
            requiredInputCount = 7,
            completionMode = SignInCompletionMode.GoToEndAnim
        };

        [Header("UI")]
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private TMP_Text field1LabelText;
        [SerializeField] private TMP_Text field1ValueText;
        [SerializeField] private TMP_Text field2LabelText;
        [SerializeField] private TMP_Text field2ValueText;
        [SerializeField] private Toggle robotToggle;
        [SerializeField] private TMP_Text checkboxLabelText;
        [SerializeField] private Button signInButton;

        private SignInScenarioData activeScenario;
        private int currentInputCount;
        private bool isVisible;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
            sectionScreenController = FindFirstObjectByType<SectionScreenController>();
        }

        private void Awake()
        {
            SetVisible(false);
            RefreshView();
        }

        private void Update()
        {
            if (!isVisible)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                RegisterInput();
            }
        }

        public void Show()
        {
            Show(defaultScenario);
        }

        public void ShowEnding()
        {
            Show(endingScenario);
        }

        public void Show(SignInScenarioData scenario)
        {
            activeScenario = scenario ?? defaultScenario;
            currentInputCount = 0;

            if (robotToggle != null)
            {
                robotToggle.SetIsOnWithoutNotify(false);
                robotToggle.interactable = false;
            }

            SetVisible(true);
            RefreshView();
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void RegisterInput()
        {
            if (!isVisible || activeScenario == null)
            {
                return;
            }

            if (currentInputCount >= GetRequiredInputCount())
            {
                return;
            }

            currentInputCount++;
            RefreshView();
        }

        public void OnToggleChanged(bool isOn)
        {
            RefreshView();
        }

        public void OnSignInPressed()
        {
            if (!CanSubmit())
            {
                return;
            }

            if (activeScenario != null && activeScenario.completionMode == SignInCompletionMode.GoToEndAnim)
            {
                Debug.Log("SignIn complete. Go to EndAnim.");
                SetVisible(false);
                return;
            }

            if (gameState != null)
            {
                gameState.StartNewDay();
            }

            if (sectionScreenController != null)
            {
                sectionScreenController.OpenSection(SectionId.Attendance);
            }

            SetVisible(false);
        }

        private void RefreshView()
        {
            SignInScenarioData scenario = activeScenario ?? defaultScenario;

            SetText(field1LabelText, scenario.field1Label);
            SetText(field2LabelText, scenario.field2Label);
            SetText(checkboxLabelText, scenario.checkboxLabel);

            int field1VisibleCount = Mathf.Clamp(currentInputCount, 0, scenario.field1TargetText?.Length ?? 0);
            int remainingCount = Mathf.Max(0, currentInputCount - field1VisibleCount);
            int field2VisibleCount = Mathf.Clamp(remainingCount, 0, scenario.field2TargetText?.Length ?? 0);

            SetText(field1ValueText, GetVisibleSubstring(scenario.field1TargetText, field1VisibleCount));
            SetText(field2ValueText, GetVisibleSubstring(scenario.field2TargetText, field2VisibleCount));

            if (robotToggle != null)
            {
                robotToggle.interactable = currentInputCount >= GetRequiredInputCount();
            }

            if (signInButton != null)
            {
                signInButton.interactable = CanSubmit();
            }
        }

        private bool CanSubmit()
        {
            return isVisible
                && activeScenario != null
                && currentInputCount >= GetRequiredInputCount()
                && robotToggle != null
                && robotToggle.isOn;
        }

        private int GetRequiredInputCount()
        {
            return activeScenario != null ? Mathf.Max(0, activeScenario.requiredInputCount) : 0;
        }

        private void SetVisible(bool visible)
        {
            isVisible = visible;

            if (visualRoot != null)
            {
                visualRoot.SetActive(visible);
            }
        }

        private static string GetVisibleSubstring(string value, int visibleCount)
        {
            if (string.IsNullOrEmpty(value) || visibleCount <= 0)
            {
                return string.Empty;
            }

            int clampedCount = Mathf.Clamp(visibleCount, 0, value.Length);
            return value.Substring(0, clampedCount);
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }
    }
}
