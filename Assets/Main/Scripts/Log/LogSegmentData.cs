using System;
using System.Collections.Generic;
using UnityEngine;

namespace NarrativeGP.Logs
{
    [Serializable]
    public class LogSegmentData
    {
        public LogSegmentType segmentType;

        [TextArea(1, 5)]
        public string text;

        public List<string> options = new();
        public int correctIndex;
    }
}
