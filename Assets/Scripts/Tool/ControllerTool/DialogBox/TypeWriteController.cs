using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    /// 打字機風格的輸入系統
    /// </summary>
    [Serializable]
	public class TypeWriteController 
	{
		/// <summary>
		/// callback回傳最新的行數跟最新的文字內容
		/// </summary>
		/// <param name="onContentModify">On content modify.</param>
		public TypeWriteController(Action<int,string> onContentModify)
		{
			Reset ();
			this.onContentModify = onContentModify;
		}
		
		const float SettingWriteSpeed = 1;
		
		[SerializeField][ReadOnly]
		string currentOutputLine;

		[SerializeField][ReadOnly]
		int currentWordIndex;

		[SerializeField][ReadOnly]
		int currentLineIndex;

		[SerializeField][ReadOnly]
		float currentProgress;

		[SerializeField][ReadOnly]
		bool firstWord;

		[SerializeField][ReadOnly]
		List<string> waitOutputLines = new List<string>();

		[SerializeField][ReadOnly]
		List<string> keepLines = new List<string>();

		[SerializeField][ReadOnly]
		bool inOutput;

		string keepText;

		event Action<int,string> onContentModify;

		float GetWriteSpeed
		{
			get
			{
				int remaingLinesCount = waitOutputLines.Count + 1;

				//還剩幾行要輸出 行數越多寫得越快 
				float remainingSpeed = 1 + (remaingLinesCount - 1) * 0.2f;
				float statusSpeed = 1;

				//第一個字出來快一點
				if (firstWord) 
				{
					statusSpeed = 3;
				}
				return SettingWriteSpeed * remainingSpeed * statusSpeed;
			}
		}

		public void Input(List<string> messages)
		{
			waitOutputLines.AddRange (messages);
			CheckWakeProcess ();
		}

		/// <summary>
		/// 判斷是否是從 休眠狀態被喚醒 並做處理
		/// </summary>
		void CheckWakeProcess()
		{
			if (!inOutput) 
			{
				inOutput = true;

				PrepareNewLine ();

				OnContentModify ();
			}
		}

		public void Refresh (float deltaTime) 
		{
			if (inOutput) 
			{
				currentProgress += deltaTime * GetWriteSpeed;

				if (currentProgress >= 1) 
				{
					//最後一個字了
					if (currentWordIndex == currentOutputLine.Length - 1) 
					{
						if (waitOutputLines.Count > 0) 
						{
							PrepareNewLine ();
						}
						else
						{
							inOutput = false;
						}
					}
					else
					{
						string newWord = PopNewWord (firstWord);
						keepText += newWord;

						if (firstWord) 
						{
							firstWord = false;
						}
					}

					OnContentModify ();
				}
			}
		}

		public void Reset ()
		{
			inOutput = false;
			currentWordIndex = 0;
			currentLineIndex = 0;
			waitOutputLines = new List<string>();
			keepText = "";
		}

		/// <summary>
		/// 是否目前輸入處在
		/// </summary>
		/// <value><c>true</c> if from origin point; otherwise, <c>false</c>.</value>
		bool FromOriginPoint
		{
			get
			{
				return currentLineIndex == 0 && currentWordIndex == 0;
			}
		}


		void PrepareNewLine()
		{
			StringBuilder stringBuilder = new StringBuilder (keepText);

			//開場時是從起點開始 其他情況是從上一行的結尾開始
			if (!FromOriginPoint)
			{
				stringBuilder.Append ("\r\n");
				currentLineIndex++;
			}

			currentOutputLine = PopNewLine ();
			keepLines.Add (currentOutputLine);
			currentWordIndex = 0;

			currentProgress = 0;
			currentWordIndex = 0;

			keepText = stringBuilder.ToString ();

			firstWord = true;
		}

		string PopNewLine()
		{
			string result = waitOutputLines [0];
			waitOutputLines.RemoveAt (0);
			return result;
		}

		//首字從0開始 之後每個都要往後移動一個index
		string PopNewWord(bool firstWord)
		{
			if (!firstWord) 
			{
				currentWordIndex++;
			}

			char[] allWords = currentOutputLine.ToCharArray ();

			return  allWords [currentWordIndex].ToString ();
		}

		void OnContentModify()
		{
			onContentModify.Invoke (currentLineIndex, keepText);
		}
	}
}
