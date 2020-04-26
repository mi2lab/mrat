using System.Collections.Generic;
using UnityEngine;

namespace MRAT
{

    public class HeatmapVisualizerView : VisualizerView
    {
        public bool do2DHeatmap = false;
        public float radius = 0.2f;
        public float opacity = 0.2f;
        public GameObject prefab;

        private List<GameObject> markers;

        private void Awake()
        {
            markers = new List<GameObject>();
        }

        public override void VisualizeEvents(List<MratEventSimple> events) {

	        ClearGraphics();

            foreach(var mratEvent in events) {
                markers.Add(PlaceMarker(mratEvent));
            }

        }

        public override void ClearGraphics() {
            foreach (GameObject g in markers)
                if(g != null) Destroy(g);
        }

        private GameObject PlaceMarker(MratEventSimple mratEvent) {

            var visEvent = mratEvent as IVisualizable;
            if (visEvent == null) return null;

            Vector3 pos = visEvent.GetVisualizationPosition();
            if (do2DHeatmap) pos.y = 0;

            var marker = Instantiate(prefab, pos, Quaternion.identity);
            marker.transform.localScale *= radius;
            marker.name = $"{mratEvent.GetType().Name} Marker";
            marker.transform.parent = transform;

            var marker_controller = marker.GetComponent<MratEventMarker>();
            marker_controller.MratEvent = mratEvent;

            Color markerColor;
            if (!ColorUtility.TryParseHtmlString(mratEvent.EventColor, out markerColor))
                markerColor = Color.gray;

            markerColor.a = opacity;
            marker_controller.SetColor(markerColor);

            return marker;
        }
    }

}
