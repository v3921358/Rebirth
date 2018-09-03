using System;
using System.Net.Sockets;
using Common.Log;
using Common.Network;
using Common.Threading;

namespace Common.Server
{
    public class ServerBase
    {
        private readonly string m_name;

        private Executor m_thread;
        private CAcceptor m_acceptor;
        
        //Implement later
        //private bool m_running;

        public string Name => m_name;

        public ServerBase(string name, int port)
        {
            m_name = name;

            m_thread = new Executor(name);
            m_acceptor = new CAcceptor(port);
            m_acceptor.OnClientAccepted += OnClientAccepted;
        }

        private void OnClientAccepted(Socket socket)
        {
            var client = new CClientSocket(socket);
            client.OnPacket += (p) => OnClientPacket(client, p);
            client.OnDisconnected += () => OnClientClosed(client);
            
            Logger.Write(LogLevel.Info,"[{0}] Accepted {1}", Name, client.Host);

            client.Initialize(Constants.Version);
        }
        private void OnClientPacket(CClientSocket socket, CInPacket packet)
        {
            Enqueue(() => HandlePacket(socket,packet));
        }
        private void OnClientClosed(CClientSocket socket)
        {
            Logger.Write(LogLevel.Info,"[{0}] Disconnected {1}",Name, socket.Host);

            Enqueue(() => HandleDisconnect(socket));
        }

        protected virtual void HandlePacket(CClientSocket socket, CInPacket packet)
        {
            var buffer = packet.ToArray();
            var opcode = (RecvOps) BitConverter.ToInt16(buffer, 0);

            var name = Enum.GetName(typeof(RecvOps), opcode);
            var str = BitConverter.ToString(buffer);

            Logger.Write(LogLevel.Info,"Recv [{0}] {1}", name, str);
        }
        protected virtual void HandleDisconnect(CClientSocket socket) { }

        public void SendPacket(CClientSocket socket, COutPacket packet)
        {
            var buffer = packet.ToArray();
            var opcode = (SendOps)BitConverter.ToInt16(buffer, 0);

            var name = Enum.GetName(typeof(SendOps), opcode);
            var str = BitConverter.ToString(buffer);

            Logger.Write(LogLevel.Info, "Send [{0}] {1}", name, str);

            //L000000L I FORGOT THIS LINE AND WONDERED WHY IT DIDNT WORK ~__~
            socket.Send(packet); 
        }

        public void Start()
        {
            m_thread.Start();
            m_acceptor.StartListen();
        }
        public void Stop()
        {
            m_acceptor.StopListen();
            m_thread.Stop();
            m_thread.Join();
        }

        public void Enqueue(Action action)
        {
            m_thread.Enqueue(action);
        }
    }
}
