using System.Collections;
using UnityEngine;

public class PlayerSetupPosition : MonoBehaviour
{
    [SerializeField] private Vector3 startPosition;

    IEnumerator Start()
    {
        yield return null;
        gameObject.transform.position = startPosition;
    }
}
