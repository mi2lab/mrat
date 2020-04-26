using System;

namespace MRAT
{
    /// <summary>
    /// MratEvent for handling Input events. 
    /// </summary>
    [Serializable]
    public class MratEventInput : MratEventStatusUpdate
    {
	    public MratInputSource InputSource;
		
	    public MratEventInput(MratEventTypes type = MratEventTypes.Custom): base(string.Empty, type)
		{
	    }

	    public MratEventInput(MratInputSource inputSource, MratEventTypes type = MratEventTypes.Custom) : base(string.Empty, type)
	    {
		    InputSource = inputSource;
	    }

	}
}
