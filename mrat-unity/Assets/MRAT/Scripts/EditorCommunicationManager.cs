#if UNITY_EDITOR
using MRAT;
using UnityEditor;
using UnityEngine;

namespace MRAT
{
	[CustomEditor(typeof(MratCommunicationManager))]
	public class EditorCommunicationManager : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			MratCommunicationManager myScript = (MratCommunicationManager)target;

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			GUILayout.Label("Custom Editor Functions");

			if (GUILayout.Button("Current Status"))
			{
				myScript.CurrentStatus();
			}
		}
	}
}
#endif

