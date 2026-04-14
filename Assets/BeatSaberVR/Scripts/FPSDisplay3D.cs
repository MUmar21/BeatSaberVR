using TMPro;
using UnityEngine;

namespace GOSIVR
{
    public class FPSDisplay3D : MonoBehaviour
    {
        private float deltaTime;

        private TextMeshPro _Text;

        private void Start()
        {
            //Application.targetFrameRate = 72;

            _Text = GetComponent<TextMeshPro>();
        }

        private void LateUpdate()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

            float fps = 1f / deltaTime;

            _Text.text = fps.ToString("F2");
        }
    }
}