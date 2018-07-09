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
using System.Collections.Generic;

namespace SGCombo.Extensions.Utilites
{
    public class LoginInfo
    {
        public string key { get; set; }
        public string guid { get; set; }
        public string serial { get; set; }
    }

	public class Proxy
	{
        private volatile bool isClosed = false;

        private Sockets<Connection> Server;

        public event EventHandler<TextArgs> WriteLine;
        public event EventHandler<ConnectArgs> Restart;

        public Proxy(int Port, bool isSecure, bool isLocal)
		{
			Server = new Sockets<Connection>(Port, isSecure, isLocal);

			Server.Closed += Closed;
            Server.Received += Process;
            Server.WriteLine += Status;
            Server.Connection += Connected;
            Server.Disconnected += Server_Disconnected;           
		}

        void Status(object sender, TextArgs e)
        {
            if(WriteLine != null) WriteLine(sender, e);
        }

        ~Proxy()
        {
            Close("~Proxy");
        }

        private Connection Connected(object sender, TextArgs Args)
		{
            //if (WriteLine != null) WriteLine(this, Args);

            Peer pp = new Peer(Server.Local);

            return new Connection(pp, Server.Local);
		}

        public void Close(string Reason)
        {
            if (isClosed) return;

            isClosed = true;

            if (Server != null)
            {
                Server.Close(Reason);
                Server = null;
            }
        }

		public void Closed(object sender, CloseArgs Args)
		{
            Logout((Connection)sender, Args.Reason, Args.Retry);
		}

        private void RegisterLogin(Connection Ctx)
        {
            bool bDebug = false;
#if DEBUG
            bDebug = true;
#endif
#if PROXY
            bDebug = true;
#endif

            string Desc = "(device)";
            if (!Ctx.Device) Desc = "(client)";

            if (!Server.Local || bDebug)
            {
                if(WriteLine != null) WriteLine(this, new TextArgs("Login: " + Ctx.Endpoint + " - " + Ctx.Key + " " + Desc));
            }
        }

        private iMsg lJson(string Reason)
        {
            return new sMsg("logout", Reason);
        }

        void Server_Disconnected(object sender, CloseArgs e)
        {
            Logout((Connection)sender, e.Reason, e.Retry);
        }

        private void Logout(Connection Con, string Reason, bool Retry)
		{
            if (Con == null) return;
            if (Server == null) return;

            if (Con.LoggedOut) return;

            Con.LoggedOut = true;

            if((Con.Connected))
			{
                try
                {                    
                    Con.Send(lJson(Reason));
                }
                catch { }
			}

            if (Server != null) Server.Remove(Con, Reason, Retry);
		}

        private void Mirror(Connection Source, List<Connection> MatchList, string Json)
		{            
            if (Json == null) return;
            if (MatchList == null) return;

            foreach (Connection Match in MatchList)
            {
                Match.Send(Json);
            }
		}

        private bool StartDevice(LoginInfo Params, Connection Con)
        {
            ConnectArgs args = new ConnectArgs();

            args.Launch = true;
            args.Started = false;

            args.Login = Params;
            args.Remote = !Server.Local;

            if (Restart != null) Restart(Con, args);

            return args.Started;
        }

        private List<Connection> GetMatches(Connection Connection)
        {
            List<Connection> MatchList = new List<Connection>();

            if (Connection == null) return MatchList;
            if (Connection.Matches == null) return MatchList;

            foreach (Connection Match in Connection.Matches)
            {
                if (!Match.Connected) continue;
                if (!Server.Contains(Match)) continue;

                MatchList.Add(Match);
            }

            return MatchList;
        }

        private void Match(Connection sServer, Connection sClient)
        {
            if (sServer != null) sServer.Add(sClient);
            if (sClient != null) sClient.Add(sServer);
        }

        private iMsg ProcessLogin(Connection Ctx, Peer Peer, cMsg Command)
        {
            LoginInfo Params = Command.Read<LoginInfo>();

            if (Server.Local) Params.key = "local";

            if ((Params.key == null) || (Params.key.Length < 1)) return lJson("Invalid key");
            if ((Params.serial == null) || (Params.serial.Length < 1)) return lJson("Invalid serial");
            if ((Params.key == "local") && !Server.Local) return lJson("Not local");

            string cKey = Params.key.Trim();

            List<Connection> MatchList = GetMatches(Ctx);

            if (MatchList == null || MatchList.Count == 0)
            {
                MatchList = (List<Connection>)Server.Get(cKey, true);
                if (MatchList.Count > 0) this.Match(MatchList[MatchList.Count - 1], Ctx);
            }

            if (!Server.Contains(Ctx))
            {
                Ctx.Key = cKey;
                Ctx.Device = false;

                if (Server.Add(Ctx))
                {
                    RegisterLogin(Ctx);
                }                
            }

            if (MatchList.Count == 0)
            {
                if (!Server.Local || !StartDevice(Params, Ctx))
                {
                    return new sMsg("wait", "Recorder is offline");
                }
            }

            Mirror(Ctx, MatchList, Command.Json);

            return new sMsg(Command.type, "Succesfull");                            
        }

        private iMsg ProcessRegister(Connection Ctx, Peer Peer, cMsg Command)
        {
            RegisterInfo Params = Command.Read <RegisterInfo>();

            if (Server.Local) Params.key = "local";

            if ((Params.key == null) || (Params.key.Length < 1)) return lJson("Invalid key");
            if ((Params.serial == null) || (Params.serial.Length < 1)) return lJson("Invalid serial");

            if (Params.key == "local")
            {
                if (!Server.Local) return lJson("Not local");
            }

            string cKey = Params.key.Trim();
            var MatchList = GetMatches(Ctx);

            if (MatchList.Count == 0)
            {
                MatchList = (List<Connection>)Server.Get(cKey, false);

                foreach (Connection Match in MatchList)
                {
                    this.Match(Match, Ctx);
                }
            }

            Ctx.Key = cKey;
            Ctx.Device = true;

            if(Server.Add(Ctx))
            {
                RegisterLogin(Ctx);
            }

            Mirror(Ctx, MatchList, Command.Json);

            return new sMsg(Command.type, "Succesfull");
        } 

        private iMsg Process(object sender, Peer Peer, cMsg Command)
		{
            List<Connection> MatchList = null;
            Connection Ctx = (Connection)sender;

            switch (Command.type)
            {
                case "login":
                    {
                        return ProcessLogin(Ctx, null, Command);
                    }

                case "register":
                    {
                        return ProcessRegister(Ctx, null, Command);
                    } 

                case "logout":
                    {
                        Logout(Ctx, Command.Text, false);
                        break;
                    }

                case "error":
                    {
                        WriteLine(this, new TextArgs("Received error: " + Command.Text));

                        break;
                    }

                default:
                    {
                        if (!Server.Contains(Ctx))
                        {
                            return lJson("Not logged in");
                        }

                        MatchList = GetMatches(Ctx);
                        if (MatchList.Count > 0) break;

                        if (Ctx.Device)
                        {
                            Logout(Ctx, "Client not available", false);
                        }
                        else
                        {
                            Logout(Ctx, "Device not available", false);
                        }

                        return null;
                    }
            }

			//WriteLine("Data: " + Command.Json);

            MatchList = GetMatches(Ctx);

            Mirror(Ctx, MatchList, Command.Json);
            return null;
		}
	}
}
