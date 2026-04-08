using UnityEngine;
namespace BeatSaberVR
{
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioSource sfxSource;
        public AudioClip slashClip;

        private void OnEnable()
        {
            BeatSaberVREvents.OnPlaySlashAudio += PlaySlash;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnPlaySlashAudio -= PlaySlash;
        }

        public void PlaySlash()
        {
            if (slashClip != null) sfxSource.PlayOneShot(slashClip);
        }
    }
}