using System.Collections.Generic;
using UnityEngine;

namespace NarrativeGP
{
    [CreateAssetMenu(menuName = "NarrativeGP/Section Flow Definition")]
    public class SectionFlowDefinition : ScriptableObject
    {
        [SerializeField] private List<SectionId> daytimeSections = new()
        {
            SectionId.Attendance,
            SectionId.Desk,
            SectionId.Emails,
            SectionId.News
        };

        [SerializeField] private SectionId wrapUpSection = SectionId.Log;

        public IReadOnlyList<SectionId> DaytimeSections => daytimeSections;
        public SectionId WrapUpSection => wrapUpSection;
    }
}
