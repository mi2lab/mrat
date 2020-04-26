using System;

namespace MRAT
{
    /// <summary>
    /// MratEvent for handling image-related events. 
    /// </summary>
    [Serializable]
    public abstract class MratEventImage : MratEventSimple
    {
	    public string ImageFileName;

	    public int ImageHeight;
	    public int ImageWidth;

		protected MratEventImage(string message, MratEventTypes type = MratEventTypes.PhotoCaptured) : base(message, type)
	    {
	    }

	    public string MakeMratFilename()
	    {
			return  ImageFileName = $"MRAT_{EventType}_{Timestamp}.png";
		}

	}
}
