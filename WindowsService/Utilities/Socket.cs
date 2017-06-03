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
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;

namespace SGCombo.Extensions.Utilites
{
    internal delegate void OnEventDelegate(Socket Socket);

    public class BufferData : EventArgs
    {
        public byte[] Data;

        public BufferData(byte[] Data)
        {
            this.Data = Data;
        }

        public BufferData(byte[] Data, int Length)
        {
            this.Data = new byte[Length];
            Array.Copy(Data, this.Data, Length);
        }
   }

	public class PacketData : EventArgs
	{
		public byte[] Data;
		public byte Index = 0;
		public byte Attribute = 0;
		public bool Corrupt = false;

		public PacketData(byte[] Data)
		{
			this.Data = Data;
		}

		public PacketData(byte Attribute, byte[] Data)
		{
			this.Data = Data;
			this.Attribute = Attribute;
		}

        public PacketData(byte Attribute, byte Index, bool Corrupt)
        {
            this.Index = Index;
            this.Corrupt = Corrupt;
            this.Attribute = Attribute;
        }

		public PacketData(byte Attribute, byte Index, byte[] Data)
		{
			this.Data = Data;
			this.Index = Index;
			this.Attribute = Attribute;
		}
	}

	public class ProgressArgs : EventArgs
	{
		public byte Progress;

		public ProgressArgs(byte Progress)
		{
			this.Progress = Progress;
		}
	}

    public class MsgArgs : EventArgs
    {
        public iMsg Message;

        public MsgArgs(iMsg Message)
        {
            this.Message = Message;
        }
    }

    public class TextArgs : EventArgs
    {
        public string Message;

        public TextArgs(string Message)
        {
            this.Message = Message;
        }
    }

    public class LineArgs : EventArgs
    {
        public LogLine Line;

        public LineArgs(LogLine Line)
        {
            this.Line = Line;
        }
    }

	public class ConnectArgs : EventArgs
	{
		public string Err = "";
		public bool Launch = false;
		public bool Remote = false;
		public bool Started = false;
        public LoginInfo Login = null;
	}

	public class RegisterInfo
	{
		public string key { get; set; }
		public string serial { get; set; }
	}

    public class LoginArgs
    {
        public string password { get; set; }
    }

	public class ConfigArgs
	{
		public int filter;
        public int remote;
		public string password;

        public ConfigArgs(int Remote, int Filter, string Password)
		{
			this.filter = Filter;
			this.remote = Remote;
			this.password = Password;
		}
	}

	public class AuthArgs : EventArgs
	{
		public string Key;
		public string Serial;

        public AuthArgs()
        {
            this.Key = "local";
            this.Serial = "local";
        }

		public AuthArgs(string Serial, string Key)
		{
			this.Key = Key;
			this.Serial = Serial;
		}
	}

    public class SocketArgs : EventArgs
    {
        public SocketInterface Socket = null;

        public SocketArgs(SocketInterface Socket)
        {
            this.Socket = Socket;
        }
    }

    public class iSocketArgs : EventArgs
    {
        public Socket Socket = null;

        public iSocketArgs(Socket Socket)
        {
            this.Socket = Socket;
        }
    }

    public interface SocketInterface 
	{
        void Init();
        void Close(string Reason);
        void Close(string Reason, bool Raise);
        void Send(byte[] buffer, int Length);

        bool isLocal { get; }
        bool IsSecure { get; }
   		bool Connected { get; }
        string Endpoint { get; }

        event EventHandler<TextArgs> Closed;
		event EventHandler<BufferData> Received;
		event EventHandler<SocketArgs> Connection;
	}

    internal class BaseSocket 
    {
        internal Socket Socket;
        internal const int BufferSize = 8192;
        internal byte[] Buffer = new byte[BufferSize];

        private bool Local = true;
        private volatile bool isClosed = false;

        public event EventHandler<TextArgs> Closed;
        public event EventHandler<BufferData> Received;
        public event EventHandler<SocketArgs> Connection;

        public BaseSocket(Socket Socket, bool isLocal)
        {
            this.Socket = Socket;
            this.Local = isLocal;
        }

