using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace Kun.XR
{
    public static class XRInput
    {
        private static List<InputDevice> devices = new List<InputDevice>();

        static Vector3? leftHandAngleOffset = null;
        static Vector3? rightHandAngleOffset = null;
        private static InputDevice primaryLeftController;
        private static InputDevice primaryRightController;
        private static InputFeatureUsage<Vector2> thumbstickAxis;
        private static InputFeatureUsage<Vector2> thumbstickAxisSecondary;
        private static InputFeatureUsage<bool> thumbstickAxisClick;

        // 記錄每個軸的方向狀態
        private static bool leftTouchPadRightFlag, leftTouchPadLeftFlag, leftTouchPadUpFlag, leftTouchPadDownFlag;
        private static bool leftThumbstickRightFlag, leftThumbstickLeftFlag, leftThumbstickUpFlag, leftThumbstickDownFlag;
        private static bool rightTouchPadRightFlag, rightTouchPadLeftFlag, rightTouchPadUpFlag, rightTouchPadDownFlag;
        private static bool rightThumbstickRightFlag, rightThumbstickLeftFlag, rightThumbstickUpFlag, rightThumbstickDownFlag;

        // 軸向變化閾值
        public const float AxisLowThreshold = 0.3f;
        public const float AxisHighThreshold = 0.7f;

        public static bool SupportsBothTouchPadAndJoystick { get; private set; }

        public static Vector3 LeftVelocity { get; private set; }
        public static Vector3 RightVelocity { get; private set; }

        public static bool LeftPrimaryBtnUp { get; private set; }
        public static bool LeftPrimaryBtnDown { get; private set; }
        public static bool LeftPrimaryBtnPress { get; private set; }
        public static bool RightPrimaryBtnUp { get; private set; }
        public static bool RightPrimaryBtnDown { get; private set; }
        public static bool RightPrimaryBtnPress { get; private set; }

        public static bool LeftSecondaryBtnUp { get; private set; }
        public static bool LeftSecondaryBtnDown { get; private set; }
        public static bool LeftSecondaryBtnPress { get; private set; }
        public static bool RightSecondaryBtnUp { get; private set; }
        public static bool RightSecondaryBtnDown { get; private set; }
        public static bool RightSecondaryBtnPress { get; private set; }


        public static bool LeftThumbstickPress { get; private set; }
        public static bool LeftThumbstickDown { get; private set; }
        public static bool LeftThumbstickUp { get; private set; }
        public static bool RightThumbstickPress { get; private set; }
        public static bool RightThumbstickDown { get; private set; }
        public static bool RightThumbstickUp { get; private set; }
        public static Vector2 LeftTouchPadAxis { get; private set; }
        public static Vector2 LeftThumbstickAxis { get; private set; }
        public static Vector2 RightTouchPadAxis { get; private set; }
        public static Vector2 RightThumbstickAxis { get; private set; }

        // 新增的 Down 版本屬性
        public static Vector2Int LeftTouchPadAxisDown { get; private set; }
        public static Vector2Int LeftThumbstickAxisDown { get; private set; }
        public static Vector2Int RightTouchPadAxisDown { get; private set; }
        public static Vector2Int RightThumbstickAxisDown { get; private set; }

        public static bool LeftGripPress
        {
            get
            {
                return LeftGripPressure >= DownThreshold;
            }
        }
        public static float LeftGripPressure { get; private set; }
        public static bool LeftGripDown { get; private set; }
        public static bool LeftGripUp { get; private set; }
        public static bool RightGripPress
        {
            get
            {
                return RightGripPressure >= DownThreshold;
            }
        }
        public static float RightGripPressure { get; private set; }
        public static bool RightGripDown { get; private set; }
        public static bool RightGripUp { get; private set; }


        public static float RightTriggerPressure { get; private set; }
        public static bool RightTriggerPress
        {
            get
            {
                return RightTriggerPressure >= DownThreshold;
            }
        }
        public static bool RightTriggerDown { get; private set; }
        public static bool RightTriggerUp { get; private set; }
        public static float LeftTriggerPressure { get; private set; }
        public static bool LeftTriggerPress
        {
            get
            {
                return LeftTriggerPressure >= DownThreshold;
            }
        }
        public static bool LeftTriggerDown { get; private set; }
        public static bool LeftTriggerUp { get; private set; }

        public static float ThumbstickDeadzoneX = 0.001f;
        public static float ThumbstickDeadzoneY = 0.001f;

        /// <summary>
        /// 按鍵按下的分水嶺
        /// </summary>
        public static float DownThreshold { get; private set; } = 0.5f;

        /// <summary>
        /// 依據裝置以及sdk提供修正值,
        /// 舉例來說oculus如果用openXR來開啟會有65度的偏移值
        /// </summary>
        public static void SetHandAngleOffset (Vector3 leftHandAngleOffset, Vector3 rightHandAngleOffset)
        {
            XRInput.leftHandAngleOffset = leftHandAngleOffset;
            XRInput.rightHandAngleOffset = rightHandAngleOffset;
        }

        #region 六軸偵測
        /// <summary>
        /// 刷新位置與旋轉
        /// </summary>
        public static void UpdatePose(XRNode node, Transform target)
        {
            Pose pose;

            var hasPose = GetPose (node, out pose);

            if (target != null && hasPose)
            {
                target.localPosition = pose.position;
                target.localRotation = pose.rotation;
            }
            else
            {
                target.localPosition = Vector3.zero;
                target.localRotation = Quaternion.identity;
            }
        }

        public static bool GetPose (XRNode node, out Pose pose)
        {
            var device = InputDevices.GetDeviceAtXRNode (node);

            if (device.isValid)
            {
                bool hasPos = device.TryGetFeatureValue (CommonUsages.devicePosition, out Vector3 pos);
                bool hasRot = device.TryGetFeatureValue (CommonUsages.deviceRotation, out Quaternion rot);

                if (hasPos && pos == Vector3.zero)
                {
                    hasPos = false;
                }

                if (hasRot && rot == Quaternion.identity)
                {
                    hasRot = false;
                }

                if (hasPos && hasRot) 
                {
                    Quaternion processRot;

                    if (node == XRNode.LeftHand && leftHandAngleOffset.HasValue)
                    {
                        processRot = rot * Quaternion.Euler (leftHandAngleOffset.Value);
                    }
                    else if (node == XRNode.RightHand && rightHandAngleOffset.HasValue)
                    {
                        processRot = rot  * Quaternion.Euler (rightHandAngleOffset.Value);;
                    }
                    else
                    {
                        processRot = rot; 
                    }

                    pose = new Pose (pos, processRot);
                    return true;
                }
                else
                {
                    pose = default;
                    return false;
                }
            }
            else
            {
                pose = default;
                return false;
            }
        }

        public static Vector2 GetAxis(XRNode node)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid)
            {
                bool hasAxis = device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis);
                if (hasAxis)
                {
                    return axis;
                }
            }
            else
            {
                Debug.LogError("Device not valid");
            }
            return default;
        }
        #endregion

        #region 輸入偵測
        public static bool IsDeviceValid(XRNode node)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid)
            {
                return true;
            }
            else
            {
                //Debug.Log($"Device {node.ToString()} is not valid.");
                return false;
            }
        }
        /// <summary>
        /// 刷新輸入
        /// </summary>
        public static void UpdateInput()
        {
            InputDevices.GetDevices(devices);

            primaryLeftController = GetLeftController();
            primaryRightController = GetRightController();

            thumbstickAxis = SupportsBothTouchPadAndJoystick ? CommonUsages.secondary2DAxis : CommonUsages.primary2DAxis;
            thumbstickAxisSecondary = SupportsBothTouchPadAndJoystick ? CommonUsages.primary2DAxis : CommonUsages.secondary2DAxis;
            thumbstickAxisClick = SupportsBothTouchPadAndJoystick ? CommonUsages.secondary2DAxisClick : CommonUsages.primary2DAxisClick;

            //控制器速度
            LeftVelocity = GetFeatureUsage(primaryLeftController, CommonUsages.deviceVelocity);
            RightVelocity = GetFeatureUsage(primaryRightController, CommonUsages.deviceVelocity);

            //左蘑菇頭按鈕
            var prevBool = LeftThumbstickPress;
            LeftThumbstickPress = GetFeatureUsage(primaryLeftController, thumbstickAxisClick);
            LeftThumbstickDown = prevBool == false && LeftThumbstickPress == true;
            LeftThumbstickUp = prevBool == true && LeftThumbstickPress == false;

            //右蘑菇頭按鈕
            prevBool = RightThumbstickPress;
            RightThumbstickPress = GetFeatureUsage(primaryRightController, thumbstickAxisClick);
            RightThumbstickDown = prevBool == false && RightThumbstickPress == true;
            RightThumbstickUp = prevBool == true && RightThumbstickPress == false;

            //左蘑菇頭偏移
            LeftTouchPadAxis = ApplyDeadZones(GetFeatureUsage(primaryLeftController, thumbstickAxisSecondary), ThumbstickDeadzoneX, ThumbstickDeadzoneY);
            LeftThumbstickAxis = ApplyDeadZones(GetFeatureUsage(primaryLeftController, thumbstickAxis), ThumbstickDeadzoneX, ThumbstickDeadzoneY);

            //右蘑菇頭偏移
            RightTouchPadAxis = ApplyDeadZones(GetFeatureUsage(primaryRightController, thumbstickAxisSecondary), ThumbstickDeadzoneX, ThumbstickDeadzoneY);
            RightThumbstickAxis = ApplyDeadZones(GetFeatureUsage(primaryRightController, thumbstickAxis), ThumbstickDeadzoneX, ThumbstickDeadzoneY);

            //軸向變化偵測
            LeftTouchPadAxisDown = GetAxisDirectionChange(LeftTouchPadAxis, 
                ref leftTouchPadRightFlag, ref leftTouchPadLeftFlag, ref leftTouchPadUpFlag, ref leftTouchPadDownFlag);
            LeftThumbstickAxisDown = GetAxisDirectionChange(LeftThumbstickAxis,
                ref leftThumbstickRightFlag, ref leftThumbstickLeftFlag, ref leftThumbstickUpFlag, ref leftThumbstickDownFlag);
            RightTouchPadAxisDown = GetAxisDirectionChange(RightTouchPadAxis,
                ref rightTouchPadRightFlag, ref rightTouchPadLeftFlag, ref rightTouchPadUpFlag, ref rightTouchPadDownFlag);

            RightThumbstickAxisDown = GetAxisDirectionChange(RightThumbstickAxis,
                ref rightThumbstickRightFlag, ref rightThumbstickLeftFlag, ref rightThumbstickUpFlag, ref rightThumbstickDownFlag);

            //左握持鍵
            prevBool = LeftGripPress;
            LeftGripPressure = SyncopatedValue(GetFeatureUsage(primaryLeftController, CommonUsages.grip));
            LeftGripDown = prevBool == false && LeftGripPress == true;
            LeftGripUp = prevBool == true && LeftGripPress == false;

            //右握持鍵
            prevBool = RightGripPress;
            RightGripPressure = SyncopatedValue(GetFeatureUsage(primaryRightController, CommonUsages.grip));
            RightGripDown = prevBool == false && RightGripPress == true;
            RightGripUp = prevBool == true && RightGripPress == false;

            //左觸發鍵
            prevBool = LeftTriggerPress;
            LeftTriggerPressure = SyncopatedValue(GetFeatureUsage(primaryLeftController, CommonUsages.trigger));
            LeftTriggerDown = prevBool == false && LeftTriggerPress == true;
            LeftTriggerUp = prevBool == true && LeftTriggerPress == false;

            //右觸發鍵
            prevBool = RightTriggerPress;
            RightTriggerPressure = SyncopatedValue(GetFeatureUsage(primaryRightController, CommonUsages.trigger));
            RightTriggerDown = prevBool == false && RightTriggerPress == true;
            RightTriggerUp = prevBool == true && RightTriggerPress == false;

            //左主要按鈕(X)
            prevBool = LeftPrimaryBtnPress;
            LeftPrimaryBtnPress = GetFeatureUsage(primaryLeftController, CommonUsages.primaryButton);
            LeftPrimaryBtnDown = prevBool == false && LeftPrimaryBtnPress == true;
            LeftPrimaryBtnUp = prevBool == true && LeftPrimaryBtnPress == false;

            //右主要按鈕(A)
            prevBool = RightPrimaryBtnPress;
            RightPrimaryBtnPress = GetFeatureUsage(primaryRightController, CommonUsages.primaryButton);
            RightPrimaryBtnDown = prevBool == false && RightPrimaryBtnPress == true;
            RightPrimaryBtnUp = prevBool == true && RightPrimaryBtnPress == false;

            //左次要按鈕(Y)
            prevBool = LeftSecondaryBtnPress;
            LeftSecondaryBtnPress = GetFeatureUsage(primaryLeftController, CommonUsages.secondaryButton);
            LeftSecondaryBtnDown = prevBool == false && LeftSecondaryBtnPress == true;
            LeftSecondaryBtnUp = prevBool == true && LeftSecondaryBtnPress == false;

            //右次要按鈕(B)
            prevBool = RightSecondaryBtnPress;
            RightSecondaryBtnPress = GetFeatureUsage(primaryRightController, CommonUsages.secondaryButton);
            RightSecondaryBtnDown = prevBool == false && RightSecondaryBtnPress == true;
            RightSecondaryBtnUp = prevBool == true && RightSecondaryBtnPress == false;

        }

        public static InputDevice GetLeftController()
        {
            InputDevices.GetDevices(devices);

            var leftHandedControllers = new List<InputDevice>();
            var dc = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(dc, leftHandedControllers);
            return leftHandedControllers.FirstOrDefault();
        }

        public static InputDevice GetRightController()
        {
            InputDevices.GetDevices(devices);

            var rightHandedControllers = new List<InputDevice>();
            var dc = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(dc, rightHandedControllers);

            return rightHandedControllers.FirstOrDefault();
        }
        #endregion

        #region 內部功能
        private static float SyncopatedValue(float inputValue)
        {
            return (float)System.Math.Round(inputValue * 1000f) / 1000f;
        }
        private static Vector2 ApplyDeadZones(Vector2 pos, float deadZoneX, float deadZoneY)
        {

            if (Mathf.Abs(pos.x) < deadZoneX)
            {
                pos.x = 0f;
            }

            if (Mathf.Abs(pos.y) < deadZoneY)
            {
                pos.y = 0f;
            }

            return pos;
        }

        /// <summary>
        /// 處理單軸的方向偵測
        /// </summary>
        /// <param name="value">當前軸向值</param>
        /// <param name="positiveFlag">正方向旗標</param>
        /// <param name="negativeFlag">負方向旗標</param>
        /// <returns>方向變化：1為正方向，-1為負方向，0為無變化</returns>
        private static int ProcessSingleAxis(float value, ref bool positiveFlag, ref bool negativeFlag)
        {
            int result = 0;

            // 正方向偵測
            if (!positiveFlag && value >= AxisHighThreshold)
            {
                positiveFlag = true;
                result = 1;
            }
            else if (positiveFlag && value <= AxisLowThreshold)
            {
                positiveFlag = false;
            }

            // 負方向偵測
            if (!negativeFlag && value <= -AxisHighThreshold)
            {
                negativeFlag = true;
                result = -1;
            }
            else if (negativeFlag && value >= -AxisLowThreshold)
            {
                negativeFlag = false;
            }

            return result;
        }

        /// <summary>
        /// 偵測軸向變化並回傳方向，使用旗標記錄進出狀態
        /// </summary>
        /// <param name="currentValue">當前軸向值</param>
        /// <param name="rightFlag">右方向旗標</param>
        /// <param name="leftFlag">左方向旗標</param>
        /// <param name="upFlag">上方向旗標</param>
        /// <param name="downFlag">下方向旗標</param>
        /// <returns>方向變化的 Vector2Int</returns>
        private static Vector2Int GetAxisDirectionChange(Vector2 currentValue, 
            ref bool rightFlag, ref bool leftFlag, ref bool upFlag, ref bool downFlag)
        {
            return new Vector2Int(
                ProcessSingleAxis(currentValue.x, ref rightFlag, ref leftFlag),
                ProcessSingleAxis(currentValue.y, ref upFlag, ref downFlag)
            );
        }

        private static float GetFeatureUsage(InputDevice device, InputFeatureUsage<float> usage, bool clamp = true)
        {
            float val;
            device.TryGetFeatureValue(usage, out val);

            return Mathf.Clamp01(val);
        }

        private static bool GetFeatureUsage(InputDevice device, InputFeatureUsage<bool> usage)
        {
            bool val;
            if (device.TryGetFeatureValue(usage, out val))
            {
                return val;
            }

            return val;
        }

        private static Vector2 GetFeatureUsage(InputDevice device, InputFeatureUsage<Vector2> usage)
        {
            Vector2 val;
            if (device.TryGetFeatureValue(usage, out val))
            {
                return val;
            }

            return val;
        }

        private static Vector3 GetFeatureUsage(InputDevice device, InputFeatureUsage<Vector3> usage)
        {
            Vector3 val;
            if (device.TryGetFeatureValue(usage, out val))
            {
                return val;
            }

            return val;
        }
        #endregion

        #region 震動功能

        public static void ShockRightHand(uint channel, float amplitude, float duration = 1f) 
        {
            primaryRightController.SendHapticImpulse(channel, amplitude, duration);
        }

        public static void ShockLeftHand(uint channel, float amplitude, float duration = 1f)
        {
            primaryLeftController.SendHapticImpulse(channel, amplitude, duration);
        }

        #endregion
    }
}
