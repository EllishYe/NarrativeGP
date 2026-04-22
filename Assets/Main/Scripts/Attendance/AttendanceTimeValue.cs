using System;

namespace NarrativeGP.Attendance
{
    [Serializable]
    public struct AttendanceTimeValue
    {
        public bool hasValue;
        public int hour;
        public int minute;

        public string ToDisplayString()
        {
            return hasValue ? $"{hour:00}:{minute:00}" : "-:-";
        }
    }
}
