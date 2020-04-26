using UnityEngine;

namespace MRAT
{
	public class EvalObjectMover: MonoBehaviour
	{
		public float MoveRangeX = 1;
		public float MoveSpeed = 1;

		public Vector3 OriginalPosition;
		public Vector3 TargetPositionMax;
		public Vector3 TargetPositionMin;
		public Vector3 CurrentTargetPosition;

		private Transform _transform;

		private void Awake()
		{
			_transform = gameObject.transform;
			OriginalPosition = _transform.position;

			TargetPositionMin = new Vector3(OriginalPosition.x - (MoveRangeX / 2), OriginalPosition.y, OriginalPosition.z);
			TargetPositionMax = new Vector3(OriginalPosition.x + (MoveRangeX / 2), OriginalPosition.y, OriginalPosition.z);

			CurrentTargetPosition = TargetPositionMax;
		}

		private void Update()
		{
			float step = MoveSpeed * Time.deltaTime;

			if (_transform.position == CurrentTargetPosition)
			{
				CurrentTargetPosition = CurrentTargetPosition == TargetPositionMax ? TargetPositionMin : TargetPositionMax;
			}

			_transform.position = Vector3.MoveTowards(_transform.position, CurrentTargetPosition, step);
		}
	}
}
