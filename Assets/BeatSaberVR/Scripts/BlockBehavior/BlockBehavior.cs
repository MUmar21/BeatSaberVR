using UnityEngine;

public enum BlockColor { Left, Right }   // Left = Red, Right = Blue
public enum CutDirection { Up, Down, Left, Right, Any }

public class BlockBehavior : MonoBehaviour
{
    public BlockColor blockColor;
    public CutDirection cutDirection;
    public Transform directionPoint;
    public float speed = 10f;

    private bool wasHit = false;

    void Update()
    {
        // Move block toward the player
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // Destroy if it passed the player without being hit (= miss)
        if (transform.position.z < -1.5f && !wasHit)
        {
            //GameManager.Instance.RegisterMiss();
            Destroy(gameObject);
        }
    }

    public void OnSuccessfulHit()
    {
        wasHit = true;
        // Spawn particle effect here later
        //GameManager.Instance.AddScore(100);
        Destroy(gameObject);
    }

    public void SetDirectionPoint(CutDirection cutDirection)
    {
        if (directionPoint == null) return;

        float zAngle;
        switch (cutDirection)
        {
            case CutDirection.Up:
                zAngle = 0f;
                break;
            case CutDirection.Down:
                zAngle = 180f;
                break;
            case CutDirection.Left:
                zAngle = 90f;
                break;
            case CutDirection.Right:
                zAngle = 270f;
                break;
            case CutDirection.Any:
            default:
                zAngle = directionPoint.localEulerAngles.z;
                break;
        }

        directionPoint.localRotation = Quaternion.Euler(0f, 0f, zAngle);
    }
}