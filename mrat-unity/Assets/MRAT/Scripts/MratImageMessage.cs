using System;
using System.Collections.Generic;

namespace MRAT
{
    /// <summary>
    /// Specialized MRAT event message only for sending photos.. 
    /// </summary>
    [Serializable]
    public class MratImageMessage: MratAbstractMessage
    {
	    public string FileName;

		public MratImageMessage(string fileName)
        {
	        FileName = fileName;
        }

	    public MratImageMessage(MratEventImage photoCaptured)
	    {
	        FileName = photoCaptured.ImageFileName;
        }

		public override Dictionary<string, string> ToDictionary()
		{
			return new Dictionary<string, string>
			{
				{ "fileName", FileName }
			};
		}
    }
}