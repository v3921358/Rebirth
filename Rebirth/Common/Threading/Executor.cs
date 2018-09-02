using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Common.Threading
{
    public class Executor : IDisposable
    {
        private readonly ManualResetEvent m_event;
        private readonly ConcurrentQueue<Action> m_queue;
        private readonly Thread m_thread;

        private bool m_started;
        private bool m_running;

        public bool Running => m_running;

        public Executor(string name)
        {
            m_event = new ManualResetEvent(false);
            m_queue = new ConcurrentQueue<Action>();

            m_thread = new Thread(Work);
            m_thread.IsBackground = false;
            m_thread.Name = $"Executor {name}";
            m_thread.Priority = ThreadPriority.AboveNormal;

            m_started = false;
            m_running = true;
        }

        public void Enqueue(Action action)
        {
            m_queue.Enqueue(action);
            m_event.Set();
        }

        public void Start()
        {
            if(m_started)
                throw new InvalidOperationException();

            m_thread.Start();
        }
        public void Stop()
        {
            if (!m_started)
                throw new InvalidOperationException();

            m_running = false;
            m_event.Set();


        }
        public void Join()
        {
            if (!m_started)
                throw new InvalidOperationException();

            m_thread.Join();
        }

        private void Work()
        {
            m_started = true;
            m_running = true;

            while (m_running)
            {
                m_event.Reset();

                while (m_queue.TryDequeue(out var action))
                {
                    action();
                }

                m_event.WaitOne();
            }

            m_running = false;
        }

        public void Dispose()
        {
            m_event.Dispose();
        }
    }
}
