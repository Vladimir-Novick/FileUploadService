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
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

namespace SGCombo.Extensions.Utilites
{
    public static class Hybi
    {
        private static Random random = new Random();

        public static ushort HybiLength(byte[] data, ref ushort pos)
        {
            pos = 0;

            try
            {
                byte firstByte = data[pos++];
                byte secondByte = data[pos++];

                int opcode = firstByte & 0x0F;
                int payloadLength = secondByte;

                if (!OpCode(opcode)) return 0;

                bool isMasked = ((secondByte & 128) == 128);
                if (isMasked) payloadLength = secondByte & 0x7F;

                if (payloadLength > 125)
                {
                    if (payloadLength != 126) Utils.Error("PayloadLength");

                    byte[] ba = new byte[2];

                    ba[1] = data[pos++];
                    ba[0] = data[pos++];

                    payloadLength = System.BitConverter.ToUInt16(ba, 0);
                }

                if (isMasked) pos += 4;

                return (ushort)(payloadLength + pos);
            }
            catch
            {
                return 0;
            }
        }

        private static bool OpCode(int Code)
        {
            switch (Code)
            {
                case 0: // Continuation Frame
                    {
                        Utils.Error("Continuation Frame");
                        break;
                    }

                case 1: // Text Frame
                    {
                        return true; // OK
                    }
                case 2: // Binary Frame
                    {
                        Utils.Error("Binary Frame");
                        break;
                    }
                case 8: // Connection Close Frame
                    {
                        return false;
                    }
                case 9: // Ping Frame
                    {
                        Utils.Error("Ping Frame");
                        break;
                    }
                case 10: //Pong Frame
                    {
                        Utils.Error("Pong Frame");
                        break;
                    }
                default:
                    {
                        Utils.Error("Uknown opcode");
                        break;
                    }
            }

            return false;
        }

        public static string HybiDecode(byte[] data, ref bool isMasked)
        {
            ushort pos = 0;
            byte[] ret = null;
            byte firstByte = data[pos++];
            byte secondByte = data[pos++];

            int opcode = firstByte & 0x0F;
            bool Compressed = (firstByte == 193);
            isMasked = ((secondByte & 128) == 128);

            if (!OpCode(opcode)) return "";

            ushort len = HybiLength(data, ref pos);

            if (!isMasked)
            {
                ret = new byte[len - pos];
                Array.Copy(data, pos, ret, 0, len - pos);
            }
            else
            {
                pos -= 4;
                len -= pos;

                List<int> mask = new List<int>();

                for (int i = 0; i < 4; i++)
                {
                    len -= 1;
                    mask.Add(data[pos++]);
                }

                List<int> unmaskedPayload = new List<int>();

                for (int i = pos; i < (pos + len); i++)
                {
                    unmaskedPayload.Add(data[i] ^ mask[(i - pos) % 4]);
                }

                ret = unmaskedPayload.Select(e => (byte)e).ToArray();
            }

            if (Compressed) ret = Utils.Decompress(ret);

            return Utils.FromUTF(ret);
        }

        public static string GenerateKey()
        {
            byte[] rnd = new byte[16];

            random.NextBytes(rnd);

            return System.Convert.ToBase64String(rnd);
        }

        public static byte[] HybiEncode(byte[] message, bool IsCompressed = false, bool Masking = false)
        {
            if (Masking) return HybiMask(message, IsCompressed);

            return HybiPlain(message, IsCompressed);
        }

        private static byte[] HybiPlain(byte[] Payload, bool IsCompressed)
        {
            int Len = 0;
            int Index = 0;
            int Offset = 0;

            if (IsCompressed)
            {
                Offset = 1; // Add zero BFINAL byte
            }

            int LoadLen = Payload.Length + Offset;
            
            Len = LoadLen + 2;
            if (LoadLen > 125) Len += 2;

            byte[] data = new byte[Len];

            if (!IsCompressed)
            {
                data[Index++] = (byte)129; // 1000 0001 FIN and opcode of text
            }
            else
            {
                data[Index++] = (byte)193; // 1100 0001 FIN and opcode of compressed text 
            }

            if (LoadLen > 125)
            {
                data[Index++] = (byte)126;

                byte[] ba = System.BitConverter.GetBytes((UInt16)LoadLen);

                data[Index++] = ba[1];
                data[Index++] = ba[0];
            }
            else
            {
                data[Index++] = (byte)LoadLen;
            }

            Array.Copy(Payload, 0, data, Index, Payload.Length);

            return data;
        }

        private static byte[] HybiMask(byte[] Payload, bool IsCompressed)
        {
            List<int> Frame = new List<int>();

            int Offset = 0;
            const byte Mask = 128;

            if (!IsCompressed)
            {
                Frame.Add(129); // 1000 0001 FIN and opcode of text
            }
            else
            {
                Offset = 1; // Add zero BFINAL byte
                Frame.Add(193); // 1100 0001 FIN and opcode of compressed text 
            }

            int LoadLen = Payload.Length + Offset;

            if (LoadLen > 125)
            {
                Frame.Add(126 + Mask); // 128 for masking

                byte[] ba = System.BitConverter.GetBytes((UInt16)LoadLen);

                Frame.Add(ba[0]);
                Frame.Add(ba[1]);
            }
            else
            {
                Frame.Add(LoadLen + Mask); // 128 for masking
            }

            List<int> mask = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                mask.Add(random.Next(0, 255));
            }

            Frame.AddRange(mask);

