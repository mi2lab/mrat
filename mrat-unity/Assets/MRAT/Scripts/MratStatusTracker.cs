using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

namespace MRAT
{
    public class MratStatusTracker : MonoBehaviour
    {
        /// <summary>
        /// Recording interval as a floating point, measured in seconds.
        /// </summary>
        public float IntervalSeconds = 1.0f;
	    public int FpsWarningLevel = 50;
	    public bool LogPerformanceWarnings = true;

		[HideInInspector]
        public float TotalUserDistanceTravelled;

        private Vector3 _lastUserPosition = Vector3.zero;

	    private MratCommunicationManager _commManager;

	    private readonly List<float> _fpsReadings = new List<float>(80);
	    private readonly List<long> _memoryReadings = new List<long>(80);

		private void Start()
	    {
		    _commManager = MratHelpers.GetMratCommunicationManager();

			InvokeRepeating(nameof(ReportStatusUpdate), 1.0f, IntervalSeconds);
        }

        private void Update()
        {
	        var dist = Vector3.Distance(CameraCache.Main.transform.position, _lastUserPosition);
	        TotalUserDistanceTravelled += dist;

			_lastUserPosition = CameraCache.Main.transform.position;

	        _fpsReadings.Add(1.0f / Time.deltaTime);
	        _memoryReadings.Add(System.GC.GetTotalMemory(false));
		}

		public void ReportStatusUpdate()
		{
			var e = new MratEventStatusUpdate();

			e.CollectDataFromUnity();

	        e.AddFpsReadings(_fpsReadings);
	        e.AddMemoryReadings(_memoryReadings);

	        _fpsReadings.Clear();
			_memoryReadings.Clear();

			_commManager.LogMratEvent(e);

			if (LogPerformanceWarnings && e.FpsWarningFlag) _commManager.LogMratEvent(e.CreatePerformanceWarningEvent());
		}
    }
}
