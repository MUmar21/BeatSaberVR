using System;
using System.Collections;
using UnityEngine;

namespace BeatSaberVR
{
    public class PooledParticle : MonoBehaviour
    {
        private ParticleSystem ps;
        private Action<PooledParticle> returnAction;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        public void Play(Vector3 position, Action<PooledParticle> onDone)
        {
            transform.position = position;
            returnAction = onDone;
            gameObject.SetActive(true);
            ps.Clear();
            ps.Play();
            StartCoroutine(WaitAndReturn());
        }

        IEnumerator WaitAndReturn()
        {
            // Wait until all particles have died
            yield return new WaitUntil(() => !ps.IsAlive(true));
            gameObject.SetActive(false);
            returnAction?.Invoke(this);
        }
    }
}