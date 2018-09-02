using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Network;
using Common.Threading;
using Common.Network;

namespace Common.Server
{
    public class ServerBase
    {
        private readonly string m_name;

        private Executor m_thread;
        private TcpAcceptor m_acceptor;

        public string Name => m_name;

        public ServerBase(string name, int port)
        {
            m_name = name;

            m_thread = new Executor(name);
            m_acceptor = new TcpAcceptor(port);
            m_acceptor.OnClientAccepted += OnClientAccepted;
        }

        private void OnClientAccepted(Socket socket)
        {
            var client = new CClientSocket(socket);
            client.OnPacket += (p) => OnClientPacket(client, p);
            client.OnDisconnected += () => OnClientClosed(client);
            client.Receive();

            Console.WriteLine("Accepted {0}", client.Host);
        }

        private void OnClientPacket(CClientSocket socket, CInPacket packet)
        {
            Enqueue(() => HandlePacket(socket,packet));
        }

        private void OnClientClosed(CClientSocket socket)
        {
            Console.WriteLine("Disconnected {0}", socket.Host);
        }

        protected virtual void HandlePacket(CClientSocket socket, CInPacket packet)
        {
            
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
