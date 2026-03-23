using UnityEngine;
namespace BeatSaberVR
{
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioSource sfxSource;
        public AudioClip slashClip;

        public void PlaySlash()
        {
            if (slashClip != null) sfxSource.PlayOneShot(slashClip);
        }
    }
}