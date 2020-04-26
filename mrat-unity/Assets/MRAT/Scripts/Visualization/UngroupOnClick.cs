using UnityEngine;
using HoloToolkit.Unity.InputModule;

using MRAT;

public class UngroupOnClick : MonoBehaviour, IInputClickHandler {

    public AgglomerativeClusteringVisualizerView parent;
    public AgglomerativeClusteringVisualizerView.ClusterNode node;

    void Start() {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
    }

    public void OnInputClicked(InputClickedEventData eventData) {
        if (!eventData.used && parent != null && node != null) {
            eventData.Use();
            parent.Split(node);
            var zoom_controller = parent.gameObject.GetComponent<ZoomClusteringController>();
            zoom_controller.enabled = false;
        }        
    }
	
}
