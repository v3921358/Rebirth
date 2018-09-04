using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Common.Client;
using Common.Log;
using Common.Network;
using Common.Threading;

namespace Common.Server
{
    public class ServerBase<TClient> where TClient : ClientBase
    {
        private readonly string m_name;
        private readonly Func<CClientSocket, TClient> m_createFunc;

        private Executor m_thread;
        private CAcceptor m_acceptor;

        public bool LogPackets { get; set; } = true;

        //Implement later
        //private bool m_running;

        public string Name => m_name;

        public ServerBase(string name, int port, Func<CClientSocket, TClient> createClient)
        {
            m_name = name;
            m_createFunc = createClient;

            m_thread = new Executor(name);
            m_acceptor = new CAcceptor(port);
            m_acceptor.OnClientAccepted += OnClientAccepted;
        }

        private void OnClientAccepted(Socket socket)
        {
            var client = new CClientSocket(socket);
            var realClient = m_createFunc(client);

            Logger.Write(LogLevel.Info, "[{0}] Accepted {1}", Name, client.Host);

            client.OnPacket += (p) => OnClientPacket(realClient, p);
            client.OnDisconnected += () => OnClientClosed(realClient);
            client.Initialize(Constants.Version);
        }
        private void OnClientPacket(TClient socket, CInPacket packet)
        {
            Enqueue(() => HandlePacket(socket, packet));
        }
        private void OnClientClosed(TClient socket)
        {
            Enqueue(() => HandleDisconnect(socket));
        }

        protected virtual void HandlePacket(TClient client, CInPacket packet)
        {
            var buffer = packet.ToArray();
            var opcode = (RecvOps)BitConverter.ToInt16(buffer, 0);

            if (FilterRecvOpCode(opcode) == false)
            {
                var name = Enum.GetName(typeof(RecvOps), opcode);
                var str = Constants.GetString(buffer);

                Logger.Write(LogLevel.Info, "Recv [{0}] {1}", name, str);
            }
        }
        protected virtual void HandleDisconnect(TClient client)
        {
            Logger.Write(LogLevel.Info, "[{0}] Disconnected {1}", Name, client.Host);
        }

        protected virtual bool FilterRecvOpCode(RecvOps recvOp)
        {
            switch (recvOp)
            {
                case RecvOps.CP_ClientDumpLog:
                case RecvOps.CP_ExceptionLog:
                    return true;
            }
            return false;
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
