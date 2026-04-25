using System;
using System.Collections.Generic;

namespace NarrativeGP.Logs
{
    [Serializable]
    public class LogSentenceData
    {
        public string rawText;
        public List<LogSegmentData> segments = new();

        public void ParseRawText()
        {
            LogSentenceData parsedSentence = LogSentenceParser.Parse(rawText);
            segments = parsedSentence.segments ?? new List<LogSegmentData>();
        }
    }
}