        ~BaseSocket()
        {
            Close("~RawSocket", false);
        }

        public bool Connected
        {
            get
            {
                if (Socket == null) return false;
                return Socket.Connected;
            }
        }

        public bool isLocal
        {
            get { return Local; }
        }

        public virtual void Close(string Reason, bool Raise)
        {
            if (isClosed) return;

            isClosed = true;

            if (Raise && (Closed != null))
            {
                Closed(this, new TextArgs(Reason));
                Closed = null;
            }

            if (Socket != null)
            {
                if (Socket.Connected)
                {
                    try { Socket.Shutdown(SocketShutdown.Both); }
                    catch { }
                }

                try { Socket.Close(); }
                catch { }

                Socket = null;
            }
        }

        public virtual void Close(string Reason)
        {
            Close(Reason, true);
        }

        public string Endpoint
        {
            get
            {
                if (Socket == null) return "";
                return Socket.RemoteEndPoint.ToString();
            }
        }

        public virtual void Init() { }
        public virtual void Read() { }
        public virtual void Send(byte[] buffer, int Length) { }

        internal void RaiseConnection(SocketArgs Args)
        {
            if (Connection != null) Connection(this, Args);
        }

        internal void HandleRead(int bytesRead)
        {
            if (bytesRead <= 0)
            {
                Close("Remote endpoint disconnected");
                return;
            }

            BufferData dataBuffer = new BufferData(Buffer, bytesRead);

            if (Received != null) Received(this, dataBuffer);

            try
            {
                Read();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (SocketException sex)
            {
                Close(sex.Message);
            }
        }
    }

    public static class Sockets
    {
        public static SocketInterface Create(Socket Socket, Peer Peer, bool isSecure, bool isLocal)
        {
            if (isSecure) 
            {
                return new SslSocket(Socket, Peer, isLocal);
            }
            else
            {
                return new RawSocket(Socket, Peer, isLocal);
            }
        }

        public static void Connect(Peer Peer, string Host, int Port, int Ms, bool isSecure, bool isLocal, EventHandler<SocketAsyncEventArgs> Completed)
        {
            IPAddress IP = Utils.Host2IP(Host);

            if (IP == null)
            {
                Utils.Error("Could not resolve host " + Host);
                return;
            }

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();

            e.RemoteEndPoint = new IPEndPoint(IP, Port);
            e.Completed += Completed;

            Socket Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Socket.NoDelay = true;
            Socket.Blocking = false;
            Socket.ExclusiveAddressUse = true;
            Socket.LingerState = new LingerOption(true, 0);

            e.UserToken = Create(Socket, Peer, isSecure, isLocal);

            if (!Socket.ConnectAsync(e)) Completed(null, e);
        }
    }

    internal class RawSocket : BaseSocket, SocketInterface
	{
        internal Peer CachePeer = null;

        public RawSocket(Socket Socket, Peer Peer, bool isLocal) : base(Socket, isLocal) 
        {
            CachePeer = Peer;
        }

        public bool IsSecure
        {
            get { return false; }
        }

        public bool IsLocal
        {
            get { return base.isLocal; }
        }

        public override void Init()
        {
            base.RaiseConnection(new SocketArgs(this));

            Read();
        }

        public override void Send(byte[] buffer, int Length)
		{
            string sError = "SendAsync";

            if (Socket == null || !Socket.Connected) return;

            try
            {
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();

                e.SetBuffer(buffer, 0, Length);
                e.Completed += Send_Completed;
                e.UserToken = this;

                if (!Socket.SendAsync(e)) Send_Completed(this, e);

                return;
            }
            catch (Exception ex)
            {
                sError += ": " + ex.Message;
            }

            Close(sError);
		}

        void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Close(e.SocketError.ToString());
                return;
            }
        }

        public override void Read()
        {
            if (Socket == null) return;
            if (!Socket.Connected) return;

            try
            {
                Socket.BeginReceive(Buffer, 0, BufferSize, 0, new AsyncCallback(ReadCallback), Socket);
            }
            catch (Exception ex)            
            {                
                if (ex is SocketException|| ex is NullReferenceException || ex is ObjectDisposedException)
                {
                    return;
                }

                throw;
            }
        }

