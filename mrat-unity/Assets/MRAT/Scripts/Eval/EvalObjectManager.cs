using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRAT
{
	public class EvalObjectManager : MonoBehaviour
	{
		public int MaxObjects = 1000;

		public int MaxGridXDimension = 10;
		public int MaxGridYDimension = 10;
		public int MaxGridZDimension = 10;

		public float ObjectSpacing = 1f;
		public GameObject ObjectPrefab;

		public int TotalObjectCount;
		public List<GameObject> EvaluationObjects;

		private void Awake()
		{
			TotalObjectCount = MaxGridXDimension * MaxGridYDimension * MaxGridZDimension;
			EvaluationObjects = new List<GameObject>(TotalObjectCount);
		}

		private void Start()
		{
			CreateObjects();
		}

		public void CreateObjects()
		{
			PopulateGrid();

			TotalObjectCount = EvaluationObjects.Count;
		}

		private void PopulateGrid()
		{
			float gridX = MaxGridXDimension;
			float gridY = MaxGridYDimension;
			float gridZ = MaxGridZDimension;

			Vector3 pos;
			var originPos = gameObject.transform.position;

			for (int z = 0; z < gridZ; z++)
			{
				for (int y = 0; y < gridY; y++)
				{
					for (int x = 0; x < gridX; x++)
					{
						pos = new Vector3(x + originPos.x, y + originPos.y, z + originPos.z) * ObjectSpacing;
						EvaluationObjects.Add(Instantiate(ObjectPrefab, pos, Quaternion.identity));

						if (EvaluationObjects.Count >= MaxObjects) return;
					}
				}

			}
		}

		public void ResetObjects()
		{
			foreach (var obj in EvaluationObjects)
			{
				Destroy(obj);
			}

			EvaluationObjects.Clear();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			CreateObjects();
		}
	}
}
