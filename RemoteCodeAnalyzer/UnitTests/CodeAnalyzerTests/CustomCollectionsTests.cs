using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;

namespace UnitTests.CodeAnalyzerTests
{
    [TestClass]
    public class SizeLimitedQueueTests
    {
        private readonly SizeLimitedBlockingQueue<int> queue = new SizeLimitedBlockingQueue<int>(5);

        [TestMethod]
        public void Test1()
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
