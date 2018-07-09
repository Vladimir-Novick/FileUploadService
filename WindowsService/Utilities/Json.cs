/*

Copyright (C) 2014-2018 by Vladimir Novick http://www.linkedin.com/in/vladimirnovick ,

    vlad.novick@gmail.com , http://www.sgcombo.com , https://github.com/Vladimir-Novick
	

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
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
