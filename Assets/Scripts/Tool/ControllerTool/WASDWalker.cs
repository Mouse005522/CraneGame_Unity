using UnityEngine;
using Kun.XR;

namespace Kun.Tool
{
    public class WASDWalker : MonoBehaviour
	{
		[SerializeField]
		public Transform Eye;

		protected virtual void Start ()
		{
			if (Eye == null)
			{
				Eye = transform;
			}
		}

		public void SetPose (Vector3 pos, Vector3 euler) 
		{
			cachePos = pos;

			cacheEuler = euler;

			var processX = Mathf.Clamp (cacheEuler.x, -87, 87);
			cacheEuler.x = processX;

			ResetPose ();
		}

		void ResetPose () 
		{
			this.transform.position = cachePos;

			//walker跟eye是父子關係
			this.transform.eulerAngles = Vector3.up * cacheEuler.y;
			this.Eye.transform.localEulerAngles = Vector3.right * cacheEuler.x;
		}

		Vector3 cachePos;
		Vector3 cacheEuler;

		[SerializeField]
		public float fasterRotSpeed;

		[SerializeField]
        public float rotSpeed;

		[SerializeField]
        public float fasterMoveSpeed;

		[SerializeField]
		public float moveSpeed;

		protected virtual void Update ()
		{
			bool isFasterMode = Input.GetKey (KeyCode.CapsLock);

			float yaw = 0.0f;

			float deltaAngle = isFasterMode ? fasterRotSpeed : rotSpeed;

			//水平旋轉
			if (Input.GetKey (KeyCode.Q) || XRInput.LeftThumbstickAxis.x < -0.5f)
			{
				yaw -= Time.deltaTime * deltaAngle;
			}

			if (Input.GetKey (KeyCode.E) || XRInput.LeftThumbstickAxis.x > 0.5f)
			{
				yaw += Time.deltaTime * deltaAngle;
			}

			if (yaw != 0.0f)
			{
				Vector3 worldEye = Eye.position;
				transform.rotation = Quaternion.Euler (0, transform.rotation.eulerAngles.y + yaw, 0.0f);

				if (transform != Eye)
				{
					transform.position = worldEye - transform.rotation * Eye.localPosition;
				}
			}

			// 俯視角
			float roll = 0.0f;
			if (Input.GetKey (KeyCode.R))
			{
				roll -= Time.deltaTime * deltaAngle;
			}
			if (Input.GetKey (KeyCode.F))
			{
				roll += Time.deltaTime * deltaAngle;
			}

			if (roll != 0.0f)
			{
				float current_roll = Mathf.Asin (-Eye.forward.y) / Mathf.PI * 180.0f;

				Eye.localRotation = Quaternion.Euler (Mathf.Clamp (current_roll + roll, -87.0f, 87.0f), 0f, 0f);
			}

			float deltaMove = isFasterMode ? fasterMoveSpeed : moveSpeed;

			Vector3 position = Vector3.zero;
			if (Input.GetKey (KeyCode.KeypadMinus) || Input.GetKey (KeyCode.Minus) || Input.GetKey (KeyCode.LeftControl))
			{
				position.y += Time.deltaTime * deltaMove;
			}
			if (Input.GetKey (KeyCode.KeypadPlus) || Input.GetKey (KeyCode.Plus) || Input.GetKey (KeyCode.LeftAlt))
			{
				position.y -= Time.deltaTime * deltaMove;
			}

			Vector3 right = Eye.right;
			right.y = 0.0f;
			right.Normalize ();
			if (Input.GetKey (KeyCode.A))
			{
				position -= right * Time.deltaTime * deltaMove;
			}
			if (Input.GetKey (KeyCode.D))
			{
				position += right * Time.deltaTime * deltaMove;
			}

			//vr的部分
			Vector2 moveAxis = XRInput.RightThumbstickAxis;
			position += right * Time.deltaTime * deltaMove * moveAxis.x;

			Vector3 forward = Eye.forward;
			forward.y = 0.0f;
			forward.Normalize ();
			if (Input.GetKey (KeyCode.W))
			{
				position += forward * Time.deltaTime * deltaMove;
			}
			if (Input.GetKey (KeyCode.S))
			{
				position -= forward * Time.deltaTime * deltaMove;
			}

			
			//vr的部分
			position += forward * Time.deltaTime * deltaMove * moveAxis.y;

			if (position.sqrMagnitude > 0.0f)
			{
				transform.position += position;
			}
		}
	}
}