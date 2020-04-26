using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityToolbag;

namespace MRAT {

    public class SimpleClusteringVisualizerView : VisualizerView
    {
        public GameObject directionalMarker;
        public GameObject groupMarker;
        public float groupingSize = 0.2f;

        private List<GameObject> markers = new List<GameObject>();

		private void Start()
        {
        }

        private List<List<MratEventSimple>> ClusterEvents(List<MratEventSimple> events, float bucketSize)
        {
            Dictionary<Tuple<int, int, int>, List<MratEventSimple>> buckets = new Dictionary<Tuple<int, int, int>, List<MratEventSimple>>();

            foreach (MratEventSimple e in events)
            {
                var visEvent = e as IVisualizable;
                if (visEvent == null) continue;

                Vector3 pos = visEvent.GetVisualizationPosition();
                var gridPos = new Tuple<int, int, int>(
                    (int)(pos.x / bucketSize),
                    (int)(pos.y / bucketSize),
                    (int)(pos.z / bucketSize)
                );

                if (buckets.ContainsKey(gridPos)) buckets[gridPos].Add(e);
                else buckets[gridPos] = new List<MratEventSimple>() { e };
            }

            return buckets.Values.ToList(); 
        }

        private Vector3 MeanPosition(List<MratEventSimple> events)
        {
            Vector3 meanPos = Vector3.zero;
            int usefulEventCount = 0;
            foreach (var simpleEvent in events)
            {
                var visEvent = simpleEvent as IVisualizable;
                if (visEvent == null) continue;
                else
                {
                    meanPos += visEvent.GetVisualizationPosition();
                    usefulEventCount += 1;
                }
            }
            return meanPos / usefulEventCount;
        }

        //return the most common color of a list of events, arbitrarily breaking ties
        private Color ModalColor(List<MratEventSimple> events)
        {
            Dictionary<Color, int> counts = new Dictionary<Color, int>();
            foreach (var simpleEvent in events)
            {
                Color markerColor;
                if (!ColorUtility.TryParseHtmlString(simpleEvent.EventColor, out markerColor))
                    markerColor = Color.gray;
                if (counts.ContainsKey(markerColor)) ++counts[markerColor];
                else counts[markerColor] = 1;
            }

            Color mode = counts.Keys.First();
            foreach (KeyValuePair<Color, int> p in counts)
            {
                if (p.Value > counts[mode]) mode = p.Key;
            }
            return mode;
        }

        public override void ClearGraphics() {
            foreach (GameObject obj in markers)
                Destroy(obj);
        }

        public override void VisualizeEvents(List<MratEventSimple> events)
        {
	        ClearGraphics();
       
            var clustered = ClusterEvents(events, groupingSize);

            foreach (var cluster in clustered)
            {

                if (cluster.Count == 1)
                {
                    var simpleEvent = cluster[0];
                    var mratEvent = simpleEvent as MratEventSimple;
                    var visEvent = simpleEvent as IVisualizable;
                    if (mratEvent == null || visEvent == null) continue;

                    PlaceEventMarker(mratEvent, directionalMarker);
                }
                else
                {

                    Vector3 meanPos = MeanPosition(cluster);
                    var marker = Instantiate(groupMarker, meanPos, Quaternion.identity);

                    var marker_controller = marker.GetComponent<MratEventMarker>();
                    marker_controller.SetText($"Group of {cluster.Count} events");
                    marker_controller.SetColor(ModalColor(cluster));
                }
            }
        }

        private void PlaceEventMarker(MratEventSimple mratEvent, GameObject prefab)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var visEvent = mratEvent as IVisualizable;

                var marker = Instantiate(prefab, visEvent.GetVisualizationPosition(), Quaternion.identity);
                marker.name = $"{mratEvent.GetType().Name} Marker";
                marker.transform.parent = transform;   
				markers.Add(marker);

                var marker_controller = marker.GetComponent<MratEventMarker>();
                marker_controller.MratEvent = mratEvent;
                marker_controller.SetText($"{mratEvent.TimeAtFrameBeginningSeconds:0.0}\n{mratEvent.EventType}");
                marker_controller.SetRotation(visEvent.GetVisualizationRotation());

                Color markerColor;
                if (!ColorUtility.TryParseHtmlString(mratEvent.EventColor, out markerColor))
                    markerColor = Color.gray;
                marker_controller.SetColor(markerColor);               
            });
        }
    }
}