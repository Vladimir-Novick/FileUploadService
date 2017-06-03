////////////////////////////////////////////////////////////////////////////
//	Copyright 2014 : Vladimir Novick    https://www.linkedin.com/in/vladimirnovick/  
//
//    NO WARRANTIES ARE EXTENDED. USE AT YOUR OWN RISK. 
//
//      Available under the BSD and MIT licenses
//
// To contact the author with suggestions or comments, use  :vlad.novick@gmail.com
//
////////////////////////////////////////////////////////////////////////////
using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SGCombo.Extensions.Utilites
{
    public class cMsg : EventArgs
	{
        public readonly string type;
        public readonly string Json;

        public cMsg(string Json)
        {
            this.Json = Json;

            jMsg Message = JsonConvert.DeserializeObject<jMsg>(Json);

            this.type = Message.type;
        }

        public T Read<T>()
        {
            if(Json == null) return default(T);

            jMsg<T> Tmp = JsonConvert.DeserializeObject<jMsg<T>>(Json);

            return Tmp.data;
        }

        public string Text 
        {
            get
            {
                object Tmp = Read<object>();
                return Tmp.ToString();
            }
        }
	}

    public interface iMsg 
    {
        string Serialize();
        string type { get; set; }
    }

    public class jMsg : iMsg
    {
        public string type { get; set; }

        public string Serialize()
        {
            throw new NotImplementedException();
        }
    }

    public class sMsg : iMsg
    {
        public string data { get; set; }
        public string type { get; set; }

        public sMsg(string Type, string Data)
        {
            this.data = Data;
            this.type = Type;
        }

        public string Serialize()
        {
            return Json.Serialize(this);
        }
    }

    public class jMsg<T> : iMsg
    {
        public T data { get; set; }
        public string type { get; set; }

        public jMsg(string Type, T Data)
        {
            this.data = Data;
            this.type = Type;
        }

        public string Serialize()
        {
            return Json.Serialize(this);
        }
    }

	public class Json
	{
        public static T Deserialize<T>(string Data)
        {
            if (Data.Length == 0) Utils.Error("Data length");

            return JsonConvert.DeserializeObject<T>(Data);
        }

        public static T Deserialize<T>(string Data, string token)
        {
            JToken Token = JObject.Parse(Data).SelectToken(token);
            return JsonConvert.DeserializeObject<T>(Token.ToObject<string>());
        }

		public static string Serialize<T>(T item)
		{
			return JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
		}
	}
}
