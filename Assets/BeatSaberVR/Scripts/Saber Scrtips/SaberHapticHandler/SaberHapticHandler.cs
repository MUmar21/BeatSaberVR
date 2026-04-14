using BeatSaberVR;
using UnityEngine;
using UnityEngine.XR;

public class SaberHapticHandler : MonoBehaviour
{
    [Header("Hand Setup")]
    [SerializeField] private BlockColor saberColor;
    private XRNode node;

    [Header("Block Hit (Sharp Click)")]
    [SerializeField] private float blockIntensity = 0.8f;
    [SerializeField] private float blockDuration = 0.05f;

    [Header("Saber Clash (Buzzing)")]
    [SerializeField] private float clashIntensity = 0.4f;

    [Header("Wall Hit (Heavy Rumble)")]
    [SerializeField] private float wallIntensity = 0.3f;
    [SerializeField] private float wallDuration = 0.1f;


    private void Awake()
    {
        node = (saberColor == BlockColor.Red) ? XRNode.RightHand : XRNode.LeftHand;
    }

    private void OnEnable()
    {
        BeatSaberVREvents.OnBlockCut += HandleBlockCut;
        BeatSaberVREvents.OnWallHit += HandleWallHit;
    }

    private void OnDisable()
    {
        BeatSaberVREvents.OnBlockCut -= HandleBlockCut;
        BeatSaberVREvents.OnWallHit -= HandleWallHit;
    }

    private void HandleBlockCut(BlockColor color)
    {
        bool isRedMatch = (color == BlockColor.Red && saberColor == BlockColor.Red);
        bool isBlueMatch = (color == BlockColor.Blue && saberColor == BlockColor.Blue);

        if (isRedMatch || isBlueMatch)
        {
            TriggerHaptic(blockIntensity, blockDuration);
        }
    }

    private void HandleWallHit()
    {
        TriggerHaptic(wallIntensity, wallDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Saber"))
        {
            TriggerHaptic(clashIntensity, 0.08f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Saber"))
        {
            // Continuous light buzz while grinding sabers together
            TriggerHaptic(clashIntensity * 0.5f, Time.fixedDeltaTime);
        }
    }

    public void TriggerHaptic(float intensity, float duration)
    {
        //if (Application.isEditor)
        //Debug.Log($"Triggering haptic on {node} with intensity {intensity} and duration {duration}");

        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (device.isValid)
        {
            //Debug.Log($"Sending haptic impulse to {node}");
            device.SendHapticImpulse(0, intensity, duration);
        }
    }
}
