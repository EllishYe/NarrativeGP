using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.Emails
{
    public class EmailsController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private List<EmailData> emails = new();
        [SerializeField] private List<EmailListItemView> listItemViews = new();

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
        private string selectedMailId;

        private void Awake()
        {
            BuildLookups();

            foreach (EmailListItemView listItemView in listItemViews)
            {
                if (listItemView == null)
                {
                    continue;
                }

                listItemView.Initialize(this);

                if (emailLookup.TryGetValue(listItemView.EmailId, out EmailData email))
                {
                    listItemView.SetTitle(email.subject);
                    continue;
                }

                listItemView.SetTitle(string.Empty);
            }
        }

        private void OnEnable()
        {
            EnsureSelectedMail();
            RefreshView();
        }

        public void SelectEmail(string emailId)
        {
            if (string.IsNullOrWhiteSpace(emailId) || !emailLookup.ContainsKey(emailId))
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
            EnsureSelectedMail();
            RefreshListItems();
            RefreshDetailPanel();
            RefreshMarkAsReadButton();
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

        private void EnsureSelectedMail()
        {
            if (!string.IsNullOrWhiteSpace(selectedMailId) && emailLookup.ContainsKey(selectedMailId))
            {
                return;
            }

            foreach (EmailData email in emails)
            {
                if (email != null && !string.IsNullOrWhiteSpace(email.id))
                {
                    selectedMailId = email.id;
                    return;
                }
            }

            selectedMailId = string.Empty;
        }

        private void RefreshListItems()
        {
            foreach (EmailListItemView listItemView in listItemViews)
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
            if (!string.IsNullOrWhiteSpace(selectedMailId) && emailLookup.TryGetValue(selectedMailId, out email))
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
    }
}
