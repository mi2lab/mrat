#if UNITY_EDITOR
using MRAT;
using UnityEditor;
using UnityEngine;

namespace MRAT
{
	[CustomEditor(typeof(EvalObjectManager))]
	public class EditorEvalObjectManager : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			var myScript = (EvalObjectManager)target;

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			GUILayout.Label("Custom Editor Functions");

			if (GUILayout.Button("Reset Objects"))
			{
				myScript.ResetObjects();
			}
		}
	}
}
#endif
