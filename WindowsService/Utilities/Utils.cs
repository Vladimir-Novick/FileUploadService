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
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Globalization;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace SGCombo.Extensions.Utilites
{
    public static class Utils
	{
		public static Encoding cLatin = Encoding.GetEncoding(1252);
        public static Encoding cUTF = new UTF8Encoding(false, true);

		private static string Base62CodingSpace = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		public static string FromLatin(byte[] data)
		{
            if (data == null) return null;

            return FromLatin(data, 0, data.Length);
		}

        public static string FromLatin(byte[] data, int index, int count)
        {
            if (data == null) return null;

            return cLatin.GetString(data, index, count);
        }

        public static string FromUTF(byte[] data)
        {
            if (data == null) return null;

			return FromUTF(data, 0, data.Length);
		}

        public static string FromUTF(byte[] data, int index, int count)
		{
			if (data == null) return null;

            return cUTF.GetString(data, index, count); 
		}

  		public static byte[] ToUTF(string data)
		{
			if (data == null) return null;

            return cUTF.GetBytes(data);
		}

		public static byte[] ToLatin(string data)
		{
			if (data == null) return null;

			return cLatin.GetBytes(data);
		}

		public static byte[] ToSize(byte[] byteArray, int len)
		{
			if (byteArray.Length == len) return byteArray;

			byte[] tmp = new byte[len];

			int size = byteArray.Length;
			if (size > len) size = len;

			Array.Copy(byteArray, tmp, size);

			return tmp;
		}

		public static string TrimFromZero(string input)
		{
			int index = input.IndexOf('\0');
			if (index < 0) return input.TrimEnd();
			return input.Substring(0, index).TrimEnd();
		}

		public static void Error(string sMsg)
		{
			Console.WriteLine(sMsg);

			throw new Exception(sMsg);
		}

		/// <summary>
		/// Convert a byte array
		/// </summary>
		/// <param name="original">Byte array</param>
		/// <returns>Base62 string</returns>
		public static string ToBase62(byte[] original)
		{
			StringBuilder sb = new StringBuilder();
			BitStream stream = new BitStream(original);         // Set up the BitStream
			byte[] read = new byte[1];                          // Only read 6-bit at a time
			while (true)
			{
				read[0] = 0;
				int length = stream.Read(read, 0, 6);           // Try to read 6 bits
				if (length == 6)                                // Not reaching the end
				{
					if ((int)(read[0] >> 3) == 0x1f)            // First 5-bit is 11111
					{
						sb.Append(Base62CodingSpace[61]);
						stream.Seek(-1, SeekOrigin.Current);    // Leave the 6th bit to next group
					}
					else if ((int)(read[0] >> 3) == 0x1e)       // First 5-bit is 11110
					{
						sb.Append(Base62CodingSpace[60]);
						stream.Seek(-1, SeekOrigin.Current);
					}
					else                                        // Encode 6-bit
					{
						sb.Append(Base62CodingSpace[(int)(read[0] >> 2)]);
					}
				}
				else
				{
					// Padding 0s to make the last bits to 6 bit
					sb.Append(Base62CodingSpace[(int)(read[0] >> (int)(8 - length))]);
					break;
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Convert a Base62 string to byte array
		/// </summary>
		/// <param name="base62">Base62 string</param>
		/// <returns>Byte array</returns>
		public static byte[] FromBase62(string base62)
		{
			// Character count
			int count = 0;

			// Set up the BitStream
			BitStream stream = new BitStream(base62.Length * 6 / 8);

			foreach (char c in base62)
			{
				// Look up coding table
				int index = Base62CodingSpace.IndexOf(c);

				// If end is reached
				if (count == base62.Length - 1)
				{
					// Check if the ending is good
					int mod = (int)(stream.Position % 8);
					stream.Write(new byte[] { (byte)(index << (mod)) }, 0, 8 - mod);
				}
				else
				{
					// If 60 or 61 then only write 5 bits to the stream, otherwise 6 bits.
					if (index == 60)
					{
						stream.Write(new byte[] { 0xf0 }, 0, 5);
					}
					else if (index == 61)
					{
						stream.Write(new byte[] { 0xf8 }, 0, 5);
					}
					else
					{
						stream.Write(new byte[] { (byte)index }, 2, 6);
					}
				}
				count++;
			}

			// Dump out the bytes
			byte[] result = new byte[stream.Position / 8];
			stream.Seek(0, SeekOrigin.Begin);
			stream.Read(result, 0, result.Length * 8);
			return result;
		}

		public static string HtmlEncode(string text)
		{
			string sOut = System.Net.WebUtility.HtmlEncode(text);
			StringBuilder tOut = new StringBuilder(sOut.Length * 2);

			foreach (char c in sOut.ToCharArray())
			{
				int value = Convert.ToInt32(c);

				if ((value > 31) && (value < 127) && (value != 96))
				{
					tOut.Append(c);
				}
				else
				{
					tOut.Append("&#");
					tOut.Append(value);
					tOut.Append(";");
				}
			}

			return tOut.ToString();
		}

        public static bool IsNumber(string aNumber)
        {
            long tmp;
            return long.TryParse(aNumber, out tmp);
        }

        private static string GetTemp()
		{
            string uDir = System.IO.Path.Combine(System.IO.Path.GetTempPath() , "TEMEC");
            if(!Directory.Exists(uDir))  Directory.CreateDirectory(uDir);

			return uDir;
		}

		public static string GetFile(string fname)
		{
			return GetFile(GetTemp(), fname);
		}

        public static string GetFile(string sdir, string fname)
		{
            if (!Directory.Exists(sdir)) Directory.CreateDirectory(sdir);

            string uDir = System.IO.Path.Combine(sdir, Guid.NewGuid().ToString());
            if (!Directory.Exists(uDir)) Directory.CreateDirectory(uDir);

			return System.IO.Path.Combine(sdir, fname);
		}

        public static string toHex(byte[] Input)
        {
            StringBuilder hex = new StringBuilder(Input.Length * 2);

            foreach (byte b in Input)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            
            return hex.ToString().ToLower();
        }

		public static bool hasConnection
		{
			get {
                var ping = new Ping();
				try
				{
                    // Ping Google's public DNS server (8.8.8.8)
                    var reply = ping.Send(new IPAddress(134744072)); 

                    return (reply.Status == IPStatus.Success);
				}
				catch { return false; }
			}
		}

		public static void Raise<T>(EventHandler<T> Evt, T Args) where T : EventArgs
		{
			if (Evt == null) return;

			var eventListeners = Evt.GetInvocationList();

			for (int index = 0; index < eventListeners.Count(); index++)
			{
				var methodToInvoke = (EventHandler<T>)eventListeners[index];
				methodToInvoke.BeginInvoke(null, Args, CompletionCallback<T>, null);
			}
		}

		private static void CompletionCallback<T>(IAsyncResult iar) where T : EventArgs
		{
			try
			{
				var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
				var invokedMethod = (EventHandler<T>)ar.AsyncDelegate;
				invokedMethod.EndInvoke(iar);
			}
			catch (SocketException se)
			{
				if (se.GetType() == typeof(System.NullReferenceException)) return;
				Console.WriteLine("SocketException: " + se.Message);
				throw;
			}
			catch (Exception ex)
			{
				if (ex.GetType() == typeof(System.NullReferenceException)) return;
				Console.WriteLine("CompletionCallback: " + ex.Message);
				throw;
			}
		}

		public static byte[] Compress(byte[] buffer)
		{
			using (var compressStream = new MemoryStream())
			using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress, false))
			{
                compressor.Write(buffer, 0, buffer.Length);
				compressor.Close();

				return compressStream.ToArray();
			}
		}

        public static byte[] Decompress(byte[] buffer)
        {
            MemoryStream input = new MemoryStream(buffer, false);

            using (var decompressStream = new MemoryStream())
            using (var decompressor = new DeflateStream(input, CompressionMode.Decompress, false))
            {
                decompressor.CopyTo(decompressStream);
                decompressor.Close();

                return decompressStream.ToArray();
            }
        }

		public static string StripNull(string input)
		{
			if (input == null) return null;

			int index = input.IndexOf('\0');
			if (index < 0) return input.TrimEnd();
			return input.Substring(0, index).TrimEnd();
		}

        public static string AddHttp(string str)
        {
            return "http://" + Utils.TrimSlash(str) + "/";
        }

        public static string AddHttps(string str)
        {
            return "https://" + Utils.TrimSlash(str) + "/";
        }

        public static string TrimSlash(string str)
        {
            int lastSlash = str.LastIndexOf('/');
            str = (lastSlash > -1) ? str.Substring(0, lastSlash) : str;

            return str;
        }

		public static UInt32 Timestamp
		{
			get
			{
				TimeSpan Delta = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
				return (UInt32)Delta.TotalSeconds;
			}
		}

        public static IPAddress Host2IP(string Host)
        {
            IPAddress[] IPs;

            try
            {
                IPs = Dns.GetHostAddresses(Host);
            }
            catch
            {
                return null;
            }

            foreach (IPAddress Address in IPs)
            {
                if (Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return Address;
                }
            }

            return null;
        }

		public static byte[] StructureToByteArray(object obj)
		{
			int len = Marshal.SizeOf(obj);
			byte[] bytearray = new byte[len];

			StructureToByteArray(bytearray, 0, obj);

			return bytearray;
		}

		public static bool StructureToByteArray(byte[] bytearray, int offset, object obj)
		{
			bool bRet = true;
			int len = Marshal.SizeOf(obj);

			if (len != (bytearray.Length - offset))
			{
				bRet = false;
				Utils.Error("Wrong structure size");
			}

			IntPtr ptr = Marshal.AllocHGlobal(len);
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, bytearray, offset, Math.Min(len, (bytearray.Length - offset)));
			Marshal.FreeHGlobal(ptr);

			return bRet;
		}

		public static bool ByteArrayToStructure(byte[] bytearray, int offset, ref object obj)
		{
			bool bRet = true;
			int len = Marshal.SizeOf(obj);

			if (len > (bytearray.Length - offset))
			{
				bRet = false;
				Utils.Error("Wrong structure size");
			}

			IntPtr i = Marshal.AllocHGlobal(len);
			Marshal.Copy(bytearray, offset, i, Math.Min(len, bytearray.Length - offset));
			obj = Marshal.PtrToStructure(i, obj.GetType());
			Marshal.FreeHGlobal(i);

			return bRet;
		}

		public static string sha256(byte[] data)
		{
			SHA256Managed crypt = new SHA256Managed();
			byte[] crypto = crypt.ComputeHash(data, 0, data.Length);

			string hash = String.Empty;

			foreach (byte bit in crypto)
			{
				hash += bit.ToString("x2");
			}

			return hash;
		}

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

		public static IEnumerable<byte[]> ArraySplit(byte[] bArray, int intBufforLengt)
		{
			int bArrayLenght = bArray.Length;
			byte[] bReturn = null;

			int i = 0;
			for (; bArrayLenght > (i + 1) * intBufforLengt; i++)
			{
				bReturn = new byte[intBufforLengt];
				Array.Copy(bArray, i * intBufforLengt, bReturn, 0, intBufforLengt);
				yield return bReturn;
			}

			int intBufforLeft = bArrayLenght - i * intBufforLengt;
			if (intBufforLeft > 0)
			{
				bReturn = new byte[intBufforLeft];
				Array.Copy(bArray, i * intBufforLengt, bReturn, 0, intBufforLeft);
				yield return bReturn;
			}
		}

        public static string DownloadString(string sUrl)
        {
            using (WebClient myWebClient = new WebClient())
            {
                string sData = null;

                try
                {
                    sData = myWebClient.DownloadString(sUrl);
                }
                catch
                {
                    return null;
                }

                return sData;
            }
        }

        public static string DownloadString(string sUrl, string Parameters)
        {
            try
            {
                System.Net.WebRequest Request = System.Net.WebRequest.Create(sUrl);

                Request.Method = "POST";
                Request.ContentType = "application/x-www-form-urlencoded";

                byte[] bytes = Utils.ToLatin(Parameters);
                Request.ContentLength = bytes.Length;

                System.IO.Stream os = Request.GetRequestStream();

                os.Write(bytes, 0, bytes.Length); //Push it out there
                os.Close();

                System.Net.WebResponse resp = Request.GetResponse();

                if (resp == null) return null;

                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

                string sOut = sr.ReadToEnd().Trim();

                if (sOut.Length == 0) return null;

                return sOut;
            }
            catch
            {
                return null;
            }
        }
    }
    
    public class TimerHelper
    {
        public string Desc = "None";
        public volatile bool isRunning = false;

        private System.Timers.Timer _timer = new System.Timers.Timer();

        public event EventHandler<TextArgs> TimerEvent;

        public TimerHelper()
        {
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Stop();

            if(TimerEvent != null) TimerEvent(this, new TextArgs(Desc));
        }

        public void Stop()
        {
            isRunning = false;

            if (_timer != null) { _timer.Stop(); }
        }

        public void Close()
        {
            Stop();

            _timer = null;
        }

        public void Start(int Ms, string Desc)
        {
            Stop();

            isRunning = true;
            this.Desc = Desc;

            _timer.Interval = Ms;
            _timer.AutoReset = false;

            _timer.Start();
        }
    }

	/// <summary>
	/// Utility that read and write bits in byte array
	/// </summary>
	internal class BitStream : Stream
	{
		private byte[] Source { get; set; }

		/// <summary>
		/// Initialize the stream with capacity
		/// </summary>
		/// <param name="capacity">Capacity of the stream</param>
		public BitStream(int capacity)
		{
			this.Source = new byte[capacity];
		}

		/// <summary>
		/// Initialize the stream with a source byte array
		/// </summary>
		/// <param name="source"></param>
		public BitStream(byte[] source)
		{
			this.Source = source;
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Bit length of the stream
		/// </summary>
		public override long Length
		{
			get { return Source.Length * 8; }
		}

		/// <summary>
		/// Bit position of the stream
		/// </summary>
		public override long Position { get; set; }

		/// <summary>
		/// Read the stream to the buffer
		/// </summary>
		/// <param name="buffer">Buffer</param>
		/// <param name="offset">Offset bit start position of the stream</param>
		/// <param name="count">Number of bits to read</param>
		/// <returns>Number of bits read</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			// Temporary position cursor
			long tempPos = this.Position;
			tempPos += offset;

			// Buffer byte position and in-byte position
			int readPosCount = 0, readPosMod = 0;

			// Stream byte position and in-byte position
			long posCount = tempPos >> 3;
			int posMod = (int)(tempPos - ((tempPos >> 3) << 3));

			while (tempPos < this.Position + offset + count && tempPos < this.Length)
			{
				// Copy the bit from the stream to buffer
				if ((((int)this.Source[posCount]) & (0x1 << (7 - posMod))) != 0)
				{
					buffer[readPosCount] = (byte)((int)(buffer[readPosCount]) | (0x1 << (7 - readPosMod)));
				}
				else
				{
					buffer[readPosCount] = (byte)((int)(buffer[readPosCount]) & (0xffffffff - (0x1 << (7 - readPosMod))));
				}

				// Increment position cursors
				tempPos++;
				if (posMod == 7)
				{
					posMod = 0;
					posCount++;
				}
				else
				{
					posMod++;
				}
				if (readPosMod == 7)
				{
					readPosMod = 0;
					readPosCount++;
				}
				else
				{
					readPosMod++;
				}
			}
			int bits = (int)(tempPos - this.Position - offset);
			this.Position = tempPos;
			return bits;
		}

		/// <summary>
		/// Set up the stream position
		/// </summary>
		/// <param name="offset">Position</param>
		/// <param name="origin">Position origin</param>
		/// <returns>Position after setup</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case (SeekOrigin.Begin):
					{
						this.Position = offset;
						break;
					}
				case (SeekOrigin.Current):
					{
						this.Position += offset;
						break;
					}
				case (SeekOrigin.End):
					{
						this.Position = this.Length + offset;
						break;
					}
			}
			return this.Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Write from buffer to the stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset">Offset start bit position of buffer</param>
		/// <param name="count">Number of bits</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			// Temporary position cursor
			long tempPos = this.Position;

			// Buffer byte position and in-byte position
			int readPosCount = offset >> 3, readPosMod = offset - ((offset >> 3) << 3);

			// Stream byte position and in-byte position
			long posCount = tempPos >> 3;
			int posMod = (int)(tempPos - ((tempPos >> 3) << 3));

			while (tempPos < this.Position + count && tempPos < this.Length)
			{
				// Copy the bit from buffer to the stream
				if ((((int)buffer[readPosCount]) & (0x1 << (7 - readPosMod))) != 0)
				{
					this.Source[posCount] = (byte)((int)(this.Source[posCount]) | (0x1 << (7 - posMod)));
				}
				else
				{
					this.Source[posCount] = (byte)((int)(this.Source[posCount]) & (0xffffffff - (0x1 << (7 - posMod))));
				}

				// Increment position cursors
				tempPos++;
				if (posMod == 7)
				{
					posMod = 0;
					posCount++;
				}
				else
				{
					posMod++;
				}
				if (readPosMod == 7)
				{
					readPosMod = 0;
					readPosCount++;
				}
				else
				{
					readPosMod++;
				}
			}
			this.Position = tempPos;
		}
	}

    [Serializable]
    public class CancelException : Exception
    {
        public CancelException() : base("Cancelled")
        {

        }
    }

    public static class EventHandlerExtensions
    {
        public static void InvokeSafely<T>(this EventHandler<T> eventHandler,
                           object sender, T eventArgs) where T : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, eventArgs);
            }
        }
    }
}
