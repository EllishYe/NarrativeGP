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
        public int arrivalDay = 1;
        public int sortOrder;
        [UnityEngine.TextArea(8, 20)]
        public string body;
    }
}
