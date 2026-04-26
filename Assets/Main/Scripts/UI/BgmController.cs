using UnityEngine;

namespace NarrativeGP
{
    public class BgmController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip bgm01;
        [SerializeField] private AudioClip bgm02;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private bool playBgm01OnStart = true;

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            EnsureAudioSource();
            ConfigureAudioSource();
        }

        private void Start()
        {
            if (playBgm01OnStart)
            {
                PlayBgm01();
            }
        }

        public void PlayBgm01()
        {
            PlayLoop(bgm01);
        }

        public void StopBgm01()
        {
            if (audioSource == null || audioSource.clip != bgm01)
            {
                return;
            }

            audioSource.Stop();
        }

        public void StopCurrentBgm()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        public void PlayBgm02()
        {
            PlayLoop(bgm02);
        }

        private void PlayLoop(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            EnsureAudioSource();
            ConfigureAudioSource();

            if (audioSource.clip == clip && audioSource.isPlaying)
            {
                return;
            }

            audioSource.clip = clip;
            audioSource.Play();
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
        }

        private void ConfigureAudioSource()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.volume = volume;
        }
    }
}
