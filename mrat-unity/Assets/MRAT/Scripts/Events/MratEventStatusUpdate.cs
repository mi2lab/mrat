using System;
using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedField.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace MRAT
{
    /// <summary>
    /// MratEvent for handling user and gaze information, including user and cursor information. 
    /// </summary>
    [Serializable]
    public class MratEventStatusUpdate : MratEventSimple, IVisualizable
    {
        public Vector3 Position;
        public Quaternion UserRotation;
        public Vector3 CursorPosition;
        public Quaternion CursorRotation;
	    public bool CursorHasTarget;
	    public string CursorTargetName;
	    public int CursorTargetId;
	    public Vector3 CursorTargetPosition;
	    public Quaternion CursorTargetRotation;

        public Vector3 GetVisualizationPosition() {
            return Position;
        }

        public Quaternion GetVisualizationRotation() {
            return UserRotation;
        }

        public float UserDistanceTravelled;

	    public int FpsWarningLevel = 30;
	    public bool FpsWarningFlag;

	    [SerializeField]
	    private float FpsAverage;

	    [SerializeField]
	    private double MemoryUsedAverage;

	    [SerializeField]
	    private List<float> FpsReadings = new List<float>(80);

	    [SerializeField]
	    private List<long> MemoryReadings = new List<long>(80);

	    public MratEventStatusUpdate(string message = "", MratEventTypes eventType = MratEventTypes.StatusUpdate) : base(message, eventType)
	    {
	    }

	    public override void CollectDataFromUnity()
	    {
		    base.CollectDataFromUnity();

		    var cursor = MratHelpers.GetAnimatedCursor();
		    var camera = CameraCache.Main;

		    Position = camera.transform.position;
		    UserRotation = camera.transform.rotation;

		    CursorPosition = cursor.Position;
		    CursorRotation = cursor.Rotation;

		    var cursorTarget = GazeManager.Instance.HitObject;

		    if (cursorTarget)
		    {
			    CursorHasTarget = true;
			    CursorTargetId = cursorTarget.GetInstanceID();
			    CursorTargetName = cursorTarget.name;
			    CursorTargetPosition = cursorTarget.transform.position;
			    CursorTargetRotation = cursorTarget.transform.rotation;
			}

			var statusTracker = MratHelpers.GetMratStatusTracker();

		    if (statusTracker != null)
		    {
			    UserDistanceTravelled = statusTracker.TotalUserDistanceTravelled;
		    }
		    else
		    {
			    Debug.Log("No MratStatusTracker could be found.");
		    }
	    }

		public void AddFpsReadings(List<float> readings)
	    {
		    FpsReadings.AddRange(readings);

		    if (FpsReadings.Count > 0)
		    {
			    FpsAverage = FpsReadings.Average();
			}

			foreach (var reading in FpsReadings)
		    {
			    if (reading > FpsWarningLevel) continue;

			    FpsWarningFlag = true;
			    break;
		    }
	    }

	    public void AddMemoryReadings(List<long> readings)
	    {
		    MemoryReadings.AddRange(readings);

		    if (MemoryReadings.Count > 0)
		    {
			    MemoryUsedAverage = MemoryReadings.Average();
		    }
	    }

	    public MratEventStatusUpdate CreatePerformanceWarningEvent()
	    {
			// HACK: This is a simple hack to create a separate performance warning event which is easy to visualize. Technically works.

		    var warning = new MratEventStatusUpdate("PerformanceWarning", MratEventTypes.PerformanceWarning)
		    {
			    FpsWarningFlag = FpsWarningFlag,
			    FpsWarningLevel = FpsWarningLevel,
				FpsAverage = FpsAverage,
				Position = Position,
				TimeAtFrameBeginningSeconds = TimeAtFrameBeginningSeconds,
				Timestamp = Timestamp,
				TimeSinceSessionStartupSeconds = TimeSinceSessionStartupSeconds,
				DateTimeIsoLocal = DateTimeIsoLocal,
				EventBaseType = EventBaseType,
				FrameCount = FrameCount,
				MemoryUsedAverage = MemoryUsedAverage
		    };

		    return warning;
	    }
    }
}
