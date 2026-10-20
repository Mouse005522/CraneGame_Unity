using System;

namespace Kun.Tool
{
    public class UpAndDownModuleCache
	{
		public int? moveToNextIndex;
		public int? moveToPrevIndex;

		protected event Action<int> moveIndexToNextEvent;
		protected event Action<int> moveIndexToPrevEvent;

		public UpAndDownModuleCache(Action<int> moveIndexToNextEvent, Action<int> moveIndexToPrevEvent)
		{
			this.moveIndexToNextEvent = moveIndexToNextEvent;
			this.moveIndexToPrevEvent = moveIndexToPrevEvent;
		}

		public void Flush ()
		{
			if (moveToNextIndex != null)
			{
				moveIndexToNextEvent?.Invoke (moveToNextIndex.Value);
			}

			if (moveToPrevIndex != null)
			{
				moveIndexToPrevEvent?.Invoke (moveToPrevIndex.Value);
			}
		}
	}

	public class RemoveAndAddCache
	{
		public bool hasAdd;
		public int? hasRemoveIndex;

		protected event Action addEvent;
		protected event Action<int> removeAtEvent;

		public RemoveAndAddCache(Action addEvent,Action<int> removeAtEvent)
		{
			this.addEvent = addEvent;
			this.removeAtEvent = removeAtEvent;
		}

		public void Flush ()
		{
			if (hasAdd) 
			{
				if (addEvent != null)
					addEvent.Invoke ();

				hasAdd = false;
			}

			if (hasRemoveIndex!=null) 
			{
				if (removeAtEvent != null)
					removeAtEvent.Invoke (hasRemoveIndex.Value);

				hasRemoveIndex = null;
			}
		}
	}

	public class RemoveAndCloneCache
	{
		public int? hasRemoveIndex;
		public int? hasCloneIndex;

		protected event Action<int> removeAtEvent;
		protected event Action<int> cloneEvent;

		public RemoveAndCloneCache(Action<int> removeAtEvent, Action<int> cloneEvent)
		{
			this.removeAtEvent = removeAtEvent;
			this.cloneEvent = cloneEvent;
		}

		public void Flush ()
		{
			if (hasRemoveIndex != null)
			{
				if (removeAtEvent != null) 
				{
					removeAtEvent.Invoke (hasRemoveIndex.Value);
				}

				hasRemoveIndex = null;
			}

			if (hasCloneIndex != null) 
			{
				if (cloneEvent != null) 
				{
					cloneEvent.Invoke (hasCloneIndex.Value);
				}

				hasCloneIndex = null;
			}
		}
	}

	public class RemoveAddAndCloneCache
	{
		public bool hasAdd;
		public int? hasRemoveIndex;
		public int? hasCloneIndex;

		protected event Action addEvent;
		protected event Action<int> removeAtEvent;
		protected event Action<int> cloneEvent;

		public RemoveAddAndCloneCache(Action addEvent, Action<int> removeAtEvent, Action<int> cloneEvent)
		{
			this.addEvent = addEvent;
			this.removeAtEvent = removeAtEvent;
			this.cloneEvent = cloneEvent;
		}

		public void Flush ()
		{
			if (hasAdd)
			{
				if (addEvent != null)
					addEvent.Invoke ();

				hasAdd = false;
			}

			if (hasRemoveIndex != null)
			{
				if (removeAtEvent != null)
					removeAtEvent.Invoke (hasRemoveIndex.Value);

				hasRemoveIndex = null;
			}

			if (hasCloneIndex != null)
			{
				if (cloneEvent != null)
					cloneEvent.Invoke (hasCloneIndex.Value);

				hasCloneIndex = null;
			}
		}
	}
}