        [System.Diagnostics.DebuggerStepThrough()] 
		private void ReadCallback(IAsyncResult ar)
		{
			int bytesRead = 0;

            Socket Handler = (Socket)ar.AsyncState;

            try
            {
                bytesRead = Handler.EndReceive(ar);
            }
            catch (ObjectDisposedException)
            {
                Close("Disposed");
                return;
            }
            catch (NullReferenceException)
            {
                Close("NullReference");
                return;
            }
            catch (SocketException se)
            {
                Close("ReadCallback: " + se.Message);
                return;
            }

            base.HandleRead(bytesRead);

            Read();
		}
	}

    internal class SslSocket : BaseSocket, SocketInterface
    {
        private SslStream _sslStream;

        private volatile bool isClosed = false;
        private static readonly X509Certificate Certificate = X509Certificate.CreateFromCertFile("exportedcertificate.cer");

        public SslSocket(Socket Socket, Peer Peer, bool isLocal) : base(Socket, isLocal)
        {
            base.Closed += SslSocket_Closed;
        }

        ~SslSocket()
        {
            Close("~SslSocket", false);
        }

        public bool IsSecure
        {
            get { return true; }
        }

        public bool IsLocal
        {
            get { return base.isLocal; }
        }

        void SslSocket_Closed(object sender, TextArgs e)
        {
            Close(e.Message, false);
        }

        public override void Close(string Reason, bool Raise)
        {
            if (isClosed) return;

            isClosed = true;

            base.Close(Reason, Raise);

            if (_sslStream != null)
            {
                _sslStream.Close();
                _sslStream.Dispose();

                _sslStream = null;
            }
        }

        public override void Init()
        {
            if (_sslStream != null) throw new Exception("_sslStream != null");

            _sslStream = new SslStream(new SslStream(new NetworkStream(Socket, true)));

            BeginAuthenticate(EndAuthenticate);
        }

        public override void Send(byte[] buffer, int Length)
        {
            string sError = "BeginWrite";

            if (Socket == null || !Socket.Connected) return;
            if (_sslStream == null || !_sslStream.CanWrite) return;

            try
            {
                _sslStream.BeginWrite(buffer, Length, 0, WriteAsyncCallback, _sslStream);
                return;
            }
            catch (Exception ex)
            {
                sError += ": " + ex.Message;
            }

            Close(sError);
        }

        private void WriteAsyncCallback(IAsyncResult ar)
        {
            string sError = "EndWrite";

            SslStream _stream = ar.AsyncState as SslStream;

            try
            {
                _stream.EndWrite(ar);
                return;
            }
            catch (ObjectDisposedException)
            {
                sError += ": Disposed";
            }
            catch (NullReferenceException)
            {
                sError += ": NullReference";
            }
            catch (SocketException se)
            {
                sError += ": " + se.Message;
            }

            Close(sError);
        }

        public override void Read()
        {
            SslStream tStream = _sslStream;

            if (tStream == null) return;
            if (!tStream.CanRead) return;

            try
            {
                tStream.BeginRead(Buffer, 0, BufferSize, new AsyncCallback(ReadCallback), tStream);
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        private void ReadCallback(IAsyncResult ar)
        {
            int bytesRead = 0;

            SslStream Handler = (SslStream)ar.AsyncState;

            try
            {
                bytesRead = Handler.EndRead(ar);
            }
            catch (ObjectDisposedException)
            {
                Close("Disposed");
                return;
            }
            catch (NullReferenceException)
            {
                Close("NullReference");
                return;
            }
            catch (SocketException se)
            {
                Close("ReadCallback: " + se.Message);
                return;
            }

            base.HandleRead(bytesRead);

            Read();
        }

        private void BeginAuthenticate(AsyncCallback endAuthenticate)
        {
            _sslStream.BeginAuthenticateAsServer(Certificate, endAuthenticate, _sslStream);
        }

        private void EndAuthenticate(IAsyncResult result)
        {
            var tStream = (SslStream)result.AsyncState;

            try
            {
                tStream.EndAuthenticateAsServer(result);
            }
            catch (Exception e)
            {
                Close(e.Message);
                return;
            }

            if (!tStream.CanRead)
            {
                Close("sslStream.CanRead");
                return;
            }

            if (!tStream.IsEncrypted || !tStream.IsAuthenticated)
            {
                Close("Not encrypted");
                return;
            }

            base.Init();
        }
    }

	public class Listener
	{
		internal Socket Socket;

        private bool isLocal = true;
        private bool isSecure = false;

        private volatile bool isClosed = false;

		internal event EventHandler<SocketArgs> Connection;

        private SocketArgs Connect(Socket Socket, Peer Peer, bool isSecure, bool isLocal)
        {
            SocketInterface sI = Sockets.Create(Socket, Peer, isSecure, isLocal);

            sI.Init();

            return new SocketArgs(sI);
        }

		public void Close(string Reason)
		{
            if (isClosed) return;

            isClosed = true;

			if (Socket != null)
			{
				try { Socket.Shutdown(SocketShutdown.Both); }
				catch { }

				try { Socket.Close(); }
				catch { }

				Socket = null;
			}
		}

        ~Listener()
        {
            Close("~Listener");
        }

		public void Listen(int Port, bool isSecure, bool isLocal)
		{
            this.isLocal = isLocal;
            this.isSecure = isSecure;

			IPEndPoint localEndPoint = null;

			if (isLocal)
			{
				localEndPoint = new IPEndPoint(IPAddress.Loopback, Port);
			}
			else
			{
				localEndPoint = new IPEndPoint(IPAddress.Any, Port);
			}

			if(Socket !=  null) Utils.Error("ss");

			// Create a TCP/IP socket.
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			Socket.NoDelay = true;
			Socket.Blocking = false;
			Socket.ExclusiveAddressUse = true;
			Socket.LingerState = new LingerOption(true, 0);

			try
			{
				// Bind the socket to the local endpoint and listen for incoming connections.
				Socket.Bind(localEndPoint);
				Socket.Listen(100);

                Socket.BeginAccept(new AsyncCallback(AcceptCallback), Socket);
			}
			catch (Exception ex)
			{
				Utils.Error("Listen: " + ex.Message);
			}
		}

        private void AcceptCallback(IAsyncResult ar)
		{
            try
            {
                // Get the socket that handles the client request.

                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                Peer pp = new Peer(isLocal);

                if (Connection != null) Connection(this, Connect(handler, pp, isSecure, isLocal));

                return;

            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("AcceptCallback: NullReference");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AcceptCallback: " + ex.Message);
            }
		}

        private bool CanBind(int Port)
        {
            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            bool isAvailable = true;

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == Port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }
	}

    public class Connection : Context
	{
        public string Key = null;

        public volatile bool Device = false;
        //public volatile bool DeviceOwner = false;

        private volatile bool isClosed = false;

        public SafeDict<Connection> Matches = new SafeDict<Connection>();
        public readonly string GUID = System.Guid.NewGuid().ToString().ToLower();

        public Connection(Peer Peer, bool bLocal) : base(bLocal) 
        {
            base.Init(this, Peer);
        }

        ~Connection()
        {
            Close("~Connection", false);
        }

        public void Add(Connection Con)
        {
            if (!Matches.Contains(Con)) Matches.Add(Con.GUID, Con);
        }

        private void CloseMatches(string Reason)
        {
            if (Matches == null) return;

            foreach (Connection Match in Matches.ToList<Connection>())
            {
                if (Match.Matches.Count > 0) Match.Matches.Remove(GUID);
                if (Match.Matches.Count == 0) Match.Close(Reason);
            }

            Matches.Clear();
        }

        public override void Close(string Reason, bool Retry = true)
        {
            if (isClosed) return;

            isClosed = true;

            base.Close(Reason, Retry);

            CloseMatches(Reason);
        }
    }
}
