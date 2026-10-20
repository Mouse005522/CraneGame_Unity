using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.HardwareInput
{
    public class Keyboard_MouseInputReceiver : InputReceiver
	{	
		public Keyboard_MouseInputReceiver ()
		{
			keyMappingTables = new List<KeyMappingTable> ();

			KeyMappingTable<KeyCode> keyCodeTable = new KeyMappingTable<KeyCode> (Input.GetKeyDown, Input.GetKey, Input.GetKeyUp);

			keyCodeTable.AddKeyPair (InputKey.Main_Use, KeyCode.Space);
			keyCodeTable.AddKeyPair (InputKey.Main_Use, KeyCode.Return);

			keyCodeTable.AddKeyPair (InputKey.Esc, KeyCode.Escape);
			keyCodeTable.AddKeyPair (InputKey.Interact, KeyCode.E);
			keyCodeTable.AddKeyPair (InputKey.Sub_Interact, KeyCode.Q);
			keyCodeTable.AddKeyPair (InputKey.Jump, KeyCode.Space);
			keyCodeTable.AddKeyPair (InputKey.Shift, KeyCode.LeftShift);
			keyCodeTable.AddKeyPair (InputKey.OpenTool, KeyCode.Tab);
			keyCodeTable.AddKeyPair (InputKey.ReturnLobby, KeyCode.Backspace);

			keyCodeTable.AddKeyPair (InputKey.Right, KeyCode.D);
			keyCodeTable.AddKeyPair (InputKey.Right, KeyCode.RightArrow);

			keyCodeTable.AddKeyPair (InputKey.Left, KeyCode.A);
			keyCodeTable.AddKeyPair (InputKey.Left, KeyCode.LeftArrow);

			keyCodeTable.AddKeyPair (InputKey.Up, KeyCode.W);
			keyCodeTable.AddKeyPair (InputKey.Left, KeyCode.UpArrow);

			keyCodeTable.AddKeyPair (InputKey.Down, KeyCode.S);
			keyCodeTable.AddKeyPair (InputKey.Left, KeyCode.DownArrow);

			keyMappingTables.Add (keyCodeTable);


			KeyMappingTable<int> mouseBtnTable = new KeyMappingTable<int> (Input.GetMouseButtonDown, Input.GetMouseButton, Input.GetMouseButtonUp);

			//左鍵
			mouseBtnTable.AddKeyPair (InputKey.Main_Use, 0);
			//左鍵
			mouseBtnTable.AddKeyPair (InputKey.Sub_Use, 1);

			keyMappingTables.Add (mouseBtnTable);
		}

		List<KeyMappingTable> keyMappingTables = new List<KeyMappingTable> ();

		public override bool GetKey (InputKey key)
		{
			return keyMappingTables.Exists (pair => pair.CheckPress (key));
		}

		public override bool GetKeyDown (InputKey key)
		{
			return keyMappingTables.Exists (pair => pair.CheckDown (key));
		}

		public override bool GetKeyUp (InputKey key)
		{
			return keyMappingTables.Exists (pair => pair.CheckUp (key));
		}

        public override Vector2 GetControllerMove ()
		{
			float x = Input.GetAxis ("Mouse X");	
			float y = Input.GetAxis ("Mouse Y");

			return new Vector2 (x, y);
		}

		public override Vector3 GetControllerPos ()
		{
			return Input.mousePosition;
		}

		/// <summary>
		/// 通過統一的底層去封裝觸發行為
		/// </summary>
		/// <typeparam name="T"></typeparam>
		class KeyMappingTable<T> : KeyMappingTable
		{
			public KeyMappingTable (Func<T, bool> getDown, Func<T, bool> getPress, Func<T, bool> getUp) 
			{
				this.getDown = getDown;
				this.getPress = getPress;
				this.getUp = getUp;
			}

			Func<T, bool> getDown;
			Func<T, bool> getPress;
			Func<T, bool> getUp;
			Func<T, bool> getClick;

			public void AddKeyPair (InputKey inputKey, T mappingKey) 
			{
				inputTable.Add ((inputKey, mappingKey));
			}

			List<(InputKey inputKey, T mappingKey)> inputTable = new List<(InputKey inputKey, T mappingKey)> ();

			public override bool CheckDown (InputKey inputKey) 
			{
				var mappingKeys = GetMappingKeys (inputKey);

				return mappingKeys.Exists (key => getDown.Invoke (key));
			}

			public override bool CheckPress (InputKey inputKey)
			{
				var mappingKeys = GetMappingKeys (inputKey);

				return mappingKeys.Exists (key => getPress.Invoke (key));
			}

			public override bool CheckUp (InputKey inputKey)
			{
				var mappingKeys = GetMappingKeys (inputKey);

				return mappingKeys.Exists (key => getUp.Invoke (key));
			}

            /// <summary>
            ///有可能兩個按鍵可以當一個鍵位用
            ///譬如ws跟上下都可以代表上下
            /// </summary>
            /// <param name="inputKey"></param>
            /// <returns></returns>
            List<T> GetMappingKeys (InputKey inputKey) 
			{
				var findPairs = inputTable.FindAll (pair => pair.inputKey == inputKey);

				return findPairs.ConvertAll (pair => pair.mappingKey);
			}
		}

		abstract class KeyMappingTable
		{
			public abstract bool CheckDown (InputKey inputKey);

			public abstract bool CheckPress (InputKey inputKey);

			public abstract bool CheckUp (InputKey inputKey);
		}
	}
}