using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MRAT
{
	public class VisualizePhotos: MonoBehaviour
	{
		public float ShowPhotoDistanceAway = 0.75f;
		public int LayerForPhotos = 2;

		public GameObject PlaceQuadForPhoto(Vector3 originPosition, Quaternion originRotation, float distanceAway = 0.0f, int layer = 0)
		{
			var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			quad.transform.localScale = new Vector3(0.8888f, 0.5f, 0.5f);
			quad.transform.position = originPosition;
			quad.transform.rotation = originRotation;
			quad.transform.position += quad.transform.forward * distanceAway;
			quad.layer = 2;

			return quad;
		}

		public void PhotoReceiver(MratEventPhotoCaptured photoEvent)
		{
//			var quad = PlaceQuadForPhoto(photoEvent.UserPosition, photoEvent.UserRotation, ShowPhotoDistanceAway, LayerForPhotos);
//			var rend = quad.GetComponent<Renderer>();
			// TODO: Update VisualizePhotos to work with new photo system.
			//			rend.material.mainTexture = photoEvent.PhotoTexture2D;
		}

		private IEnumerator LoadFileToObjectTexture(string filePath)
		{
			var quad = GameObject.Find("TestNetworkConnection");

			using (var uwr = UnityWebRequestTexture.GetTexture(filePath))
			{
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.Log("LoadFileToObjectTexture request error: " + uwr.error);
				}
				else
				{
					// Get downloaded asset bundle
					quad.GetComponent<Renderer>().material.mainTexture = DownloadHandlerTexture.GetContent(uwr);
				}
			}

		}

	}
}
