using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MRAT {

    public class AgglomerativeClusteringVisualizerView : VisualizerView
    {

        public float initialVisualizationPercentage = 0.6f;

        public GameObject directionalMarker;
        public GameObject groupMarker;
        public GameObject axisMarker;

        public float motionRate = 0.05f;
        public float scaleRate = 0.05f;
        public AnimationCurve motionCurve;
        public AnimationCurve groupScaleCurve;
        public float groupSizeMaxScale = 200;

        //interface for auto zooming features
        public int NumMarkers {
            get {
                return markers.Count;
            }
        }

        public int TotalEvents {
            get {
                return maxEvents;
            }
        }
        private int maxEvents;

        public Vector3 DataRoot {
            get {
                return centroidMarker ? centroidMarker.transform.position : Vector3.zero;
            }
        }
        private GameObject centroidMarker;

        private List<ClusterNode> roots;
        private Dictionary<ClusterNode, GameObject> markers;

        private void Awake()
        {
            roots = new List<ClusterNode>();
            markers = new Dictionary<ClusterNode, GameObject>();
        }

        public class ClusterNode
        {
            public ClusterNode(IVisualizable obj)
            {
                this.obj = obj;
                this.pos = obj.GetVisualizationPosition();
                this.child1 = null;
                this.child2 = null;
                this.numChildren = 1;
            }

            public ClusterNode(ClusterNode child1, ClusterNode child2)
            {
                this.obj = null;
                this.child1 = child1;
                this.child2 = child2;
                this.numChildren = child1.numChildren + child2.numChildren;
                this.pos = (child1.numChildren * child1.pos + child2.numChildren * child2.pos) / this.numChildren;
            }

            public ClusterNode child1, child2;
            public IVisualizable obj;
            public Vector3 pos;
            public int numChildren;
        }

        Color GetColor(IVisualizable e) {
            var mratEvent = e as MratEventSimple;
            Color markerColor;
            if (!ColorUtility.TryParseHtmlString(mratEvent.EventColor, out markerColor))
                markerColor = Color.gray;
            return markerColor;
        }

        //TODO: This function is currently a DFS not modal. 
        private IVisualizable Representative(ClusterNode node) {
            return node.obj == null ? Representative(node.child1) : node.obj;
        }
        
        private Quaternion MeanRotation(ClusterNode node) {
            if (node.obj == null)
            {
                float balance = node.child1.numChildren
                    / (node.child1.numChildren + node.child2.numChildren);

                return Quaternion.Slerp(
                    MeanRotation(node.child1),
                    MeanRotation(node.child2),
                    balance
                );
            }
            else return node.obj.GetVisualizationRotation();
        }

        public bool Join() {

            if (roots.Count <= 1) return false;

            //find closest pair of nodes
            int best_i = 0, best_j = 1;
            float bestDistance = Vector3.Distance(roots[best_i].pos, roots[best_j].pos);
            for (int i = 0; i < roots.Count; ++i)
            {
                for (int j = i + 1; j < roots.Count; ++j)
                {
                    float candDistance = Vector3.Distance(roots[i].pos, roots[j].pos);
                    if (candDistance < bestDistance)
                    {
                        best_i = i;
                        best_j = j;
                        bestDistance = candDistance;
                    }
                }
            }

            //join two trees
            ClusterNode a = roots[best_i];
            ClusterNode b = roots[best_j];
            ClusterNode parent = new ClusterNode(a, b);
            
            roots.Remove(a);
            roots.Remove(b);
            roots.Add(parent);

            UpdateGraphicsWithEffects(a, parent.pos);
            UpdateGraphicsWithEffects(b, parent.pos);
            UpdateGraphicsWithEffects(parent, parent.pos);

            return true;
        }

        //Gets the node whose children are most distant
        ClusterNode GetSplitTarget() {

            if (roots.Count == maxEvents)
                return null;

            //find the node whose children are furthest apart
            int bestIndex = -1;
            float bestDistance = float.NegativeInfinity;
            for (int i = 0; i < roots.Count; ++i)
            {
                if (roots[i].child1 == null) continue; //skip leaves
                float candDistance = Vector3.Distance(roots[i].child1.pos, roots[i].child2.pos);
                if (candDistance > bestDistance)
                {
                    bestIndex = i;
                    bestDistance = candDistance;
                }
            }
            return bestIndex == -1 ? null : roots[bestIndex];
        }

        public void Split(ClusterNode target) {

            roots.Remove(target);
            roots.Add(target.child1);
            roots.Add(target.child2);

            UpdateGraphicsWithEffects(target, target.pos);
            UpdateGraphicsWithEffects(target.child1, target.pos);
            UpdateGraphicsWithEffects(target.child2, target.pos);
        }
		
        //Splits the node whose children are most distant
        //returns whether it successfully split a node
        public bool SplitClosest() {           
            
            //split node
            ClusterNode target = GetSplitTarget();

            if (target == null) return false;

            Split(target);

            return true;
        }

        public override void VisualizeEvents(List<MratEventSimple> eventList)
        {

            Destroy(centroidMarker);

            //reset tracking variables
            maxEvents = eventList.Count;
            Vector3 meanEventPosition = Vector3.zero;          

            //populate new roots list
            roots.Clear();
            int numVisEvents = 0;
            foreach (var e in eventList) {
                var visEvent = e as IVisualizable;
                if (visEvent == null) continue;
                meanEventPosition += visEvent.GetVisualizationPosition();
                numVisEvents += 1;
                roots.Add(new ClusterNode(visEvent));
            }

            meanEventPosition /= numVisEvents;
            centroidMarker = Instantiate(axisMarker, meanEventPosition, Quaternion.identity);

            int desired = Mathf.CeilToInt(initialVisualizationPercentage * eventList.Count);
            while (roots.Count > desired) Join();

            ResetGraphics();
        }

        public override void ClearGraphics()
        {
	        foreach (GameObject obj in markers.Values)
	        {
		        Destroy(obj);
			}

	        markers.Clear();
        }

        private  void ResetGraphics()
        {
            ClearGraphics();

            foreach (ClusterNode node in roots)
                markers[node] = PlaceMarker(node);
        }

        private void UpdateGraphicsWithEffects(ClusterNode node, Vector3 location) {
            if (roots.Contains(node))
            {
                if (!markers.ContainsKey(node)) {
                    markers[node] = PlaceMarker(node);
                    markers[node].transform.position = location;
                    StartCoroutine(AnimateToLocation(markers[node], node.pos));
                }                    
            }
            else
            {
                if (markers.ContainsKey(node))
                {
                    if (node.pos == location) {
                        StartCoroutine(ScaleAndDestroy(markers[node]));
                    }
                    else {
                        StartCoroutine(AnimateAndDestroy(markers[node], location));
                    }                    
                    markers.Remove(node);
                }
            }
        }

        private GameObject PlaceMarker(ClusterNode node)
        {

            var mratEvent = node.obj as MratEventSimple;

            if (node.obj == null)
            {
                var marker = Instantiate(groupMarker, node.pos, Quaternion.identity);
                marker.name = "Marker Group";
                marker.transform.parent = transform;

                //apply groupsize scaling
                float size_t = node.numChildren / groupSizeMaxScale;
                marker.transform.localScale = groupScaleCurve.Evaluate(size_t) * marker.transform.localScale;

                var marker_controller = marker.GetComponent<MratEventMarker>();               
                marker_controller.SetRotation(MeanRotation(node));

                var rep = Representative(node);
                var rep_mratEvent = rep as MratEventSimple;
                marker_controller.SetText($"{node.numChildren}x Events");
                marker_controller.SetColor(Color.gray);

                //add script to allow hololens clicks to split the group
                var ungroup_handler = marker.GetComponentInChildren<UngroupOnClick>();
                ungroup_handler.parent = this;
                ungroup_handler.node = node;

                return marker;
            }
            else {               

                var marker = Instantiate(directionalMarker, node.obj.GetVisualizationPosition(), Quaternion.identity);
                marker.name = $"{mratEvent.GetType().Name} Marker";
                marker.transform.parent = transform;

                var marker_controller = marker.GetComponent<MratEventMarker>();
                marker_controller.MratEvent = mratEvent;
                marker_controller.SetText($"{mratEvent.TimeAtFrameBeginningSeconds:0.0}\n{mratEvent.EventType}");
                marker_controller.SetRotation(node.obj.GetVisualizationRotation());

                marker_controller.SetColor(GetColor(node.obj));

                return marker;
            }            
        }

        IEnumerator AnimateToLocation(GameObject g, Vector3 target) {

            if (g.transform.position != target) {
                float t = 0;
                Vector3 initialPosition = g.transform.position;
                Vector3 velocity = (target - initialPosition);
                while (g && t < 1)
                {
                    g.transform.position = initialPosition + motionCurve.Evaluate(t) * velocity;
                    t += motionRate / velocity.magnitude;
                    yield return null;
                }
                if (g) g.transform.position = target;
                yield return null;
            }
        }

        IEnumerator AnimateAndDestroy(GameObject g, Vector3 target) {
            g.GetComponentInChildren<Text>().enabled = false;
            yield return StartCoroutine(AnimateToLocation(g, target));
            Destroy(g);
            yield return null;
        }

        IEnumerator ScaleDown(GameObject g) {
            float t = 0;
            Vector3 origScale = g.transform.localScale;
            while (g && t < 1) {
                t += scaleRate;
                float factor = Mathf.Clamp(1 - t, 0.00001f, 1);
                g.gameObject.transform.localScale = factor * origScale;
                yield return null;
            }
            yield return null;
        }

        IEnumerator ScaleAndDestroy(GameObject g) {
            g.GetComponentInChildren<Text>().enabled = false;
            yield return StartCoroutine(ScaleDown(g));
            Destroy(g);
            yield return null;
        }

    }


}