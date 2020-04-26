using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace MRAT
{
	[Serializable]
	public class MratSessionIds
	{
		[SuppressMessage("ReSharper", "InconsistentNaming")] 
		public string[] sessionIds;

		public static MratSessionIds CreateFromJson(string jsonString)
		{
			if (string.IsNullOrEmpty(jsonString))
			{
				return null;
			}

			return JsonUtility.FromJson<MratSessionIds>(jsonString);
		}
	}
}
