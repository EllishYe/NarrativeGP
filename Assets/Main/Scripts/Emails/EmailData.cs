using System;

namespace NarrativeGP.Emails
{
    [Serializable]
    public class EmailData
    {
        public string id;
        public string subject;
        public string sender;
        public string dateText;
        [UnityEngine.TextArea(8, 20)]
        public string body;
    }
}
