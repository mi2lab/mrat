using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MRAT
{
	public class MratEventVisualizer : MonoBehaviour
	{
        private VisualizerView view;

		public int EventMarkerLayer = 8;
		public bool VisualizeFromInteractiveEndpoint;
		public bool NoLoggingWhenVisualizing;

		/// <summary>
		/// SessionId to use for visualization - only used if VisualizeFromInteractiveEndpoint is not chosen.
		/// </summary>
		public string SessionIdToVisualize = "";

		public float InteractiveRefreshInterval = 1.0f;

		private MratCommunicationManager _mratCommunicationManager;

		private bool _originalLoggingMode;

		private void Start()
		{
            view = GetComponent<VisualizerView>();
			_mratCommunicationManager = MratHelpers.GetMratCommunicationManager();
			_originalLoggingMode = _mratCommunicationManager.LocalLoggingOnly;
		}

		public void StartVisulization()
		{
			if (NoLoggingWhenVisualizing && !_mratCommunicationManager.LocalLoggingOnly)
			{
				_originalLoggingMode = _mratCommunicationManager.LocalLoggingOnly;

				_mratCommunicationManager.LocalLoggingOnly = true;
			}

			if (VisualizeFromInteractiveEndpoint)
			{
				Invoke(nameof(VisualizeEvents), 1f);
				InvokeRepeating(nameof(VisualizationUpdater), 1f + InteractiveRefreshInterval, InteractiveRefreshInterval);
			}
			else
			{
				Invoke(nameof(VisualizeEvents), 1f);
			}
		}

		private void VisualizationUpdater()
		{
			if (VisualizeFromInteractiveEndpoint)
			{
				RefreshInteractiveDataIfAvailable();
			}
		}

		private async void RefreshInteractiveDataIfAvailable()
		{
			var result = await _mratCommunicationManager.GetInteractiveDataStatus();
            if (result) VisualizeEvents();
		}

		public async Task<List<MratEventSimple>> GetEventsToVisualize()
		{
			List<MratEventSimple> eventList;

			if (VisualizeFromInteractiveEndpoint)
			{
				eventList = await _mratCommunicationManager.GetEventsFromInteractiveEndPoint();
			}
			else
			{
				eventList = await _mratCommunicationManager.GetEventsBySessionId(SessionIdToVisualize);
			}

			return eventList;
		}

        public async void VisualizeEvents()
        {
            var eventList = await GetEventsToVisualize();
            view.VisualizeEvents(eventList);
        }

        public void StopVisualizing()
        {
	        CancelInvoke();

			view.ClearGraphics();

			if (NoLoggingWhenVisualizing)
			{
				_mratCommunicationManager.LocalLoggingOnly = _originalLoggingMode;
			}
        }
	}
}
