using UnityEngine;

namespace NarrativeGP
{
    public class SectionButtonLink : MonoBehaviour
    {
        [SerializeField] private SectionScreenController sectionScreenController;
        [SerializeField] private SectionId sectionId;

        public void OpenAssignedSection()
        {
            if (sectionScreenController == null)
            {
                return;
            }

            sectionScreenController.OpenSection(sectionId);
        }
    }
}
