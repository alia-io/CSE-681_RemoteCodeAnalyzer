using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;

namespace UnitTests
{
    [TestClass]
    public class SizeLimitedQueueTests
    {
        private readonly SizeLimitedQueue<int> queue = new SizeLimitedQueue<int>(5);

        [TestMethod]
        public void TestQueue1()
        {
            Task.Run(Enqueuer);
            Task.Run(Dequeuer);
        }

        private void Enqueuer()
        {
            bool expected = true;
            bool actual;

            for (int i = 0; i < 100000; i++)
            {
                queue.Enqueue(i);
                actual = queue.Count() <= 5;
                Assert.AreEqual(expected, actual);
            }
        }

        private void Dequeuer()
        {
            bool expected = true;
            bool actual;
            for (int i = 0; i < 100000; i++)
            {
                queue.Dequeue();
                actual = queue.Count() < 5;
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
