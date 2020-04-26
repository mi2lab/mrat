using MRAT;
using UnityEngine;

public class ZoomClusteringController : MonoBehaviour {

    public AnimationCurve responseCurve;
    public float maxDist = 20;
    private AgglomerativeClusteringVisualizerView visualizer;

    private void Awake()
    {
        visualizer = GetComponent<AgglomerativeClusteringVisualizerView>();
    }

    private void Update()
    {
        float dist = Vector3.Distance(Camera.main.transform.position, visualizer.DataRoot);
        int num_desired = (int) (visualizer.TotalEvents * responseCurve.Evaluate(dist / maxDist));
        while (visualizer.NumMarkers > num_desired && visualizer.Join());
        while (visualizer.NumMarkers < num_desired && visualizer.SplitClosest());
    }

}
