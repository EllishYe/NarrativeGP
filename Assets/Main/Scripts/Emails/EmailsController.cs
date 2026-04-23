using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.Emails
{
    public class EmailsController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameState gameState;

        [Header("Data")]
        [SerializeField] private List<EmailData> emails = new();

        [Header("List UI")]
        [SerializeField] private Transform mailListContent;
        [SerializeField] private EmailListItemView mailItemPrefab;

        [Header("Detail UI")]
        [SerializeField] private TMP_Text subjectText;
        [SerializeField] private TMP_Text senderText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button markAsReadButton;
        [SerializeField] private TMP_Text markAsReadButtonLabel;
        [SerializeField] private string markAsReadText = "Mark as Read";
        [SerializeField] private string readText = "Read";

        [Header("Runtime State")]
        [SerializeField] private List<string> readEmailIds = new();

        private readonly Dictionary<string, EmailData> emailLookup = new();
        private readonly HashSet<string> readLookup = new();
        private readonly List<EmailData> visibleEmails = new();
        private readonly List<EmailListItemView> spawnedItemViews = new();
        private string selectedMailId;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void Awake()
        {
            BuildLookups();
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.StateChanged += HandleGameStateChanged;
            }

            RefreshView();
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.StateChanged -= HandleGameStateChanged;
            }
        }

        public void SelectEmail(string emailId)
        {
            if (string.IsNullOrWhiteSpace(emailId) || !visibleEmails.Any(email => email.id == emailId))
            {
                return;
            }

            selectedMailId = emailId;
            RefreshView();
        }

        public void MarkSelectedAsRead()
        {
            if (!TryGetSelectedEmail(out EmailData selectedEmail))
            {
                return;
            }

            if (!readLookup.Add(selectedEmail.id))
            {
                return;
            }

            readEmailIds.Add(selectedEmail.id);
            RefreshView();
        }

        [ContextMenu("Refresh Emails")]
        public void RefreshView()
        {
            BuildVisibleEmailList();
            EnsureSelectedMail();
            RebuildListItems();
            RefreshListItems();
            RefreshDetailPanel();
            RefreshMarkAsReadButton();
            SyncSectionProgress();
        }

        public void SyncSectionProgressToGameState(GameState targetGameState)
        {
            if (targetGameState == null)
            {
                return;
            }

            int currentDay = Mathf.Max(1, targetGameState.CurrentDay);
            bool hasUnreadNewEmailsToday = false;

            foreach (EmailData email in emails)
            {
                if (email == null || string.IsNullOrWhiteSpace(email.id))
                {
                    continue;
                }

                if (email.arrivalDay != currentDay)
                {
                    continue;
                }

                if (!readLookup.Contains(email.id))
                {
                    hasUnreadNewEmailsToday = true;
                    break;
                }
            }

            targetGameState.SetSectionDailyProgress(
                SectionId.Emails,
                hasUnreadNewEmailsToday,
                !hasUnreadNewEmailsToday);
        }

        private void BuildLookups()
        {
            emailLookup.Clear();
            readLookup.Clear();

            foreach (EmailData email in emails)
            {
                if (email == null || string.IsNullOrWhiteSpace(email.id))
                {
                    continue;
                }

                emailLookup[email.id] = email;
            }

            foreach (string readEmailId in readEmailIds)
            {
                if (!string.IsNullOrWhiteSpace(readEmailId))
                {
                    readLookup.Add(readEmailId);
                }
            }
        }

        private void BuildVisibleEmailList()
        {
            visibleEmails.Clear();

            int currentDay = gameState != null ? Mathf.Max(1, gameState.CurrentDay) : 1;
            foreach (EmailData email in emails)
            {
                if (email == null || string.IsNullOrWhiteSpace(email.id))
                {
                    continue;
                }

                if (email.arrivalDay <= currentDay)
                {
                    visibleEmails.Add(email);
                }
            }

            visibleEmails.Sort(CompareEmails);
        }

        private void EnsureSelectedMail()
        {
            if (!string.IsNullOrWhiteSpace(selectedMailId) && visibleEmails.Any(email => email.id == selectedMailId))
            {
                return;
            }

            foreach (EmailData email in visibleEmails)
            {
                if (email != null && !string.IsNullOrWhiteSpace(email.id))
                {
                    selectedMailId = email.id;
                    return;
                }
            }

            selectedMailId = string.Empty;
        }

        private void RebuildListItems()
        {
            for (int i = spawnedItemViews.Count - 1; i >= 0; i--)
            {
                if (spawnedItemViews[i] != null)
                {
                    Destroy(spawnedItemViews[i].gameObject);
                }
            }

            spawnedItemViews.Clear();

            if (mailListContent == null || mailItemPrefab == null)
            {
                return;
            }

            foreach (EmailData email in visibleEmails)
            {
                EmailListItemView itemView = Instantiate(mailItemPrefab, mailListContent);
                itemView.Initialize(this);
                itemView.Bind(email.id, email.subject);
                spawnedItemViews.Add(itemView);
            }
        }

        private void RefreshListItems()
        {
            foreach (EmailListItemView listItemView in spawnedItemViews)
            {
                if (listItemView == null)
                {
                    continue;
                }

                bool isRead = readLookup.Contains(listItemView.EmailId);
                bool isSelected = listItemView.EmailId == selectedMailId;
                listItemView.SetState(isRead, isSelected);
            }
        }

        private void RefreshDetailPanel()
        {
            if (!TryGetSelectedEmail(out EmailData selectedEmail))
            {
                ApplyBlankDetail();
                return;
            }

            SetText(subjectText, selectedEmail.subject);
            SetText(senderText, selectedEmail.sender);
            SetText(dateText, selectedEmail.dateText);
            SetText(bodyText, selectedEmail.body);
        }

        private void RefreshMarkAsReadButton()
        {
            bool hasSelectedEmail = TryGetSelectedEmail(out EmailData selectedEmail);
            bool isRead = hasSelectedEmail && readLookup.Contains(selectedEmail.id);

            if (markAsReadButton != null)
            {
                markAsReadButton.interactable = hasSelectedEmail && !isRead;
            }

            if (markAsReadButtonLabel != null)
            {
                markAsReadButtonLabel.text = isRead ? readText : markAsReadText;
            }
        }

        private bool TryGetSelectedEmail(out EmailData email)
        {
            if (!string.IsNullOrWhiteSpace(selectedMailId) && emailLookup.TryGetValue(selectedMailId, out email) && visibleEmails.Any(item => item.id == selectedMailId))
            {
                return true;
            }

            email = null;
            return false;
        }

        private void ApplyBlankDetail()
        {
            SetText(subjectText, string.Empty);
            SetText(senderText, string.Empty);
            SetText(dateText, string.Empty);
            SetText(bodyText, string.Empty);
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private void HandleGameStateChanged()
        {
            RefreshView();
        }

        private void SyncSectionProgress()
        {
            SyncSectionProgressToGameState(gameState);
        }

        private static int CompareEmails(EmailData left, EmailData right)
        {
            int arrivalComparison = right.arrivalDay.CompareTo(left.arrivalDay);
            if (arrivalComparison != 0)
            {
                return arrivalComparison;
            }

            int sortOrderComparison = left.sortOrder.CompareTo(right.sortOrder);
            if (sortOrderComparison != 0)
            {
                return sortOrderComparison;
            }

            return string.CompareOrdinal(left.id, right.id);
        }
    }
}
