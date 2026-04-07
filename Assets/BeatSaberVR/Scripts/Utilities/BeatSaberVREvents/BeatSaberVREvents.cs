using System;

namespace BeatSaberVR
{
    public static class BeatSaberVREvents
    {
        public static Action OnGameStart;
        public static Action OnGameplayEnd;
        public static Action OnWallHit;
        public static Action OnTriggerGameOver;
        public static Action<int> OnGameOver;
        public static Action<BlockColor> OnBlockSpawned;
        public static Action<BlockColor> OnBlockCut;
        public static Action<Swords> OnSwordSelected;
    }
}