using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kun.Tool
{
    public class DialogBox
	{
		[SerializeField][ReadOnly]
		Text text;

		RectTransformAdapter rectTransformAdapter;

		/// <summary>
		/// 打字機風格的輸入系統
		/// </summary>
		TypeWriteController typeWriteController;


		const float ChineseWordValue = 4f;

		const float lineHeight = 17.25f;

		float leastAreaTextHeight;

		int currentLineCount;

		//開場的時候抓取text 高 比初始值低就用初始直 超過就用新值
		float GetTextAreaHeight
		{
			get
			{
				float calculationHight = currentLineCount * lineHeight;

				if (calculationHight < leastAreaTextHeight) 
				{
					calculationHight = leastAreaTextHeight;
				}

				return calculationHight;
			}
		}

		/// <summary>
		/// 該字體 英文跟數字等寬 所以判斷中文就好 
		/// </summary>
		const float EnglisgAndNumberWordValue = 2.273f;

		CanvasGroup canvasGroup;

		// Use this for initialization
		public void Init (Text text, CanvasGroup canvasGroup)
		{
			this.canvasGroup = canvasGroup;
			
			InitPars (text);
			InitController ();

			Close (true);
		}

		public void Update (float deltaTime)
		{
			typeWriteController.Refresh (deltaTime);
		}

		void InitController ()
		{
			typeWriteController = new TypeWriteController (OnTextContentModify);
		}

		void InitPars (Text text)
		{
			this.text = text;
			RectTransform rectTransform = text.GetComponent<RectTransform> ();
			rectTransformAdapter = new RectTransformAdapter (rectTransform);
			currentLineCount = 0;
			text.text = "";
			leastAreaTextHeight = rectTransformAdapter.Height;
		}

		// <summary>
		// 判斷會不會超過text的畫面自動切成多行
		// </summary>
		// <returns>The to multi linse.</returns>
		List<string> ProcessToMultiLinse(string msg)
		{
			List<string> lines = new List<string> ();

			//超過100就該換行了
			float currentLineValue = 0;

			StringBuilder stringBuilder = new StringBuilder ();

			Array.ForEach (msg.ToCharArray(),(c)=>
				{
					float wordValue = GetWordValue(c);

					currentLineValue+=wordValue;

					if(currentLineValue > 100)
					{
						lines.Add(stringBuilder.ToString());
						//把上一行完結 這個字元作為下一行的開頭
						stringBuilder = new StringBuilder(c);
						currentLineValue = wordValue;
					}
					else
					{
						stringBuilder.Append(c);
					}
				});

			//把剩餘的字數 作為最後一行
			if (stringBuilder.Length > 0) 
			{
				lines.Add (stringBuilder.ToString ());
			}

			return lines;
		}

		float GetWordValue(Char c)
		{
			bool isChinese = CheckIsChinese (c);

			if (isChinese) 
			{
				return ChineseWordValue;
			}
			else
			{
				return EnglisgAndNumberWordValue;
			}
		}

		bool CheckIsChinese(char c)
		{
			if (c >= 0X4e00 && c < 0X9fbb) 
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Reset ()
		{
			typeWriteController.Reset ();
		}

		/// <summary>
		/// 初始化或是重設的時候 跳過觸發callback
		/// </summary>
		/// <param name="ignoreCallback">If set to <c>true</c> ignore callback.</param>
		public void Close (bool ignoreCallback = false)
		{
			Reset ();
			canvasGroup.alpha = 0f;
		}

		/// <summary>
		/// 初始化或是重設的時候 跳過觸發callback
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="ignoreCallback">If set to <c>true</c> ignore callback.</param>
		public void Open (string message, bool ignoreCallback = false)
		{
			Input (message);
			canvasGroup.alpha = 1f;
		}

		void Input (string message)
		{
			List<string> lines = ProcessToMultiLinse (message);

			typeWriteController.Input (lines);
		}

		void OnTextContentModify (int lineCount, string newText)
		{
			if (currentLineCount != lineCount) 
			{
				currentLineCount = lineCount;
				rectTransformAdapter.Height = GetTextAreaHeight;
			}
			text.text = newText;
   		}
	}	
}