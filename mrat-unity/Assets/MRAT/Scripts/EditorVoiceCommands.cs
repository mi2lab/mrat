#if UNITY_EDITOR
using MRAT;
using UnityEditor;
using UnityEngine;

namespace MRAT
{
	[CustomEditor(typeof(MratVoiceCommands))]
	public class EditorVoiceCommands : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			MratVoiceCommands myScript = (MratVoiceCommands)target;

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			GUILayout.Label("Custom Editor Functions");

			if (GUILayout.Button("Activate Visualizer"))
			{
				myScript.ActivateVisualizer();
				myScript.VoiceCommandIssued("Activate Visualizer");
			}

			if (GUILayout.Button("Deactivate Visualizer"))
			{
				myScript.DeactivateVisualizer();
				myScript.VoiceCommandIssued("Deactivate Visualizer");
			}

			if (GUILayout.Button("Voice Command Event Test"))
			{
				myScript.VoiceCommandIssued("Voice command button test");
			}
		}
	}
}
#endif
