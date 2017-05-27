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
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SGCombo.Extensions.Utilites
{
    public class WebsocketHandler
    {
        public Peer Peer = null;
        public Connection Ctx = null;

        internal bool Local = false;
        internal DataStream Encoder = null;
        internal WebBufffer WebData = null;
        internal SocketInterface Socket = null;
       
        public volatile bool LoggedOut = false;
        private volatile bool isClosed = false;
        private volatile bool SendedQuit = false;

        public event CommandHandler Command;
        public event ConnectionHandler Connection;
        public event EventHandler<CloseArgs> Closed;

        public delegate iMsg CommandHandler(object sender, cMsg args);
        public delegate iMsg ConnectionHandler(object sender, EventArgs args);

        public WebsocketHandler(bool bLocal)
        {
            this.Local = bLocal;
        }

        public virtual void Init(Connection Ctx, Peer Peer)
        {
            if (Peer == null) throw new Exception();

            this.Ctx = Ctx;
            this.Peer = Peer;

            WebData = new WebBufffer();

            WebData.Header += Data_Header;
            WebData.Command += Data_Command;
        }

        ~WebsocketHandler()
        {
            Close("~WebsocketHandler", false);
        }

        public bool Connected
        {
            get
            {
                if (Socket == null) return false;
                return Socket.Connected;
            }
        }

        public virtual void Close(string Reason, bool Retry = true)
        {
            if (isClosed) return;

            isClosed = true;

            if ((Socket != null) && (Socket.Connected))
            {
                // Write("Close socket: " + Reason);

                try
                {
                    Send(new sMsg("logout", Reason));
                }
                catch { }

                Socket.Close(Reason, false);
                Socket = null;
            }

            Raise_Closed(new CloseArgs(Reason, Retry));
        }

        public string Endpoint
        {
            get
            {
                if (Socket == null) return "";
                return Socket.Endpoint;
            }
        }

        private void Data_Command(object sender, cMsg Args)
        {
            iMsg sReturn = null;
            WebsocketHandler Connection = (WebsocketHandler)sender;

            if (Command == null) return;
            if (Connection == null) return;

            try
            {
                sReturn = Command(Connection, Args);
            }
            catch (Exception ex)
            {
                sReturn = new sMsg("error", ex.Message);
            }

            if(Connection != null) Connection.Send(sReturn);
        }

        public void Connect(Socket Socket, Peer Peer, bool isSecure, bool isLocal)
        {
            Start(Sockets.Create(Socket, Peer, isSecure, isLocal));
        }

        public void Start(SocketInterface tSocket)
        {
            this.Socket = tSocket;

            this.Socket.Closed += Raw_Closed;
            this.Socket.Received += Raw_Received;
            this.Socket.Connection += Raw_Connected;

            this.Socket.Init();
        }

        public void Connect(string Host, int Port, int Ms, bool isSecure)
        {
            var xx = this;

            Sockets.Connect((Peer)xx, Host, Port, Ms, isSecure, Local, Completed);
        }

        private void Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Close(e.SocketError.ToString(), true);
                return;
            }

            SocketInterface tSocket = e.UserToken as SocketInterface;

            Start(tSocket);
        }

        private void Raw_Closed(object sender, TextArgs e)
        {
            CloseArgs cA = new CloseArgs(e.Message, true);

            Raise_Closed(cA);
        }

        private void Raise_Closed(CloseArgs cA)
        {
            if (Closed == null) return;

            Closed(this, cA);
            Closed = null;
        }

        private void Raw_Received(object sender, BufferData buffer)
        {
            Connection Ctx = null;
            RawSocket RawSock = (RawSocket)sender;

            if (sender.GetType() == typeof(Peer))
            {
                Ctx = ((Peer)sender).Ctx;
            }
            else if (sender.GetType() == typeof(Connection))
            {
                Ctx = (Connection)sender;
            }
            else if (sender.GetType() == typeof(RawSocket))
            {
                Ctx = (Connection)((RawSocket)sender).CachePeer.Ctx;
            }

            if(Ctx != null)
            {
                try
                {
                    if (WebData.Append(Ctx, RawSock, buffer)) return;
                }
                catch (Exception ex)
                {
                    Close("Raw_Received" + ex.Message);
                    return;
                }
            }

            Close("Connection closed");
            return;
        }

        public void Send(iMsg Payload)
        {
            if (Payload == null) return;

            switch(Payload.type)
            {
                case "logout":
                    {
                        if (SendedQuit) return;
                        if (Payload == null) return;

                        SendedQuit = true;

                        Send(Payload.Serialize());
                        return;
                    }

                case "error":
                case "reboot":
                    {
                        Send(Payload.Serialize());
                        return;
                    }
            }
            
            Send(Payload.Serialize());
        }

        public void Send(string Payload)
        {
            if (Payload == null) return;
            if (Payload.Length == 0) return;

            SendBytes(Utils.ToUTF(Payload));
        }

        internal bool SendRaw(byte[] Buffer)
        {
            return SendRaw(Buffer, Buffer.Length);
        }

        internal bool SendRaw(byte[] Buffer, int Length)
        {
            if (Socket == null) return false;
            if (WebData == null) return false;

            if (!Connected) return false;

            if (Buffer == null) return false;
            if (Length == 0) return false;

            Socket.Send(Buffer, Length);

            return true;
        }

        private void SendBytes(byte[] Buffer)
        {
            int MinCompress = 250;
            bool bCompress = (Encoder != null);

            if (bCompress && (Buffer.Length <= MinCompress)) bCompress = false;

#if DEBUG
            bCompress = (Encoder != null);
#endif

            if (bCompress) Buffer = Encoder.Compress(Buffer);

            byte[] bHybi = Hybi.HybiEncode(Buffer, bCompress);

            if(!SendRaw(bHybi))
            {
                Console.WriteLine("Sendraw failed?");
                throw new Exception("Sendraw failed?");
            }
        }

        private void Raw_Connected(object sender, SocketArgs args)
        {
            this.NewConnection(sender, args);
        }

        private void Data_Header(object sender, TextArgs args)
        {
            this.NewHeader(sender, args);
        }

        internal void RaiseConnection()
        {
            iMsg sData = Connection(this, new EventArgs());
            Connection = null;

            Send(sData);
        }

        protected virtual void NewHeader(object sender, TextArgs args) { }
        protected virtual void NewConnection(object sender, SocketArgs args) { }
    }

    public class Backoff
    {
        public int Max = 20;
        public int Speed = 100;

        private volatile int Count = 0;
        private TimerHelper Retry = null;

        public event EventHandler<TextArgs> TimerEvent;

        public Backoff()
        {
            Retry = new TimerHelper();
            Retry.TimerEvent += Retry_TimerEvent;
        }

        void Retry_TimerEvent(object sender, TextArgs e)
        {
            TimerEvent(sender, e);
        }

        public void Stop()
        {
            if (Retry != null) Retry.Stop();
        }

        public void Reset()
        {
            Count = 0;
        }

        public void Start(string Desc)
        {
            Count += 1;
            if (Count > Max) Count = (Max / 2);

            int Ms = Count * Speed;

            Retry.Start(Ms, Desc);
        }
    }

    public class Peer : WebsocketHandler
    {
        public Backoff Backoff = null;

        public volatile bool Stopped = false;
        public volatile bool LoggedIn = false;
        public volatile bool Registered = false;
        public volatile bool DeviceOwner = false;

        public Peer(bool bLocal) : base(bLocal) 
        {
            var Ctx = new Connection(this, bLocal);

            base.Init(Ctx, this);

            Backoff = new Backoff();
        }

        ~Peer()
        {
            base.Close("~Peer", false);
        }

        public override void Close(string Reason, bool Retry = true)
        {
            LoggedIn = false;
            Registered = false;

            Backoff.Stop();

            base.Close(Reason, Retry);
        }

        protected override void NewHeader(object sender, TextArgs args)
        {
            if (!args.Message.Contains("HTTP/1.1 101"))
            {
                Close("NewHeader");
                return;
            }

            WebData.handshakeComplete = true;

            base.RaiseConnection();
        }

        protected override void NewConnection(object sender, SocketArgs args)
        {
            try
            {
                string request = string.Format(
                    "GET {0} HTTP/1.1" + Environment.NewLine +
                    "Upgrade: WebSocket" + Environment.NewLine +
                    "Connection: Upgrade" + Environment.NewLine +
                    "Host: {1}" + Environment.NewLine +
                    "Origin: {2}" + Environment.NewLine +
                    "Pragma: no-cache" + Environment.NewLine +
                    "Cache-Control: no-cache" + Environment.NewLine +
                    "Accept-Encoding: gzip, deflate" + Environment.NewLine +
                    "Sec-WebSocket-Version: 13" + Environment.NewLine +
                    "Sec-WebSocket-Key: " + Hybi.GenerateKey() + "" + Environment.NewLine +
                    "Sec-WebSocket-Extensions: permessage-deflate; client_no_context_takeover; server_no_context_takeover" +
                    Environment.NewLine + Environment.NewLine, "/", args.Socket.Endpoint, "null");

                if (!this.SendRaw(Utils.ToLatin(request)))
                {
                    Console.WriteLine("Sendraw failed?");
                    throw new Exception("Sendraw failed?");
                }

            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    } 

    public interface Closeable
    {
        void Close(string Reason);
    }

    internal class ContextArgs : EventArgs
    {
        internal Connection Context;

        public ContextArgs(Connection vContext)
        {
            Context = vContext;
        }
    }

    public class CloseArgs : EventArgs
    {
        public bool Retry = false;
        public string Reason = "";

        public CloseArgs(string Text, bool Retry) 
        {
            this.Reason = Text;
            this.Retry = Retry;
        }
    }

    internal class JsonArgs : ContextArgs
    {
        public TextArgs Json;

        public JsonArgs(TextArgs vJson, Connection vContext) : base(vContext)
        {
            Json = vJson;
        }
    }

    internal class Sockets<T>
    {
        private Listener iServer;

        public bool Local = true;
        private volatile bool isClosed = false;

        private SafeDict<List<string>> Keys;
        private SafeDict<Connection> Clients;

        public event ReceivedHandler Received;
        public event ConnectionHandler Connection;
        public event EventHandler<CloseArgs> Closed;
        public event EventHandler<TextArgs> WriteLine;
        public event EventHandler<CloseArgs> Disconnected;

        public delegate iMsg ReceivedHandler(object sender, Peer Peer, cMsg args);
        public delegate Connection ConnectionHandler(object sender, TextArgs args);

        public Sockets(int Port, bool isSecure, bool isLocal)
        {
            Keys = new SafeDict<List<string>>();
            Clients = new SafeDict<Connection>();

            iServer = new Listener();
            iServer.Connection += Connected;

            iServer.Listen(Port, isSecure, isLocal);
        }

        ~Sockets()
        {
            Close("~Server");
        }

        private void Connected(object sender, SocketArgs e)
        {
            if (e == null) return;
            if (Connection == null) return;

            Connection Ctx = Connection(this, new TextArgs("Conntected: " + e.Socket.Endpoint));

            if (Ctx == null) return;

            Ctx.Closed += Socket_Closed;
            Ctx.Command += Data_Command;

            Ctx.Start(e.Socket);
        }

        iMsg Data_Command(object sender, cMsg Args)
        {
            iMsg sReturn = null;
            WebsocketHandler Connection = (WebsocketHandler)sender;

            if (Received == null) return null;
            if (Connection == null) return null;

            try
            {
                sReturn = Received(Connection, Connection.Peer, Args);
            }
            catch (Exception ex)
            {
                string sLine = "Server.Err: " + ex.Message;

                Write(sLine);

                sReturn = new sMsg("error", sLine);
            }

            return sReturn;
        }

        private void Write(string Val)
        {
            if (WriteLine != null) WriteLine(this, new TextArgs(Val));
        }

        private void Socket_Closed(object sender, CloseArgs args)
        {
            Connection Sender = (Connection)sender;

            if(Disconnected != null) Disconnected(Sender, args);

            Remove(Sender, args.Reason, args.Retry);
        }

        public void Close(string Reason)
        {
            if (isClosed) return;

            isClosed = true;

            foreach (Connection Ctx in Clients) // Client first
            {
                Remove(Ctx, "Server is closing: " + Reason, false);
            }
            
            Keys.Clear();
            Clients.Clear();

            Keys = null;
            Clients = null;

            iServer.Close(Reason);
        }

        public bool Add(Connection Connection)
        {
            if (Clients == null) return false;
            if (Contains(Connection)) return false;
            if (Connection.Key.Length == 0) return false;

            List<string> Result = Keys.Get(Connection.Key);
            if (Result == null) Result = new List<string>();

            Result.Add(Connection.GUID);

            if(Result.Count == 1)
            {
                if (!Keys.Addb(Connection.Key, Result)) return false;
            }

            return Clients.Addb(Connection.GUID, Connection);
        }
  
        public List<Connection> Get(string Key, bool Devices)
        {
            if (Keys == null) return null;
            if ((Key == null) || Key.Length == 0) return null;

            List<string> Result = Keys.Get(Key);
            List<Connection> Matches = new List<Connection>();

            if (Result == null) Result = new List<string>();

            foreach (string Guid in Result)
            {
                Connection Match = Clients.Get(Guid);

                if (Match == null) continue;
                if (Match.Device != Devices) continue;

                Matches.Add(Match);
            }

            return Matches;
        }

        public bool Contains(Connection Connection)
        {
            if (Clients == null) return false;
            return Clients.Contains(Connection);
        }
        
        public void Remove(Connection Context, string Reason, bool Retry)
        {
            if (Clients == null) return;
            if (Context == null) return;

            if (!Clients.Contains(Context)) return;

            bool bDebug = false;
#if DEBUG
            bDebug = true;
#endif
#if PROXY
            bDebug = true;
#endif

            if (!Local || bDebug)
            {
                string Desc = "(device)";
                if (!Context.Device) Desc = "(client)";
            
                Write("Logout: " + Context.Endpoint + " - " + Context.Key + " " + Desc + " - " + Reason);
            }

            Clients.Remove(Context.GUID);

            List<string> KeyList = Keys.Get(Context.Key);

            if (KeyList != null)
            {
                if (KeyList.Count == 0) Keys.Remove(Context.Key);
                if (KeyList.Contains(Context.GUID)) KeyList.Remove(Context.GUID);
            }

            if (Closed != null) Closed(Context, new CloseArgs(Reason, Retry));

            Context.Close(Reason);
        }
    }
}
