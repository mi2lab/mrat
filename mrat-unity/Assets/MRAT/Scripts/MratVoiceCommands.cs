/*===============================================================================
Copyright (c) 2016 PTC Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
#if ENABLE_HOLOLENS_MODULE_API || UNITY_5_5_OR_NEWER
#if UNITY_WSA_10_0
#define HOLOLENS_API_AVAILABLE
#endif
#endif

using UnityEngine;

#if HOLOLENS_API_AVAILABLE
using UnityEngine.Windows.Speech;
#endif

using System.Collections.Generic;
using System.Linq;

namespace MRAT
{
	public class MratVoiceCommands : MonoBehaviour
	{
		private MratCommunicationManager _mratCommunicationManager;
		//private MratPhotoRecorder _photoRecorder;

		// So that this builds against older versions of the Unity DLLs we need to 
		// #if the code that uses HoloLens specific features out.
		// Unity have suggested that UNITY_HOLOGRAPHIC should be defined but we
		// have not seen this work
#if HOLOLENS_API_AVAILABLE

		#region PRIVATE_MEMBERS
		private KeywordRecognizer keywordRecognizer;
		private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
		#endregion //PRIVATE_MEMBERS

		#region MONOBEHAVIOUR_METHODS

		// Use this for initialization
		private void Start()
		{
			_mratCommunicationManager = MratHelpers.GetMratCommunicationManager();
			//_photoRecorder = GameObject.Find("MratManager")?.GetComponent<MratPhotoRecorder>();

			keywords.Add("Visualize", () =>
			{
				VoiceCommandIssued("Visualize");
				ActivateVisualizer();
			});

			keywords.Add("Clear", () =>
			{
				VoiceCommandIssued("Clear");
				DeactivateVisualizer();
			});

            //			keywords.Add("Action", () =>
            //			{
            //				_photoRecorder.TakePictureToFile();
            //
            //				VoiceCommandIssued("Action");
            //				_mratCommunicationManager.SayNow("Rolling");
            //
            //				var msg = new MratEventStatusUpdate("Action", MratEventTypes.CustomStart);
            //				msg.CollectDataFromUnity();
            //				_mratCommunicationManager.LogMratEvent(msg);
            //			});
            //
            //			keywords.Add("Cut", () =>
            //			{
            //				_photoRecorder.TakePictureToFile();
            //
            //				VoiceCommandIssued("Cut");
            //				_mratCommunicationManager.SayNow("That's a wrap");
            //
            //				var msg = new MratEventStatusUpdate("Cut", MratEventTypes.CustomEnd);
            //				msg.CollectDataFromUnity();
            //				_mratCommunicationManager.LogMratEvent(msg);
            //			});

            // Tell the KeywordRecognizer about our keywords.
            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray(), ConfidenceLevel.Low);

			// Register a callback for the KeywordRecognizer and start recognizing!
			keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
			keywordRecognizer.Start();
		}

		#endregion //MONOBEHAVIOUR_METHODS

		#region PRIVATE_METHODS
		private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
		{
			System.Action keywordAction;
			if (keywords.TryGetValue(args.text, out keywordAction))
			{
				keywordAction.Invoke();
			}
		}
		#endregion //PRIVATE_METHODS

#endif // HOLOLENS_API_AVAILABLE

		public void ActivateVisualizer()
		{
			var vizScript = GameObject.Find("MratMRVisualizer")?.GetComponent<MratEventVisualizer>();

			if (vizScript == null) return;

			_mratCommunicationManager.SayNow("Visualizing");
			Debug.Log("Starting visualizer");

			vizScript.StartVisulization();
		}

		public void DeactivateVisualizer()
		{
			var vizScript = GameObject.Find("MratMRVisualizer")?.GetComponent<MratEventVisualizer>();

			if (vizScript == null) return;

			_mratCommunicationManager.SayNow("Stopping visualizer");
			Debug.Log("Stopping visualizer");

			vizScript.StopVisualizing();
		}

		public void VoiceCommandIssued(string command)
		{
			var msg = new MratEventStatusUpdate(command, MratEventTypes.VoiceCommand);
			msg.CollectDataFromUnity();

			_mratCommunicationManager.LogMratEvent(msg);
            Debug.Log("Logged Command ------- " + command);

        }
    }


}
