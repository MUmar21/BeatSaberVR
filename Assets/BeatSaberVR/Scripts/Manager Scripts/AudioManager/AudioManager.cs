using UnityEngine;
namespace BeatSaberVR
{
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioSource sfxSource;
        public AudioClip slashClip;
        public AudioClip missClip;

        private void OnEnable()
        {
            BeatSaberVREvents.OnBlockCut += PlaySlash;
            BeatSaberVREvents.OnBlockMiss += PlayMiss;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnBlockCut -= PlaySlash;
            BeatSaberVREvents.OnBlockMiss -= PlayMiss;
        }

        public void PlaySlash(BlockColor color)
        {
            PlaySfx(slashClip);
        }

        public void PlayMiss()
        {
            PlaySfx(missClip);
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null) sfxSource.PlayOneShot(clip);
        }
    }
}