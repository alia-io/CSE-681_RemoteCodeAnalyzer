using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /* Stores all data needed for code analysis and output writing */
    public class CodeAnalysisData
    {
        public List<ProgramFile> ProcessedFiles { get; } // List of file objects with all subtypes
        public ProgramClassTypeCollection ProgramClassTypes { get; } // Collection of all classes and interfaces in all files

        public CodeAnalysisData()
        {
            this.ProcessedFiles = new List<ProgramFile>();
            this.ProgramClassTypes = new ProgramClassTypeCollection();
        }
    }

    /* Stores the directory path, filetype, unprocessed file queue, and optional arguments */
    public class UploadData
    {
        // Queues
        public Queue<ProgramFile> RawTextQueue { get; private set; }


        // 
        public string DirectoryPath { get; private set; }
        public string FileType { get; private set; }

        public UploadData()
        {
            this.RawTextQueue = new Queue<ProgramFile>();
        }

        /* Sets the file type to analyze and the optional settings */
        /*public void SetInputSessionData(string[] input)
        {
            this.DirectoryPath = input[3];

            if (input[0].Equals("/S"))
                this.IncludeSubdirectories = true;

            if (input[1].Equals("/R"))
                this.SetRelationshipData = true;

            if (input[2].Equals("/X"))
                this.PrintToXml = true;

            if (input[4].Equals("*.cs") || input[4].Equals("*.java") || input[4].Equals("*.txt"))
                this.FileType = input[4];
        }*/

        /* Reads all files, creates and enqueues the ProgramFile objects with their raw text data */
        public void EnqueueFiles()
        {
            string[] filePaths;

            if (this.FileType.Equals("*.cs") || this.FileType.Equals("*.java") || this.FileType.Equals("*.txt"))
            {
                //if (this.IncludeSubdirectories)
                    //filePaths = Directory.GetFiles(this.DirectoryPath, this.FileType, SearchOption.AllDirectories);
                //else
                    filePaths = Directory.GetFiles(this.DirectoryPath, this.FileType, SearchOption.TopDirectoryOnly);

                foreach (string filePath in filePaths) // Read and enqueue all files
                {
                    string[] filePathArray = filePath.Split('\\');
                    string fileName = filePathArray[filePathArray.Length - 1];
                    this.RawTextQueue.Enqueue(new ProgramFile(filePath, fileName, File.ReadAllText(filePath)));
                }
            }
        }
    }
}
