using System;

namespace NarrativeGP.Attendance
{
    [Serializable]
    public class AttendanceRecord
    {
        public string dateIso;
        public AttendanceStatus status = AttendanceStatus.Pending;
        public AttendanceTimeValue clockIn;
        public AttendanceTimeValue clockOut;
        public string hoursWorkedText = "-";
        public string note = "-";
    }
}
