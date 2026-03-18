// WallBehavior.cs
using UnityEngine;

public class WallBehavior : MonoBehaviour
{
    public float speed = 10f;
    void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime);
        if (transform.position.z < -1.5f) Destroy(gameObject);
    }

    // Player's collider triggers this if they don't dodge
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //GameManager.Instance.RegisterWallHit();
        }
    }
}