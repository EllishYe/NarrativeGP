using UnityEngine;
using UnityEngine.EventSystems;

namespace NarrativeGP.DayTransition
{
    public class SignInInputCatcher : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private SignInController signInController;

        private void Reset()
        {
            signInController = GetComponentInParent<SignInController>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            signInController?.RegisterInput();
        }
    }
}
