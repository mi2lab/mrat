using System;
using System.Collections.Generic;

namespace MRAT
{
	[Serializable]
    public class MratServerMessage: MratAbstractMessage
    {
        public string User;
        public string Token;
        public List<string> JsonMessages { get; set; } = new List<string>();

	    public MratServerMessage()
	    {
	    }

	    public MratServerMessage(string user, string token): this(user, token, null)
	    {
	    }

		public MratServerMessage(string jsonMessage) : this("", "", jsonMessage)
        {
        }

        public MratServerMessage(string user, string token, string jsonMessage)
        {
            User = user;
            Token = token;

	        if (!string.IsNullOrEmpty(jsonMessage))
	        {
		        JsonMessages.Add(jsonMessage);
	        }
        }

	    public override Dictionary<string, string>  ToDictionary()
	    {
		    var jsonMessage = "{}";

		    if (JsonMessages.Count == 1)
		    {
			    jsonMessage = JsonMessages[0];
		    }
		    else if (JsonMessages.Count > 1)
		    {
			    jsonMessage = $"[{string.Join(",", JsonMessages)}]";
		    }

		    return new Dictionary<string, string>
		    {
			    { "user", User },
			    { "token", Token },
			    { "doc", jsonMessage }
		    }; 
	    }
    }
}
