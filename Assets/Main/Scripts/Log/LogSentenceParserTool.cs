using UnityEngine;

namespace NarrativeGP.Logs
{
    public class LogSentenceParserTool : MonoBehaviour
    {
        [TextArea(4, 10)]
        [SerializeField] private string rawText;

        [SerializeField] private LogSentenceData parsedSentence = new();

        [SerializeField] private string parseStatus;

        public LogSentenceData ParsedSentence => parsedSentence;

        [ContextMenu("Parse Raw Text")]
        public void ParseRawText()
        {
            parsedSentence = LogSentenceParser.Parse(rawText);
            parseStatus = $"Parsed {parsedSentence.segments.Count} segments.";
        }

        [ContextMenu("Clear Parsed Result")]
        public void ClearParsedResult()
        {
            parsedSentence = new LogSentenceData();
            parseStatus = "Cleared.";
        }
    }
}
