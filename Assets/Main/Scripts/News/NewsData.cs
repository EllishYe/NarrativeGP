using System;
using System.Collections.Generic;
using UnityEngine;

namespace NarrativeGP.News
{
    [Serializable]
    public class NewsData
    {
        [Serializable]
        public class BodyVersionEntry
        {
            [TextArea(6, 20)]
            public string text;
        }

        public string id;
        public string listTitle;
        public string displayTitle;
        public Sprite image;
        public List<BodyVersionEntry> bodyVersions = new();
    }
}
