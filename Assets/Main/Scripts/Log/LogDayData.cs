using System;
using System.Collections.Generic;

namespace NarrativeGP.Logs
{
    [Serializable]
    public class LogDayData
    {
        public int day = 1;
        public List<LogSentenceData> sentences = new();
    }
}
