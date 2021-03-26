/////////////////////////////////////////////////////////////////////////////////
///                                                                           ///
///  ReadFile.cs - Fulfills IReadFile operation contracts                     ///
///                                                                           ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019               ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10    ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:              ///
///                 Software Modeling and Analysis, 2021                      ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu          ///
///                                                                           ///
/////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module fulfills the IReadFile operation contracts to read file blocks and
 *   obtain file metadata. File blocks should be requested repeatedly until the end
 *   of the file is reached. The file's metadata contains the line numbers that present
 *   potentially problematic analysis information, which is to be used when rendering
 *   file text to highlight these lines.
 * 
 *   Public Interface
 *   ----------------
 *   ChannelFactory<IReadFile> readFileFactory = new ChannelFactory<IReadFile>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/ReadFile/"));
 *   INavigation filereader = readFileFactory.CreateChannel();
 *   FileBlock block = filereader.ReadBlock((string) user, (string) project, (string) version, (string) filename);
 *   XElement metadata = filereader.FileMetadata((string) user, (string) project, (string) version, (string) filename);
 */

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.ServiceModel;
using RCALibrary;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class ReadFile : IReadFile
    {
        private int blockNumber = 0;
        private long position = 0;

        /* Reads the next 16000 bytes of the file and sends them back */
        public FileBlock ReadBlock(string user, string project, string version, string filename)
        {
            string filepath = ".\\root\\" + user + "\\" + project + "\\" + version + "\\" + filename;
            FileBlock block = new FileBlock(filepath, blockNumber);

            Host.LogNewRequest("Read Block");

            using (FileStream s = new FileStream(filepath, FileMode.Open))
            {
                s.Position = position;

                if (s.Length - s.Position < block.Buffer.Length) // There are less than 16000 bytes left to read
                {
                    block.Length = (int)(s.Length - s.Position);
                    block.LastBlock = true;
                    s.Read(block.Buffer, 0, block.Length);

                    // End of file reached - reset saved data for the next request
                    blockNumber = 0;
                    position = 0;
                }
                else // Read a normal block
                {
                    block.Length = block.Buffer.Length;
                    s.Read(block.Buffer, 0, block.Length);
                    blockNumber++;
                    position = s.Position;
                }
            }

            return block;
        }

        /* Retrieves the list of problematic lines for the file */
        public XElement FileMetadata(string user, string project, string version, string filename)
        {
            string filePath = ".\\root\\" + user + "\\" + project + "\\" + version + "\\metadata.xml";
            XDocument metadata;

            try
            {
                metadata = XDocument.Load(filePath);
                try
                {
                    return metadata.Elements("severity").Elements("analysis").Where(element => element.Attribute("type").Value.Equals(filename)).First();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not retrieve file metadata: {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to open project metadata file: {0}", e.ToString());
            }

            return null;
        }
    }
}
