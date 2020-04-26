using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using Debug = UnityEngine.Debug;
// ReSharper disable CheckNamespace

namespace MRAT
{
    public class MratPhotoRecorder : MonoBehaviour
    {
	    public bool CaptureHolograms = false;
	    public MratPhotoCapturedEventReceiverUnityEvent PhotoEventReceiver;

		[HideInInspector]
        public Resolution CameraResolution;
		
        private CameraParameters _cameraParameters;
	    private string _persistantPath;
	    private MratCommunicationManager _communicationManager;
		
		public void Start()
		{
		    _persistantPath = Application.persistentDataPath;
			_communicationManager = MratHelpers.GetMratCommunicationManager();
		}

	    public void TakePictureToFile(string filename = "")
	    {
		    var watch = Stopwatch.StartNew();

			var photoEvent = new MratEventPhotoCaptured
		    {
			    ImageHeight = _cameraParameters.cameraResolutionHeight,
			    ImageWidth = _cameraParameters.cameraResolutionWidth
		    };

		    photoEvent.CollectDataFromUnity();

		    if (string.IsNullOrEmpty(filename))
		    {
			    filename = photoEvent.MakeMratFilename();
		    }

		    photoEvent.ImageFileName = filename;

		    var filePath = Path.Combine(_persistantPath, filename);

		    var resolutions = PhotoCapture.SupportedResolutions.ToArray();

		    if (!resolutions.Any(res => res.height > 0 && res.width > 0))
		    {
			    // No valid resolutions, which usually means there is no camera available.
			    Debug.Log("EnablePictureMode cannot complete, no resolutions available (probably no camera found).");
			    return;
		    }

		    // HoloLens has max photo resolution of 2048 x 1152 (16:9 aspect ratio)
		    CameraResolution = resolutions.OrderByDescending(res => res.width * res.height).First();

//		    Debug.Log("TakePictureToFile: Available camera resolutions: \n");
//
//		    foreach (var res in resolutions.OrderByDescending(res => res.width * res.height))
//		    {
//			    Debug.Log($"Width: {res.width}, Height: {res.height}, Resolution: {res.width * res.height}");
//		    }

			_cameraParameters = new CameraParameters
		    {
			    hologramOpacity = CaptureHolograms ? 1f : 0f,
			    cameraResolutionWidth = CameraResolution.width,
			    cameraResolutionHeight = CameraResolution.height,
			    pixelFormat = CapturePixelFormat.BGRA32
		    };

		    try
		    {
			    PhotoCapture.CreateAsync(CaptureHolograms, delegate(PhotoCapture captureObject)
			    {
				    captureObject.StartPhotoModeAsync(_cameraParameters,
					    delegate(PhotoCapture.PhotoCaptureResult photoModeResult)
					    {
						    if (photoModeResult.success)
						    {
							    Debug.Log("OnPhotoModeStarted success, ready to take picture");
						    }
						    else
						    {
							    Debug.LogError("Unable to start photo mode!");

							    captureObject.StopPhotoModeAsync(delegate { captureObject.Dispose(); });

							    return;
						    }

						    captureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.PNG,
							    delegate(PhotoCapture.PhotoCaptureResult photoResult)
							    {
								    var time = Time.realtimeSinceStartup;
								    Debug.Log(photoResult.success
									    ? time + " OnCapturedPhotoToDisk: Saved photo to disk"
									    : time + " OnCapturedPhotoToDisk: Failed to save photo to disk!");

								    watch.Stop();

								    Debug.Log("CaptureImageToFile done in " + watch.ElapsedMilliseconds);

								    PhotoEventReceiver.Invoke(photoEvent);

								    _communicationManager.SayNext("Photo taken");

								    captureObject.StopPhotoModeAsync(delegate { captureObject.Dispose(); });
							    });
					    });
			    });
		    }
		    catch (Exception e)
		    {
				Debug.Log("Error during photo recording: " + e);
		    }

			Debug.Log(Time.realtimeSinceStartup + " Saving photo to file with path: " + filePath);
		}
    }
}

