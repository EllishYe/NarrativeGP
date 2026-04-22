using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.Attendance
{
    public class AttendanceController : MonoBehaviour
    {
        private enum DateRelation
        {
            Past,
            Today,
            Future
        }

        [Header("Dependencies")]
        [SerializeField] private GameState gameState;

        [Header("Game Date")]
        [SerializeField] private string firstWorkDateIso = "2026-04-18";

        [Header("Data")]
        [SerializeField] private List<AttendanceRecord> records = new();
        [SerializeField] private List<AttendanceCalendarDateButton> calendarButtons = new();

        [Header("Detail UI")]
        [SerializeField] private TMP_Text dateValueText;
        [SerializeField] private TMP_Text statusValueText;
        [SerializeField] private TMP_Text clockInValueText;
        [SerializeField] private TMP_Text clockOutValueText;
        [SerializeField] private TMP_Text hoursWorkedValueText;
        [SerializeField] private TMP_Text noteValueText;
        [SerializeField] private Button checkInButton;

        [Header("Calendar Colors")]
        [SerializeField] private Color defaultBackgroundColor = Color.white;
        [SerializeField] private Color defaultTextColor = Color.black;
        [SerializeField] private Color todaySelectedBackgroundColor = Color.red;
        [SerializeField] private Color todaySelectedTextColor = Color.white;
        [SerializeField] private Color todayUnselectedBackgroundColor = Color.white;
        [SerializeField] private Color todayUnselectedTextColor = Color.red;
        [SerializeField] private Color selectedBackgroundColor = Color.black;
        [SerializeField] private Color selectedTextColor = Color.white;
        [SerializeField] private Color futureBackgroundColor = new(0.85f, 0.85f, 0.85f, 1f);
        [SerializeField] private Color futureTextColor = new(0.45f, 0.45f, 0.45f, 1f);

        [Header("Runtime Check-In Defaults")]
        [SerializeField] private AttendanceTimeValue defaultCheckInTime = new()
        {
            hasValue = true,
            hour = 9,
            minute = 2
        };

        private readonly Dictionary<string, AttendanceRecord> recordLookup = new();
        private string selectedDateIso;
        private string runtimeDateIso;
        private bool hasCheckedInToday;
        private AttendanceTimeValue todayClockInTime;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void Awake()
        {
            BuildLookup();
            SyncRuntimeDate();

            foreach (AttendanceCalendarDateButton calendarButton in calendarButtons)
            {
                if (calendarButton != null)
                {
                    calendarButton.Initialize(this);
                }
            }
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.StateChanged += HandleGameStateChanged;
            }

            SelectCurrentGameDate();
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.StateChanged -= HandleGameStateChanged;
            }
        }

        public void SelectDate(string dateIso)
        {
            if (!TryParseDate(dateIso, out DateTime date))
            {
                return;
            }

            if (GetDateRelation(date) == DateRelation.Future)
            {
                return;
            }

            selectedDateIso = dateIso;
            RefreshView();
        }

        public void CheckIn()
        {
            DateTime currentGameDate = GetCurrentGameDate();
            SyncRuntimeDate();

            if (!CanCheckIn(currentGameDate))
            {
                return;
            }

            hasCheckedInToday = true;
            todayClockInTime = defaultCheckInTime;
            selectedDateIso = currentGameDate.ToString("yyyy-MM-dd");
            RefreshView();

            if (gameState != null)
            {
                gameState.MarkSectionCompleted(SectionId.Attendance);
            }
        }

        [ContextMenu("Refresh Attendance")]
        public void RefreshView()
        {
            SyncRuntimeDate();

            if (string.IsNullOrWhiteSpace(selectedDateIso))
            {
                SelectCurrentGameDate();
                return;
            }

            RefreshCalendarButtons();
            RefreshDetailPanel();
            RefreshCheckInButton();
        }

        private void SelectCurrentGameDate()
        {
            selectedDateIso = GetCurrentGameDate().ToString("yyyy-MM-dd");
            RefreshView();
        }

        private void HandleGameStateChanged()
        {
            RefreshView();
        }

        private void SyncRuntimeDate()
        {
            string currentDateIso = GetCurrentGameDate().ToString("yyyy-MM-dd");
            if (runtimeDateIso == currentDateIso)
            {
                return;
            }

            runtimeDateIso = currentDateIso;
            hasCheckedInToday = false;
            todayClockInTime = new AttendanceTimeValue();
        }

        private void BuildLookup()
        {
            recordLookup.Clear();

            foreach (AttendanceRecord record in records)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.dateIso))
                {
                    continue;
                }

                recordLookup[record.dateIso] = record;
            }
        }

        private void RefreshCalendarButtons()
        {
            DateTime currentGameDate = GetCurrentGameDate();

            foreach (AttendanceCalendarDateButton calendarButton in calendarButtons)
            {
                if (calendarButton == null || !TryParseDate(calendarButton.DateIso, out DateTime buttonDate))
                {
                    continue;
                }

                DateRelation relation = GetDateRelation(buttonDate);
                bool isSelected = calendarButton.DateIso == selectedDateIso;
                bool isToday = SameDate(buttonDate, currentGameDate);

                if (relation == DateRelation.Future)
                {
                    calendarButton.SetVisuals(futureBackgroundColor, futureTextColor, false);
                    continue;
                }

                if (isToday && isSelected)
                {
                    calendarButton.SetVisuals(todaySelectedBackgroundColor, todaySelectedTextColor, true);
                    continue;
                }

                if (isToday)
                {
                    calendarButton.SetVisuals(todayUnselectedBackgroundColor, todayUnselectedTextColor, true);
                    continue;
                }

                if (isSelected)
                {
                    calendarButton.SetVisuals(selectedBackgroundColor, selectedTextColor, true);
                    continue;
                }

                calendarButton.SetVisuals(defaultBackgroundColor, defaultTextColor, true);
            }
        }

        private void RefreshDetailPanel()
        {
            if (!TryParseDate(selectedDateIso, out DateTime selectedDate))
            {
                ApplyBlankDetail();
                return;
            }

            DateRelation relation = GetDateRelation(selectedDate);
            if (relation == DateRelation.Future)
            {
                ApplyBlankDetail();
                return;
            }

            if (relation == DateRelation.Today)
            {
                ApplyTodayDetail(selectedDate);
                return;
            }

            if (!TryGetRecord(selectedDate, out AttendanceRecord record))
            {
                ApplyBlankDetail(selectedDateIso);
                return;
            }

            ApplyStoredDetail(record);
        }

        private void RefreshCheckInButton()
        {
            if (checkInButton == null)
            {
                return;
            }

            DateTime currentGameDate = GetCurrentGameDate();
            bool canCheckIn = CanCheckIn(currentGameDate);
            checkInButton.interactable = canCheckIn && selectedDateIso == currentGameDate.ToString("yyyy-MM-dd");
        }

        private bool CanCheckIn(DateTime currentGameDate)
        {
            if (!TryParseDate(selectedDateIso, out DateTime selectedDate))
            {
                return false;
            }

            if (!SameDate(selectedDate, currentGameDate))
            {
                return false;
            }

            return !hasCheckedInToday;
        }

        private void ApplyTodayDetail(DateTime currentGameDate)
        {
            string currentDateIso = currentGameDate.ToString("yyyy-MM-dd");

            if (!hasCheckedInToday)
            {
                ApplyPendingDetail(currentDateIso);
                return;
            }

            ApplyCheckedInDetail(currentDateIso);
        }

        private void ApplyPendingDetail(string dateIso)
        {
            SetDetailText(dateValueText, dateIso);
            SetDetailText(statusValueText, AttendanceStatus.Pending.ToString());
            SetDetailText(clockInValueText, "-:-");
            SetDetailText(clockOutValueText, "-:-");
            SetDetailText(hoursWorkedValueText, "-");
            SetDetailText(noteValueText, "-");
        }

        private void ApplyCheckedInDetail(string dateIso)
        {
            SetDetailText(dateValueText, dateIso);
            SetDetailText(statusValueText, AttendanceStatus.Attended.ToString());
            SetDetailText(clockInValueText, todayClockInTime.ToDisplayString());
            SetDetailText(clockOutValueText, "-:-");
            SetDetailText(hoursWorkedValueText, "In Progress");
            SetDetailText(noteValueText, "-");
        }

        private void ApplyStoredDetail(AttendanceRecord record)
        {
            SetDetailText(dateValueText, record.dateIso);
            SetDetailText(statusValueText, record.status.ToString());
            SetDetailText(clockInValueText, record.clockIn.ToDisplayString());
            SetDetailText(clockOutValueText, record.clockOut.ToDisplayString());
            SetDetailText(hoursWorkedValueText, string.IsNullOrWhiteSpace(record.hoursWorkedText) ? "-" : record.hoursWorkedText);
            SetDetailText(noteValueText, string.IsNullOrWhiteSpace(record.note) ? "-" : record.note);
        }

        private void ApplyBlankDetail(string dateText = "")
        {
            SetDetailText(dateValueText, dateText);
            SetDetailText(statusValueText, string.Empty);
            SetDetailText(clockInValueText, string.Empty);
            SetDetailText(clockOutValueText, string.Empty);
            SetDetailText(hoursWorkedValueText, string.Empty);
            SetDetailText(noteValueText, string.Empty);
        }

        private static void SetDetailText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private bool TryGetRecord(DateTime date, out AttendanceRecord record)
        {
            return recordLookup.TryGetValue(date.ToString("yyyy-MM-dd"), out record);
        }

        private DateTime GetCurrentGameDate()
        {
            if (!TryParseDate(firstWorkDateIso, out DateTime firstWorkDate))
            {
                return DateTime.Today;
            }

            int dayOffset = gameState != null ? Mathf.Max(0, gameState.CurrentDay - 1) : 0;
            return firstWorkDate.AddDays(dayOffset);
        }

        private DateRelation GetDateRelation(DateTime date)
        {
            DateTime currentGameDate = GetCurrentGameDate();

            if (SameDate(date, currentGameDate))
            {
                return DateRelation.Today;
            }

            return date < currentGameDate ? DateRelation.Past : DateRelation.Future;
        }

        private static bool SameDate(DateTime left, DateTime right)
        {
            return left.Year == right.Year && left.Month == right.Month && left.Day == right.Day;
        }

        private static bool TryParseDate(string dateIso, out DateTime date)
        {
            return DateTime.TryParseExact(
                dateIso,
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out date);
        }
    }
}
