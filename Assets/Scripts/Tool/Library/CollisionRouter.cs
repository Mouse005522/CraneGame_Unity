using System;
using UnityEngine;

namespace Kun.Tool
{
    public class CollisionRouter : MonoBehaviour
	{
		public event Action<Collision> onCollisionEnter;
		
        void OnCollisionEnter (Collision collision)
        {
			onCollisionEnter?.Invoke (collision);
        }

		public event Action<Collision> onCollisionStay;

        void OnCollisionStay (Collision collision)
        {
			onCollisionStay?.Invoke (collision);
        }

		public event Action<Collision> onCollisionExit;

        void OnCollisionExit (Collision collision)
        {
			onCollisionExit?.Invoke (collision);
        }

		public event Action<Collider> onTriggerEnter;

        void OnTriggerEnter (Collider other)
        {
			onTriggerEnter?.Invoke (other);
        }

		public event Action<Collider> onTriggerStay;

        void OnTriggerStay (Collider other)
        {
			onTriggerStay?.Invoke (other);
        }

		public event Action<Collider> onTriggerExit;

        void OnTriggerExit (Collider other)
        {
			onTriggerExit?.Invoke (other);
        }
    }
}
