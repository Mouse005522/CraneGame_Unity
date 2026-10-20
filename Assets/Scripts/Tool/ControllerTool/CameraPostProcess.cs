using UnityEngine;

namespace Kun.Tool
{
    public class CameraPostProcess : MonoBehaviour
    {
        public Material postProcessMat;

        [SerializeField]
        RenderTexture secondCameraTexture;

        void OnRenderImage (RenderTexture source, RenderTexture destination)
        {
            if (postProcessMat != null)
            {
                postProcessMat.SetTexture ("_MainTex", source);
                postProcessMat.SetTexture ("_SecondCameraTex", secondCameraTexture);

                Graphics.Blit (source, destination, postProcessMat);
            }
            else
            {
                Graphics.Blit (source, destination);
            };
        }
    }
}