            for (int i = 0; i < Payload.Length; i++)
            {
                Frame.Add(Payload[i] ^ mask[i % 4]);
            }

            if(IsCompressed)
            {
                Frame.Add(0 ^ mask[(LoadLen - 1) % 4]); // Add zero BFINAL byte
            }

            return Frame.Select(e => (byte)e).ToArray();
        }
    }

    internal class WebBufffer
    {
        internal bool Mask;
        private ushort dataLength;
        public bool handshakeComplete;

        private StringBuilder dataString;

        public event EventHandler<cMsg> Command;
        public event EventHandler<TextArgs> Header;

        public WebBufffer()
        {
            dataString = new StringBuilder();
        }

        //public bool Append(Peer Peer, BufferData Buffer)
        //{
        //    return Append(Peer.xCon, Buffer);
        //}

        public bool Append(WebsocketHandler wHandler, RawSocket SourceSocket, BufferData Buffer)
        {
            if (wHandler == null) return false;

            if (!handshakeComplete)
            {
                if (wHandler.Socket == null)
                {
                    wHandler.Socket = SourceSocket;
                }

                string sNew = Utils.FromLatin(Buffer.Data, 0, Buffer.Data.Length);

                dataString.Append(sNew);

                if (sNew.EndsWith("\r\n"))
                {
                    string sData = dataString.ToString();

                    if (sData.EndsWith("\r\n\r\n"))
                    {
                        if (Header != null)
                        {
                            Header(this, new TextArgs(sData));
                        }

                        dataString = new StringBuilder();
                    }
                }

                return true;
            }

            if (dataString.Length == 0)
            {
                ushort dummy = 0;
                dataLength = Hybi.HybiLength(Buffer.Data, ref dummy);

                if (dataLength == 0) return false;
            }

            ushort left = 0;
            byte[] newData = null;

            int end = Math.Min(Buffer.Data.Length, dataLength - dataString.Length);

            dataString.Append(Utils.FromLatin(Buffer.Data, 0, end));

            if (dataString.Length != dataLength) return true;

            if (end < Buffer.Data.Length) left = (ushort)(Buffer.Data.Length - end);

            byte[] bData = Utils.ToLatin(dataString.ToString());
            string sRet = Hybi.HybiDecode(bData, ref Mask);

            if (sRet.Length == 0) return false;

            dataString = new StringBuilder();

            if (left > 0)
            {
                newData = new byte[left];
                Array.Copy(Buffer.Data, end, newData, 0, left);
            }

            if ((sRet.Substring(0, 1) != "{") || (sRet.Substring(sRet.Length - 1) != "}"))
            {
                Utils.Error("Invalid JSON: " + sRet);
                return false;
            }

            if (Command != null)
            {
                cMsg Cmd = null;

                try
                {
                    Cmd = new cMsg(sRet);
                }
                catch (Exception ex)
                {
                    Utils.Error("Invalid JSON: " + ex.Message);
                    return false;
                }

                if (Command != null) Command(wHandler, Cmd);
            }

            if (newData != null)
            {
                return Append(wHandler, SourceSocket, new BufferData(newData, newData.Length));
            }

            return true;
        }
    }

    public class Context : WebsocketHandler
    {
        public Context(bool bLocal) : base(bLocal)  { }
        
        private static String Handshake(String secWebSocketKey)
        {
            String ret = secWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] sha1Hash = new SHA1CryptoServiceProvider().ComputeHash(Utils.ToUTF(ret));

            return Convert.ToBase64String(sha1Hash);
        }

        protected override void NewHeader(object sender, TextArgs args)
        {
            string sKey = "";
            var writer = new StringBuilder();
            StringReader Sr = new StringReader(args.Message);

            writer.AppendLine("HTTP/1.1 101 Switching Protocols");

            while (Sr.Peek() > 0)
            {
                string Line = Sr.ReadLine();

                if (Line.StartsWith("Upgrade:"))
                {
                    writer.AppendLine(Line);
                }

                if (Line.StartsWith("Connection:"))
                {
                    writer.AppendLine(Line);
                }

                if (Line.StartsWith("Sec-WebSocket-Key:"))
                {
                    sKey = Line.Substring(18).Trim();
                }

                if (Line.StartsWith("Sec-WebSocket-Extensions:"))
                {
                    if (Line.Contains("permessage-deflate"))
                    {
                        bool DoDeflate = !base.Local;
#if DEBUG
                        DoDeflate = true;
#endif
                        if(DoDeflate)
                        { 
                            base.Encoder = new DataStream();
                            writer.AppendLine("Sec-WebSocket-Extensions: permessage-deflate; client_no_context_takeover; server_no_context_takeover");
                        }
                    }
                }
            }

            if (sKey.Length == 0)
            {
                Close("sKey.Length");
                return;
            }

            writer.AppendLine("Sec-WebSocket-Version: 13");
            writer.AppendLine("Sec-WebSocket-Accept: " + Handshake(sKey));
            writer.AppendLine("");

            string sOut = writer.ToString().Replace(Environment.NewLine, "\r\n");

            if (!base.SendRaw(Utils.ToUTF(sOut)))
            {
                Console.WriteLine("Sendraw failed?");
                throw new Exception("Sendraw failed?");
            }

            WebData.handshakeComplete = true;
        }
    }

    internal class DataStream
    {
        public DataStream() { }

        public byte[] Compress(byte[] buffer)
        {
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                compressor.Write(buffer, 0, buffer.Length);
                compressor.Close();

                return compressStream.ToArray();
            }
        }
    }
}
