using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace MRAT
{
	[Serializable]
	public class MratInputSource
	{
		public uint SourceId;

		[NonSerialized]
		public IInputSource Source;

		public InteractionSourceInfo SourceKind;
		public Vector3 PreviousSourcePosition;
		public Vector3 CurrentSourcePosition;
		public string PositionSide;
		public bool IsTrackable;

		public long SourceDetectedTimestamp;
		public long SourceLostTimestamp;
		public long SourceTrackedDuration;

		public int InputClickCount;

		public MratInputSource()
		{
		}
		
		public MratInputSource(IInputSource source, uint sourceId)
		{
			if (source == null) return;

			SourceDetectedTimestamp = MratHelpers.GetTimestamp();

			Source = source;
			SourceId = sourceId;
			
			if (!Source.TryGetSourceKind(SourceId, out SourceKind)) return;

			if (!TryGetSourcePosition(out CurrentSourcePosition)) return;

			IsTrackable = true;
			PreviousSourcePosition = CurrentSourcePosition;

			PositionSide = GetSideFromPosition(CurrentSourcePosition);
		}

		public bool TryUpdateSourcePosition()
		{
			Vector3 newPosition;

			if (!TryGetSourcePosition(out newPosition)) return false;

			PreviousSourcePosition = CurrentSourcePosition;
			CurrentSourcePosition = newPosition;

			PositionSide = GetSideFromPosition(CurrentSourcePosition);

			return true;
		}

		public bool TryGetSourcePosition(out Vector3 inputPosition)
		{
			bool result;

			switch (SourceKind)
			{
				case InteractionSourceInfo.Hand:
					result = Source.TryGetGripPosition(SourceId, out inputPosition);
					break;
				case InteractionSourceInfo.Controller:
					result = Source.TryGetPointerPosition(SourceId, out inputPosition);
					break;
				case InteractionSourceInfo.Other:
					if (Source.TryGetGripPosition(SourceId, out inputPosition))
					{
						Debug.Log("Other Input, GripPosition: " + inputPosition);
						result = true;
						break;
					}
					else if (Source.TryGetPointerPosition(SourceId, out inputPosition))
					{
						Debug.Log("Other Input, PointerPosition: " + inputPosition);
						result = true;
						break;
					}
					else
					{
						inputPosition = Vector3.zero;
						result = false;
						break;
					}
				default:
					inputPosition = Vector3.zero;
					result = false;
					break;
			}

			return result;
		}

		public static string GetSideFromPosition(Vector3 inputPosition)
		{
			// Hand position code adapted from: https://forums.hololens.com/discussion/comment/6264/#Comment_6264

			var heading = inputPosition - GazeManager.Instance.GazeOrigin;
			var perp = Vector3.Cross(GazeManager.Instance.GazeTransform.forward, heading);
			var dot = Vector3.Dot(perp, GazeManager.Instance.GazeTransform.up);

			return dot <= 0 ? "left" : "right";
		}

		/// <summary>
		/// Update object with source lost info, including tracking duration.
		/// </summary>
		public void SourceLost(long timestamp = 0)
		{
			if (timestamp <= 0)
			{
				timestamp = MratHelpers.GetTimestamp();
			}

			SourceLostTimestamp = timestamp;

			if (SourceDetectedTimestamp <= 0) return;

			SourceTrackedDuration = SourceLostTimestamp - SourceDetectedTimestamp;
		}
	}
}
