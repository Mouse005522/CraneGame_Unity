using UnityEngine;

namespace Kun.HardwareInput
{
    /// <summary>
    /// 給不需要互動的物件跑Task用的
    /// </summary>
    public class NotInteractiveInputReceiver : InputReceiver
	{
		public override Vector2 GetControllerMove ()
		{
			return Vector2.zero;
		}

		public override Vector3 GetControllerPos ()
		{
			return Vector3.zero;
		}

		public override bool GetKey (InputKey key)
		{
			return false;
		}

		public override bool GetKeyDown (InputKey key)
		{
			return false;
		}

		public override bool GetKeyUp (InputKey key)
		{
			return false;
		}
    }
	
	/// <summary>
	/// 方便後續抽換輸入模組
	/// </summary>
	public abstract class InputReceiver
	{
		public abstract Vector2 GetControllerMove ();

		public abstract Vector3 GetControllerPos ();

		public abstract bool GetKey (InputKey key);
		public abstract bool GetKeyDown (InputKey key);
		public abstract bool GetKeyUp (InputKey key);
	}

	public enum InputKey
	{
		/// <summary>
		/// 關閉UI
		/// </summary>
		Esc,
		/// <summary>
		/// 互動
		/// </summary>
		Interact,
		/// <summary>
		/// 副 互動
		/// </summary>
		Sub_Interact,
		/// <summary>
		/// 使用物品
		/// </summary>
		Main_Use,
		/// <summary>
		/// 副 使用物品
		/// </summary>
		Sub_Use,
		/// <summary>
		/// 跳躍或是從座位離開
		/// </summary>
		Jump,
		/// <summary>
		/// 加速
		/// </summary>
		Shift,
		/// <summary>
		/// 開啟道具欄
		/// </summary>
		OpenTool,
		/// <summary>
		/// 回到大廳
		/// </summary>
		ReturnLobby,
		/// <summary>
		/// 右移
		/// </summary>
		Right,
		/// <summary>
		/// 左移
		/// </summary>
		Left,
		/// <summary>
		/// 上移
		/// </summary>
		Up,
		/// <summary>
		/// 下移
		/// </summary>
		Down
	}
}
