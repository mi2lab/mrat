using System;
using UnityEngine;
using UnityEngine.Events;

namespace MRAT
{
	[Serializable]
	public class MratTextureReceiverUnityEvent : UnityEvent<Texture2D>
	{
	}

	[Serializable]
	public class MratPhotoCapturedEventReceiverUnityEvent : UnityEvent<MratEventPhotoCaptured>
	{
	}

	[Serializable]
	public class MratGameObjectUnityEvent : UnityEvent<GameObject>
	{
	}
}
