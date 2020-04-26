using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MRAT
{
	public class MratWebTextureLoader : MonoBehaviour
	{
		/// <summary>
		/// Set alpha of image of after texture is loaded, with valid range 0-1. Set to negative if alpha should not be changed at all.
		/// </summary>
		[Range(-0.1f,1f)]
		public float ImageAlphaValue = -0.1f;

		public bool LogProgress;

		public void LoadTexture(string texturePath)
		{
			StartCoroutine(nameof(CoroutineGetTexture), texturePath);
		}

		private  IEnumerator CoroutineGetTexture(string texturePath)
		{
			var www = UnityWebRequestTexture.GetTexture(texturePath);

			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError)
			{
				Debug.Log($"CoroutineGetTexture failed, with path: {texturePath}, error: {www.error}");
			}
			else
			{
				if (LogProgress) Debug.Log("Loading texture from path: " + texturePath);

				var rend = GetComponent<Renderer>();

				rend.material.mainTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

				if (!(ImageAlphaValue < 0)) yield break;

				var color = rend.material.color;
				color.a = ImageAlphaValue;

				rend.material.SetColor(0, color);
			}
		}
	}
}
