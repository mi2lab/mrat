using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityToolbag;

namespace MRAT {

    public class BasicVisualizerView : VisualizerView
    {
        public GameObject MarkerGeneric;
        public GameObject MarkerImage;
        public int EventMarkerLayer = 8;

        public bool ShowUpdateEvents = true;
        public bool ShowInputEvents = true;
        public bool ShowPhotoEvents = true;
        public string PhotoTextureEndpoint = "http://67.194.29.10:8081/mrat/getScreenshot?fileName=";
        
        private List<MratEventSimple> _eventsVisualized = new List<MratEventSimple>();
        private readonly List<GameObject> _markersVisualized = new List<GameObject>();
        
        public override void ClearGraphics()
        {
            foreach (var obj in _markersVisualized)
            {
                Destroy(obj);
            }

            _markersVisualized.Clear();
            _eventsVisualized.Clear();
        }

        public override void VisualizeEvents(List<MratEventSimple> events)
        {
	        ClearGraphics();

            foreach (var simpleEvent in events)
            {

                Color markerColor;

                if (!ColorUtility.TryParseHtmlString(simpleEvent.EventColor, out markerColor))
                {
                    markerColor = Color.gray;
                }

                if (simpleEvent.GetType().Name == typeof(MratEventInput).Name)
                {
                    if (!ShowInputEvents) continue;

                    var mratEvent = (MratEventInput)simpleEvent;
                    var eventLabel = $"{mratEvent.TimeAtFrameBeginningSeconds:0.0}\n{mratEvent.EventType}";

                    PlaceEventMarker(simpleEvent, mratEvent.Position, mratEvent.UserRotation, MarkerGeneric, markerColor,
                        mratEvent.EventType, eventLabel, 0);
                }
                else if (simpleEvent.GetType().Name == typeof(MratEventObjectUpdate).Name)
                {
                    if (!ShowInputEvents) continue;

                    var mratEvent = (MratEventObjectUpdate)simpleEvent;
                    var eventLabel = $"{mratEvent.TimeAtFrameBeginningSeconds:0.0}\n{mratEvent.EventType}";

                    PlaceEventMarker(simpleEvent, mratEvent.Position, mratEvent.ObjectRotation, MarkerGeneric, markerColor,
                        mratEvent.EventType, eventLabel, 0);
                }
                //				else if (simpleEvent.GetType().Name == typeof(MratEventTaskStatusUpdate).Name)
                //				{
                //					if (!ShowInputEvents) continue;
                //
                //					var mratEvent = (MratEventTaskStatusUpdate)simpleEvent;
                //					var eventLabel = $"{mratEvent.TimeAtFrameBeginningSeconds:0.0}\n{mratEvent.EventType}";
                //
                //					PlaceEventMarker(simpleEvent, mratEvent.MarkerPosition, mratEvent.MarkerRotation, MarkerGeneric, markerColor,
                //						mratEvent.EventType, eventLabel, 0);
                //				}
                else if (simpleEvent.GetType().Name == typeof(MratEventStatusUpdate).Name)
                {
                    if (!ShowUpdateEvents) continue;

                    var mratEvent = (MratEventStatusUpdate)simpleEvent;
                    var eventLabel = $"{mratEvent.TimeAtFrameBeginningSeconds:0.0}\n{mratEvent.EventType}";

                    PlaceEventMarker(simpleEvent, mratEvent.Position, mratEvent.UserRotation, MarkerGeneric, markerColor,
                        mratEvent.EventType, eventLabel, 1);
                }
                else if (simpleEvent.GetType().Name == typeof(MratEventPhotoCaptured).Name)
                {
                    if (!ShowPhotoEvents) continue;

                    var mratEvent = (MratEventPhotoCaptured)simpleEvent;
                    var eventLabel = $"{mratEvent.TimeAtFrameBeginningSeconds:0.0}\n{mratEvent.EventType}";

                    PlaceEventMarker(simpleEvent, mratEvent.UserPosition, mratEvent.UserRotation, MarkerImage, markerColor,
                        mratEvent.EventType, eventLabel, 0, mratEvent.ImageFileName);
                }
            }
        }

        private void PlaceEventMarker(MratEventSimple mratEvent, Vector3 markerPosition, Quaternion markerRotation, GameObject objectForMarker, Color markerColor,
            string markerName, string markerLabel, float minimumDistanceBetweenEvents, string markerTextureFileName = null)
        {
            _eventsVisualized.Add(mratEvent);

            Dispatcher.InvokeAsync(() =>
            {
                if (minimumDistanceBetweenEvents > 0)
                {
                    var hitColliders = Physics.OverlapSphere(markerPosition, minimumDistanceBetweenEvents, EventMarkerLayer);

                    var tooClose = hitColliders.Any(col => col.CompareTag("EventMarker"));

                    if (tooClose) return;
                }

                var marker = Instantiate(objectForMarker, markerPosition, markerRotation);
                marker.name = markerName;

                _markersVisualized.Add(marker);

                var marker_controller = marker.GetComponent<MratEventMarker>();

                marker_controller.SetText(markerLabel);
                marker_controller.MratEvent = mratEvent;

                marker.layer = EventMarkerLayer;

                var rends = marker.GetComponentsInChildren<Renderer>();

                foreach (var rend in rends)
                {
                    if (rend.gameObject.CompareTag("HasColorableRenderer"))
                    {
                        rend.material.color = markerColor;
                    }
                }

                if (string.IsNullOrEmpty(markerTextureFileName)) return;

                var loader = marker.GetComponentInChildren<MratWebTextureLoader>();

                if (loader != null)
                {
                    loader.LoadTexture(PhotoTextureEndpoint + WebUtility.UrlEncode(markerTextureFileName));
                }
            });
        }
    }


}