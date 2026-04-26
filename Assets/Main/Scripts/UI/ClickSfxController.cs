using UnityEngine;
using UnityEngine.InputSystem;

namespace NarrativeGP
{
    public class ClickSfxController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickClip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private bool playOnLeftClick = true;
        [SerializeField] private bool playOnRightClick;

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            EnsureAudioSource();
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            bool didClick = (playOnLeftClick && mouse.leftButton.wasPressedThisFrame)
                || (playOnRightClick && mouse.rightButton.wasPressedThisFrame);

            if (didClick)
            {
                PlayClick();
            }
        }

        public void PlayClick()
        {
            if (audioSource == null || clickClip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clickClip, volume);
        }

        private void EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }
}
