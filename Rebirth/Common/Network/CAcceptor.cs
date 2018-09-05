using System;
using System.Net;
using System.Net.Sockets;

namespace Common.Network
{
    public class CAcceptor : IDisposable
    {
        private readonly int m_port;
        private readonly IPAddress m_address;
        private Socket m_sock;
        private bool m_active;
        private bool m_disposed;

        public IPAddress Addresss => m_address;
        public int Port => m_port;
        public bool Active => m_active;
        public bool Disposed => m_disposed;

        public event Action<Socket> OnClientAccepted;

        public CAcceptor(IPAddress address, int port)
        {
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), @"Port is out of range");

            m_address = address;
            m_port = port;
            m_active = false;
            m_disposed = false;
        }

        public CAcceptor(int port) : this(IPAddress.Any, port) { }

        public void StartListen(int backlog = 0)
        {
            ThrowIfDisposed();

            if (m_active)
                throw new InvalidOperationException();

            m_sock = CSockHelp.CreateTcpSock();
            m_sock.Bind(new IPEndPoint(m_address, m_port));
            m_sock.Listen(backlog);

            m_active = true;

            BeginAccept();
        }
        public void StopListen()
        {
            ThrowIfDisposed();

            if (!m_active)
                throw new InvalidOperationException();

            m_active = false;
            m_sock.Dispose();
        }

        private void BeginAccept()
        {
            if (!m_disposed && m_active)
                m_sock.BeginAccept(EndAccept, null);
        }
        private void EndAccept(IAsyncResult iar)
        {
            try
            {
                var socket = m_sock.EndAccept(iar);
                OnClientAccepted?.Invoke(socket);
            }
            catch (ObjectDisposedException) { }
            catch (SocketException) { }
            finally
            {
                BeginAccept();
            }
        }

        protected void ThrowIfDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException(GetType().Name);
        }

        public virtual void Dispose()
        {
            if (!m_disposed)
            {
                if (m_active)
                    StopListen();

                m_disposed = true;
            }
        }
    }
}