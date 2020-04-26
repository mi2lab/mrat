using UnityEngine;
using UnityEngine.UI;

namespace MRAT
{
	/// <summary>
	/// Unity component to be placed at the top level of an EventMarker (object with tag EventMarker).
	/// This allows an EventMarker to be associated with the event it was spawned based on.
	/// </summary>
	public class MratEventMarker: MonoBehaviour
	{
		[HideInInspector]
		public MratEventSimple MratEvent;

        public GameObject mesh;
        private Text label;

        private void Awake()
        {
            label = GetComponentInChildren<Text>();
        }

        public void SetRotation(Quaternion rot)
        {
            mesh.transform.forward = rot*Vector3.up;
        }

        public void SetText(string text)
        {
            label.text = text;
        }

        public void SetColor(Color color) {
            mesh.GetComponent<Renderer>().material.color = color;
        }
	}
}
