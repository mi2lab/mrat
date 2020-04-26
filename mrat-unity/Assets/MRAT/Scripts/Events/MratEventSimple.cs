using System;
using UnityEngine;

namespace MRAT
{
    /// <summary>
    /// Custom simple MRAT event message, which is also the base class for specialised event types. 
    /// </summary>
    [Serializable]
    public class MratEventSimple: MratAbstractEvent
    {
        /// <summary>
        /// This will be automatically overridden by the server, so ignore it. 
        /// </summary>
        public string AppName;

        /// <summary>
        /// This will be automatically overridden by the server, so ignore it. 
        /// </summary>
        public string SessionId;

	    public string Mode;

        public MratEventTypes EventTypeId;
        public string EventType;
		
        public long Timestamp;

        public string Message;

	    public float TimeAtFrameBeginningSeconds;

	    public float TimeSinceSessionStartupSeconds;

	    public string DateTimeIsoLocal;

		public int FrameCount;

        public MratEventSimple(string message = "", MratEventTypes eventType = MratEventTypes.Custom)
        {
            EventTypeId = eventType;
            EventType = eventType.ToString();
            Message = message;
        }

		public override void CollectDataFromUnity()
	    {
		    Timestamp = MratHelpers.GetTimestamp();

		    TimeAtFrameBeginningSeconds = Time.time;
		    TimeSinceSessionStartupSeconds = Time.realtimeSinceStartup;

		    FrameCount = Time.frameCount;

		    DateTimeIsoLocal = DateTime.Now.ToString("o");
		}
    }
}