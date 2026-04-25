using System;
using System.Collections.Generic;
using System.Text;

namespace NarrativeGP.Logs
{
    public static class LogSentenceParser
    {
        public static LogSentenceData Parse(string rawText)
        {
            LogSentenceData sentenceData = new();
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return sentenceData;
            }

            int index = 0;
            StringBuilder textBuffer = new();

            while (index < rawText.Length)
            {
                char currentChar = rawText[index];

                if (currentChar == '[')
                {
                    FlushTextBuffer(sentenceData, textBuffer);
                    int closingIndex = FindMatchingBracket(rawText, index);
                    if (closingIndex < 0)
                    {
                        AppendPlainTextToken(sentenceData, rawText[index..]);
                        break;
                    }

                    string blankContent = rawText.Substring(index + 1, closingIndex - index - 1);
                    LogSegmentData blankSegment = BuildBlankSegment(blankContent);
                    sentenceData.segments.Add(blankSegment);
                    index = closingIndex + 1;
                    continue;
                }

                if (char.IsWhiteSpace(currentChar))
                {
                    FlushTextBuffer(sentenceData, textBuffer);
                    index++;
                    continue;
                }

                textBuffer.Append(currentChar);
                index++;
            }

            FlushTextBuffer(sentenceData, textBuffer);
            return sentenceData;
        }

        private static int FindMatchingBracket(string text, int startIndex)
        {
            for (int i = startIndex + 1; i < text.Length; i++)
            {
                if (text[i] == ']')
                {
                    return i;
                }
            }

            return -1;
        }

        private static LogSegmentData BuildBlankSegment(string blankContent)
        {
            LogSegmentData segmentData = new()
            {
                segmentType = LogSegmentType.Blank
            };

            string[] rawOptions = blankContent.Split('/');
            int detectedCorrectIndex = -1;

            for (int i = 0; i < rawOptions.Length; i++)
            {
                string trimmedOption = rawOptions[i].Trim();
                if (trimmedOption.StartsWith("(") && trimmedOption.EndsWith(")"))
                {
                    trimmedOption = trimmedOption[1..^1].Trim();
                    detectedCorrectIndex = segmentData.options.Count;
                }

                if (!string.IsNullOrWhiteSpace(trimmedOption))
                {
                    segmentData.options.Add(trimmedOption);
                }
            }

            segmentData.correctIndex = detectedCorrectIndex >= 0 ? detectedCorrectIndex : 0;
            return segmentData;
        }

        private static void FlushTextBuffer(LogSentenceData sentenceData, StringBuilder textBuffer)
        {
            if (textBuffer.Length == 0)
            {
                return;
            }

            AppendPlainTextToken(sentenceData, textBuffer.ToString());
            textBuffer.Clear();
        }

        private static void AppendPlainTextToken(LogSentenceData sentenceData, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            sentenceData.segments.Add(new LogSegmentData
            {
                segmentType = LogSegmentType.Text,
                text = token
            });
        }
    }
}
