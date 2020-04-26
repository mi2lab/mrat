using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRAT
{
	[Serializable]
	public abstract class MratAbstractMessage
	{
		/// <summary>
		/// Converts this object to a string, in JSON format.
		/// </summary>
		/// <returns></returns>
		public new string ToString()
		{
			var result = JsonUtility.ToJson(this);
			return result;
		}

		public abstract Dictionary<string, string> ToDictionary();
	}
}
