using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DiplomWPF.Common.Helpers
{
    class DThreadPool
    {
        private List<Thread> threads = new List<Thread>();
        public void addThread(Thread thread)
        {
            threads.Add(thread);
        }
        public Thread getThread(int id)
        {
            return threads[id];
        }
        public void delThread(Thread thread)
        {
            threads.Remove(thread);
        }
        public void Clear()
        {
            threads.Clear();
        }
        public bool allThreadsCompleted()
        {
            bool flag = false;
            foreach (Thread thread in threads)
            {
                if (ThreadState.Stopped == thread.ThreadState) flag = true;
                else return false;
            }
            return flag;
        }

        public bool isClean()
        {
            return threads.Count == 0;
        }

        public void closeAll()
        {
            foreach (Thread thread in threads)
            {
                thread.Abort();
            }
        }
    }
}
