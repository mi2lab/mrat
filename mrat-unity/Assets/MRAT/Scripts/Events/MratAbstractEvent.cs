using System;
using UnityEngine;

namespace MRAT
{
    [Serializable]
    public abstract class MratAbstractEvent
    {
	    // ReSharper disable once InconsistentNaming
		/// <summary>
		/// Special field to hold _id returned by MongoDB database, do not try to set manually.
		/// </summary>
	    public string _id;

		public string EventBaseType;
		public string EventColor = "#376301";

        public string UserId;

		/// <summary>
		/// Converts this object to a string, in JSON format.
		/// </summary>
		/// <returns></returns>
		public new string ToString()
		{
			var result = JsonUtility.ToJson(this);
			return result;
		}

		protected MratAbstractEvent()
		{
			EventBaseType = GetType().Name;
		}

		public abstract void CollectDataFromUnity();
	}
}
