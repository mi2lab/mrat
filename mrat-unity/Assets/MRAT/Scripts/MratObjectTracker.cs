using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

namespace MRAT
{
	public class MratObjectTracker : MonoBehaviour
	{
		[HideInInspector]
		public static MratObjectTracker Instance;

		public float PollForObjectChangesInterval = 1;
        public long ObjectFixationTime = 1000;  // minimum fixation time in milliseconds

		public MratGameObjectUnityEvent TrackableObjectAdded;
		public MratGameObjectUnityEvent TrackedObjectTransformChanged;
		public MratGameObjectUnityEvent TrackedObjectDestroyed;

		private readonly HashSet<GameObject> _trackedObjects = new HashSet<GameObject>();

        private GameObject _objectInFocus = null;
        private long _focusStartTimestamp = -1;


        private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(gameObject);
			}
		}

		private void Start()
		{
			InvokeRepeating(nameof(CheckForTransformChanges), 1f, PollForObjectChangesInterval);

            FocusManager.Instance.FocusEntered += OnFocusEnter;
            FocusManager.Instance.FocusExited += OnFocusExit;
        }

		private void ObjectDestroyedListener(GameObject obj)
		{
			_trackedObjects.Remove(obj);

			if (!obj.GetComponent<MratTrackable>().TrackDestruction)
			{
				var mratEvent = new MratEventObjectUpdate(obj, MratEventTypes.ObjectDestroyed, "Object destroyed");

				mratEvent.CollectDataFromUnity();

				MratCommunicationManager.Instance.LogMratEvent(mratEvent);
			}
		}

		private void TrackObjectInternal(GameObject obj)
		{
            Debug.Log("Tracking " + obj.name);
			obj.transform.hasChanged = false;

			_trackedObjects.Add(obj);

			if (obj.GetComponent<MratTrackable>().TrackStart)
			{
				var mratEvent = new MratEventObjectUpdate(obj, MratEventTypes.ObjectStartedTracking, "Started tracking new object");

				mratEvent.CollectDataFromUnity();

				MratCommunicationManager.Instance.LogMratEvent(mratEvent);
			}

			TrackableObjectAdded.Invoke(obj);
		}

		public void TrackObject(MratTrackable obj)
		{
			obj.OnDestructionEvent.AddListener(ObjectDestroyedListener);
			TrackObjectInternal(obj.gameObject);
		}

		private void CheckForTransformChanges()
		{
			foreach (var obj in _trackedObjects)
			{
				if (!obj.transform.hasChanged) continue;

				obj.transform.hasChanged = false;

				TrackedObjectTransformChanged.Invoke(obj);

                if (obj.GetComponent<MratTrackable>().TrackUpdates) {
                    var mratEvent = new MratEventObjectUpdate(obj, MratEventTypes.ObjectUpdate, "Object transform changed");

                    mratEvent.CollectDataFromUnity();

                    MratCommunicationManager.Instance.LogMratEvent(mratEvent);
                }
			}
		}

        public void OnFocusEnter(GameObject obj)
        {
            if (_trackedObjects.Contains(obj) && _objectInFocus == null && obj.GetComponent<MratTrackable>().TrackFixation) {
;                _objectInFocus = obj;
                _focusStartTimestamp = MratHelpers.GetTimestamp();
            }

            else
            {
                SetDefaultFixationHelperVariables();
            }
        }

        public void OnFocusExit(GameObject obj)
        {
            var fixationTime = MratHelpers.GetTimestamp() - _focusStartTimestamp;

            if (_objectInFocus == obj && fixationTime > ObjectFixationTime && obj.GetComponent<MratTrackable>().TrackFixation)
            {
                var mratEvent = new MratEventObjectUpdate(obj, MratEventTypes.ObjectFixate, "Object fixated upon");
                mratEvent.CollectDataFromUnity();
                MratCommunicationManager.Instance.LogMratEvent(mratEvent);
            }

            SetDefaultFixationHelperVariables();
        }

        private void SetDefaultFixationHelperVariables() {
            _objectInFocus = null;
            _focusStartTimestamp = -1;
        }

    }
}
