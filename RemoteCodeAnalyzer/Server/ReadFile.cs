using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using RCALibrary;
using System.ServiceModel.Channels;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class ReadFile : IReadFile
    {
        private int blockNumber = 0;
        private long position = 0;

        public FileBlock ReadBlock(string user, string project, string version, string filename)
        {
            string filepath = ".\\root\\" + user + "\\" + project + "\\" + version + "\\" + filename;
            FileBlock block = new FileBlock(filepath, blockNumber);

            Console.WriteLine("Read Block Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            using (FileStream s = new FileStream(filepath, FileMode.Open))
            {
                s.Position = position;

                if (s.Length - s.Position < block.Buffer.Length)
                {
                    block.Length = (int)(s.Length - s.Position);
                    block.LastBlock = true;
                    blockNumber = 0;
                    position = 0;
                    s.Read(block.Buffer, 0, block.Length);
                }
                else
                {
                    block.Length = block.Buffer.Length;
                    s.Read(block.Buffer, 0, block.Length);
                    blockNumber++;
                    position = s.Position;
                }
            }

            return block;
        }
    }
}
