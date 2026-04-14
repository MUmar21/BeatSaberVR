using System;

namespace BeatSaberVR
{
    public static class BeatSaberVREvents
    {
        public static Action OnGameStart;
        public static Action OnGameplayEnd;
        public static Action OnWallHit;
        public static Action OnTriggerGameOver;
        public static Action OnChoiceMade;
        public static Action OnBlockMiss;
        public static Action<int> OnGameOver;
        public static Action<int> OnAddScore;
        public static Action<BlockColor> OnBlockCut;
    }
}