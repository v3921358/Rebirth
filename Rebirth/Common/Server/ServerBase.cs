using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Common.Client;
using Common.Log;
using Common.Network;
using Common.Packets;
using Common.Threading;

namespace Common.Server
{
    public class ServerBase<TClient> where TClient : ClientBase
    {
        private readonly string m_name;
        private readonly WvsCenter m_center;
        private readonly Executor m_thread;
        private readonly CAcceptor m_acceptor;

        public bool LogPackets { get; set; } = true;

        //Implement later
        //private bool m_running;

        public string Name => m_name;
        public WvsCenter ParentServer => m_center;

        public ServerBase(string name, int port,WvsCenter parent)
        {
            m_name = name;
            m_center = parent;
            m_thread = new Executor(name);
            m_acceptor = new CAcceptor(port);
            m_acceptor.OnClientAccepted += OnClientAccepted;
        }

        private void OnClientAccepted(Socket socket)
        {
            var client = new CClientSocket(socket);
            var realClient = CreateClient(client);

            Logger.Write(LogLevel.Info, "[{0}] Accepted {1}", Name, client.Host);

            client.OnPacket += (packet) => Enqueue(() => HandlePacket(realClient, packet));
            client.OnDisconnected += () => Enqueue(() => HandleDisconnect(realClient));

            Enqueue(() => client.Initialize(Constants.Version));
        }

        protected virtual void HandlePacket(TClient client, CInPacket packet)
        {
            var buffer = packet.ToArray();
            var opcode = (RecvOps)BitConverter.ToInt16(buffer, 0);

            if (Constants.FilterRecvOpCode(opcode) == false)
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

        protected virtual TClient CreateClient(CClientSocket socket)
        {
            throw new InvalidOperationException();
        }

        public void Start()
        {
            m_thread.Start();
            m_acceptor.StartListen();

            Logger.Write(LogLevel.Debug, "[{0}] listening on port {1}", Name, m_acceptor.Port);
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
