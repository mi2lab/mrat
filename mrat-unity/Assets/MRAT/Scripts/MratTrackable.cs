using UnityEngine;

namespace MRAT
{
	/// <summary>
	/// Component to allow object to be discovered and tracked by the MratObjectTracker.
	/// </summary>
	public class MratTrackable : MonoBehaviour
	{   
        // whether or not to track certain milestones in object's lifecycle
        public bool TrackStart = true;
        public bool TrackUpdates;
        public bool TrackFixation;
        public bool TrackDestruction = true;

        public MratGameObjectUnityEvent OnDestructionEvent;

		private void Start()
		{
			if (OnDestructionEvent == null)
			{
				OnDestructionEvent = new MratGameObjectUnityEvent();
			}

			MratObjectTracker.Instance.TrackObject(this);
		}

		private void OnDestroy()
		{
			OnDestructionEvent.Invoke(gameObject);
		}
	}
}
