using UnityEngine;
using UnityEngine.UI;

namespace Kun.Tool
{
    /// <summary>
    /// 專為 VR 專案設計的外螢幕攝影機管理器
    /// 功能：自動比例縮放、手動拍照渲染、Editor/Release 效能分流
    /// </summary>
    public class SpectatorCameraSystem : MonoBehaviour
    {
        [SerializeField]
        Camera spectatorCamera;

        [Header ("只是同步到螢幕上不會畫到全滿")]
        [SerializeField]
        int targetFPS = 30;

        [Header ("超過會被等比例拉回來")]
        [SerializeField]
        float maxDimension = 1920f;

        public RenderTexture RT;

        [SerializeField]
        float _timer;

        public void Init ()
        {
            if (spectatorCamera != null) 
            {
                spectatorCamera.enabled = false;

                spectatorCamera.allowHDR = false; // 減少頻寬壓力
            }

            UpdateRT ();
        }

        void LateUpdate ()
        {
            if (Application.isEditor)
            {
                if (Screen.width != RT.width || Screen.height != RT.height)
                {
                    UpdateRT ();
                }
            }

            _timer += Time.deltaTime;
            if (_timer >= 1f / targetFPS)
            {
                spectatorCamera.ResetWorldToCameraMatrix ();
                spectatorCamera.Render ();
                _timer = 0;
            }
        }

        private void UpdateRT ()
        {
            int tw = Screen.width;
            int th = Screen.height;

            // 等比例縮放計算
            if (tw > maxDimension || th > maxDimension)
            {
                float scale = (tw > th) ? maxDimension / tw : maxDimension / th;
                tw = (Mathf.RoundToInt (tw * scale) / 2) * 2;
                th = (Mathf.RoundToInt (th * scale) / 2) * 2;
            }

            if (RT == null)
            {
                RT = new RenderTexture (tw, th, 24);

                //(禁用 MSAA)。

                RT.antiAliasing = 1;
            }
            else if (RT.width != tw || RT.height != th)
            {
                RT.Release (); // 釋放舊的 GPU 記憶體
                RT.width = tw;
                RT.height = th;
            }

            // 2. 關鍵：告訴攝影機，現在的畫幅比例變了！
            // 這樣攝影機拍出來的圓形才不會變橢圓
            spectatorCamera.aspect = (float)tw / th;

            RT.antiAliasing = QualitySettings.antiAliasing > 0 ? QualitySettings.antiAliasing : 2;

            spectatorCamera.targetTexture = RT;
        }

        private void OnDestroy ()
        {
            if (RT != null)
            {
                RT.Release ();
                Destroy (RT); // 最終銷毀
            }
        }
    }
}