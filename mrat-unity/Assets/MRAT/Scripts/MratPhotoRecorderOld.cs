using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using Debug = UnityEngine.Debug;

namespace MRAT
{
    public class MratPhotoRecorderOld : MonoBehaviour
    {
        // Code originally based on: https://developer.microsoft.com/en-us/windows/mixed-reality/locatable_camera_in_unity
        public bool EnablePictureModeOnStart = true;
	    public bool CaptureHolograms;
	    public MratPhotoCapturedEventReceiverUnityEvent PhotoEventReceiver;

		[HideInInspector]
        public Resolution CameraResolution;

        /// <summary>
        /// Property to allow turning on and off of picture mode. If turned off, will release access to camera.
        /// </summary>
        public bool TakePictures {
            get { return _takePictures; }
            set
            {
                if (value == _takePictures) return;
                _takePictures = value;

                if (value)
                {
                    EnablePictureMode();
                }
                else
                {
                    DisablePictureMode();
                }
            }
        }

        private bool _takePictures = false;

        private PhotoCapture _photoCaptureObject;
        private CameraParameters _cameraParameters;
        private bool _readyToTakePicture;
	    private string _persistantPath;

		public void Start()
		{
		    _persistantPath = Application.persistentDataPath;

			if (EnablePictureModeOnStart)
			{
				Invoke(nameof(DelayedEnablePictureMode), 2.0f);
			}
        }

	    private void DelayedEnablePictureMode()
	    {
			Debug.Log("Enabling picture mode...");
		    TakePictures = true;
	    }
		
        private void EnablePictureMode()
        {
            var resolutions = PhotoCapture.SupportedResolutions.ToArray();

            if (!resolutions.Any(res => res.height > 0 && res.width > 0))
            {
                // No valid resolutions, which usually means there is no camera available.
                Debug.Log("EnablePictureMode cannot complete, no resolutions available (probably no camera found). Disabling picture mode.");
                _takePictures = false;
                _readyToTakePicture = false;

                return;
            }

            Debug.Log("EnablePictureMode: Available camera resolutions: \n");

            foreach (var res in resolutions)
            {
                Debug.Log($"Width: {res.width}, Height: {res.height}, Resolution: {res.width * res.height}");
            }

            // HoloLens has max photo resolution of 2048 x 1152 (16:9 aspect ratio)
            CameraResolution = resolutions.OrderByDescending(res => res.width * res.height).First();

            _cameraParameters = new CameraParameters
            {
                hologramOpacity = CaptureHolograms ? 1f : 0f,
                cameraResolutionWidth = CameraResolution.width,
                cameraResolutionHeight = CameraResolution.height,
                pixelFormat = CapturePixelFormat.BGRA32
            };

            var aspectRatio = MratHelpers.GetAspectRatio(CameraResolution.width, CameraResolution.height);

            Debug.Log("Camera resolution: " + CameraResolution.width + " wide by " + CameraResolution.height +
                      " high = " + CameraResolution.width * CameraResolution.height + ", aspect ratio: " +
                      aspectRatio.x + ":" + aspectRatio.y);
            
            PhotoCapture.CreateAsync(CaptureHolograms, OnPhotoCaptureCreated);
        }

        private void DisablePictureMode()
        {
            _photoCaptureObject?.StopPhotoModeAsync(OnPhotoModeStopped);
            _readyToTakePicture = false;
            _takePictures = false;
        }

        public void CaptureImageToFile(string filename = "")
	    {
	        if (!TakePictures)
	        {
	            Debug.Log("CaptureImageToFile called, but TakePictures is off - ignoring command.");
	            return;
	        }

            if (_readyToTakePicture && _photoCaptureObject != null)
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

				_photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.PNG, OnCapturedPhotoToDisk);

				watch.Stop();

				Debug.Log("CaptureImageToFile done in " + watch.ElapsedMilliseconds);

				Debug.Log(Time.realtimeSinceStartup + " Saving photo to file with path: " + filePath);

				PhotoEventReceiver.Invoke(photoEvent);
			}
			else
		    {
			    Debug.Log("CaptureImageToFile called, but not ready to take picture yet. Ignoring command.");
		    }
	    }

        private void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
	    {
		    var time = Time.realtimeSinceStartup;
		    Debug.Log(result.success ? time + " OnCapturedPhotoToDisk: Saved photo to disk" : time + " OnCapturedPhotoToDisk: Failed to save photo to disk!");
	    }

		private void OnPhotoCaptureCreated(PhotoCapture captureObject)
        {
            _photoCaptureObject = captureObject;

            _photoCaptureObject.StartPhotoModeAsync(_cameraParameters, OnPhotoModeStarted);
        }

        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                _readyToTakePicture = true;
				Debug.Log("OnPhotoModeStarted success, ready to take pictures.");
            }
            else
            {
                Debug.LogError("Unable to start photo mode!");
            }
        }

        private void OnPhotoModeStopped(PhotoCapture.PhotoCaptureResult result)
        {
            _photoCaptureObject.Dispose();
            _photoCaptureObject = null;
        }

		private void OnDestroy()
        {
	        _photoCaptureObject?.StopPhotoModeAsync(OnPhotoModeStopped);
	        _readyToTakePicture = false;
            _takePictures = false;
        }
    }
}

