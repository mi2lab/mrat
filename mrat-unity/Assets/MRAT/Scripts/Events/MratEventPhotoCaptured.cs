using System;
using HoloToolkit.Unity;
using UnityEngine;

namespace MRAT
{
    /// <summary>
    /// MratEvent for handling photocapture events. 
    /// </summary>
    [Serializable]
    public class MratEventPhotoCaptured : MratEventImage
    {
	    public Vector3 UserPosition;
	    public Quaternion UserRotation;

	    public MratEventPhotoCaptured(string message = "", MratEventTypes type = MratEventTypes.PhotoCaptured) : base(message, type)
        {
        }

	    public override void CollectDataFromUnity()
	    {
		    base.CollectDataFromUnity();

		    var camera = CameraCache.Main.transform;

		    UserPosition = camera.position;
		    UserRotation = camera.rotation;
		}
	}
}
