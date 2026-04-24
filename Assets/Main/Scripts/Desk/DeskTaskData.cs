using System;
using UnityEngine;

namespace NarrativeGP.Desk
{
    [Serializable]
    public class DeskTaskData
    {
        public string id;
        [TextArea(2, 5)]
        public string prompt;
        public Sprite image;
        public string correctPattern = "000000000";
        public bool isInteractive = true;
    }
}
