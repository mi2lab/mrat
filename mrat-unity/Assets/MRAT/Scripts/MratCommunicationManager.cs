using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace MRAT
{
    public class MratCommunicationManager : MonoBehaviour
    {
	    [HideInInspector]
	    public static MratCommunicationManager Instance;

        /// <summary>
        /// If checked then no messages will be sent to the server, only logged locally
        /// </summary>
        public bool LocalLoggingOnly;
        public bool LogGeneratedMessagesLocally = true;
	    public bool SpeachOutputEnabled = true;

	    public string CommandMode
	    {
		    get
		    {
				return _commandMode;
		    }

		    set
		    {
			    if (_commandMode != null && _commandMode != value)
			    {
				    _commandMode = value;
			    }
		    }
	    }

	    private string _commandMode = "None"; 
	    private string _appName = "not configured";
        private string _sessionId = "not configured";
        private string _userId = "not configured";
	    private TextToSpeech _speaker;
	    private readonly LinkedList<string> _toSpeakList = new LinkedList<string>();

	    private MratDaemon _daemon;

	    public void HandleCommandModeChange(string modeName)
	    {
		    CommandMode = modeName;
		}

	    private void Awake()
	    {
		    _appName = SceneManager.GetActiveScene().name;
            _sessionId = Guid.NewGuid().ToString();
            _userId = "user" + Guid.NewGuid().ToString();
            _daemon = MratHelpers.GetMratDaemon();
		    _speaker = MratHelpers.GetSpeaker();

			if (Instance == null)
		    {
			    Instance = this;
			}
			else if (Instance != this)
		    {
			    Destroy(gameObject);
			}
		}

		private void Start()
        {
			if (LocalLoggingOnly)
            {
                Debug.Log("LocalLoggingOnly activated, so no messages will be sent to the server");

				SayNext("Local logging only mode, no data will be collected");
            }

            SendStartupMessage();

	        StartCoroutine(nameof(CoroutineSpeachHandler));
        }

	    public void SayNow(string textToSay)
	    {
		    _toSpeakList.AddFirst(textToSay);
	    }

	    public void SayNext(string textToSay)
	    {
		    _toSpeakList.AddLast(textToSay);
	    }

	    private IEnumerator CoroutineSpeachHandler()
	    {
		    // ReSharper disable once TooWideLocalVariableScope
		    string textToSay;

		    while (true)
		    {
			    yield return new WaitWhile(() => _toSpeakList.Count <= 0 
			                                     || _speaker.SpeechTextInQueue() 
			                                     || _speaker.IsSpeaking());

			    textToSay = _toSpeakList.First.Value;

				_toSpeakList.RemoveFirst();

			    _speaker.StartSpeaking(textToSay);
			}

		    // ReSharper disable once IteratorNeverReturns
		}

		private void SendStartupMessage()
        {
            var e = new MratEventSimple("Application started", MratEventTypes.AppStart);
			e.CollectDataFromUnity();

            LogMratEvent(e);

			SayNow("Application started");
        }

	    public void CurrentStatus()
	    {
		    if (LocalLoggingOnly)
		    {
			    Debug.Log("Status: local logging only");
				SayNext("Local logging only");
			}
		    else
		    {
			    Debug.Log("Status: data will be saved remotely");
			    SayNext("Data will be saved remotely");
			}
		}

		/// <summary>
		/// Simply call to log an MratEvent, which can  be of type MratEventSimple or type derived from it.
		/// </summary>
		/// <param name="mratEvent"></param>
		public void LogMratEvent(MratEventSimple mratEvent)
        {
	        mratEvent.AppName = _appName;
	        mratEvent.SessionId = _sessionId;
            mratEvent.UserId = _userId;

			if (string.IsNullOrEmpty(mratEvent.Mode))
	        {
		        mratEvent.Mode = CommandMode;
	        }

	        if (LocalLoggingOnly)
	        {
				if (LogGeneratedMessagesLocally) Debug.Log($"MratEventSimple ({mratEvent.EventType}) logged only locally: {mratEvent.ToString()}");
			}
	        else
	        {
		        try
		        {
			        _daemon.SendSimpleEvent(mratEvent);
		        }
		        catch (Exception e)
		        {
					Debug.LogError("Weird error: " + e);
		        }


				if (LogGeneratedMessagesLocally) Debug.Log($"MratEventSimple ({mratEvent.EventType}) given to daemon: {mratEvent.ToString()}");
			}
        }

	    public void LogMratImageEvent(MratEventImage imageEvent)
	    {
		    if (LocalLoggingOnly)
		    {
			    if (LogGeneratedMessagesLocally) Debug.Log("LogMratImageEvent logged only locally: " + imageEvent.ToString());
		    }
		    else
		    {
		        _daemon.SendImageEvent(imageEvent);

		        if (LogGeneratedMessagesLocally) Debug.Log("LogMratImageEvent given to daemon: " + imageEvent.ToString());
		    }
		}

	    public Task<List<MratEventSimple>> GetEventsBySessionId(string sessionId)
	    {
		    return _daemon.GetEventsBySessionId(sessionId);
	    }

	    public Task<List<MratEventSimple>> GetEventsFromInteractiveEndPoint()
	    {
		    return _daemon.GetEventsFromInteractiveEndpoint();
	    }

	    public Task<bool> GetInteractiveDataStatus()
	    {
		    return _daemon.GetInteractiveDataStatus();
	    }

	}
}
