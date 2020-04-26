using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MRAT
{
	public class EvalTextControl: MonoBehaviour
	{
		private EvalObjectManager _evalManager;
		private TextMesh _textMesh;
		private readonly List<float> _fpsReadings = new List<float>(3000);

		private int _evalCount = 0;
		private int _cycleCount = 0;
		private int _lastObjectCount = 0;

		private void Awake()
		{
			_evalManager = FindObjectOfType<EvalObjectManager>();

			_textMesh = GetComponent<TextMesh>();
		}

		private void Start()
		{
			InvokeRepeating(nameof(CycleEval), 5, 1);
		}

		private void CycleEval()
		{
			if (_lastObjectCount != _evalManager.TotalObjectCount)
			{
				_lastObjectCount = _evalManager.TotalObjectCount;

				_textMesh.text = $"Objects:  {_lastObjectCount}";

				_evalCount = 0;
				_cycleCount = 0;

				_fpsReadings.Clear();

				return;
			}

			if (_cycleCount >= 15)
			{
				UpdateText();

				_cycleCount = 0;
			}
			else
			{
				_cycleCount++;
			}
		}

		private void UpdateText()
		{
			_lastObjectCount = _evalManager.TotalObjectCount;

			_textMesh.text = $"Objects:  {_lastObjectCount}\n" +
			                 $"FPS avg: {_fpsReadings.Average()}\n" + 
			                 $"       min: {_fpsReadings.Min()}\n" + 
			                 $"       max: {_fpsReadings.Max()}\n" + 
			                 $"     eval#: {_evalCount}\n";

			_evalCount++;

			_fpsReadings.Clear();
		}

		private void Update()
		{
			_fpsReadings.Add(1.0f / Time.smoothDeltaTime);
		}

	}
}
