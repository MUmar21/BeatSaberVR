
using UnityEngine;

namespace BeatSaberVR
{
    public enum BlockColor { Red, Blue }   // Left = Red, Right = Blue
    public enum CutDirection { Up, Down, Left, Right, Any }

    public class GameManager : Singleton<GameManager>
    {
        private int score = 0;
        private int combo = 0;

        public void AddScore(int points)
        {
            combo++;
            int total = points * combo; // combo multiplier
            score += total;
            Debug.Log($"Score: {score}  Combo: {combo}x");
        }

        public void RegisterMiss()
        {
            combo = 0; // reset combo on miss
            Debug.Log("Miss! Combo reset.");
        }

        public void RegisterWallHit() { }
    }
}