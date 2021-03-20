using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace CodeAnalyzer
{
    /* Stores central data and controls the flow of execution through the application */
    public class Executive
    {
        /* Input data */
        private readonly int numberOfFiles;
        private readonly string directoryPath;

        /* Queues and saved data for intermediate processing */
        private readonly SizeLimitedBlockingQueue<string> inputFiles;
        private readonly SizeLimitedBlockingQueue<string> parsedFiles;
        private readonly ProgramClassTypeCollection programClassTypes = new ProgramClassTypeCollection();

        /* Output data */
        private readonly ConcurrentOrderedList processedFiles = new ConcurrentOrderedList();

        public Executive(int numberOfFiles, string directoryPath)
        {
            this.numberOfFiles = numberOfFiles;
            this.directoryPath = directoryPath;

            inputFiles = new SizeLimitedBlockingQueue<string>(numberOfFiles);
            parsedFiles = new SizeLimitedBlockingQueue<string>(numberOfFiles);
        }

        /* Enqueues a file name onto the inputFiles queue */
        public void EnqueueInputFile(string filename) => inputFiles.Enqueue(filename);

        /* Creates the main executive objects to perform all major tasks and activities, limiting access to private data members */
        public async void PerformCodeAnalysis()
        {
            // Create temp directory to store intermediate data
            try
            {
                Directory.CreateDirectory(directoryPath + "\\temp");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create temp directory.\n{0}", e.ToString());
                return;
            }

            // Task to dequeue inputFiles, parse them, and enqueue them into parsedFiles queue
            _ = Task.Run(() => 
            {
                for (int i = 0; i < numberOfFiles; i++)
                {
                    string filename = inputFiles.Dequeue();
                    _ = Task.Run(() =>
                    {
                        new TextParser(directoryPath + "\\" + filename, directoryPath + "\\temp\\" + filename + ".txt").ProcessFile();
                        parsedFiles.Enqueue(filename);
                    });
                }
            });

            // Task to dequeue parsedFiles and analyze them, establishing hierarchy of types and collecting function data
            await Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < numberOfFiles; i++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        string filename = parsedFiles.Dequeue();
                        ProgramFile file = new ProgramFile(filename, directoryPath);
                        new FileAnalyzer(file, programClassTypes).ProcessFileCode();
                        processedFiles.Add(file);
                    }));
                }
                Task.WaitAll(tasks.ToArray());
            });


            




            /* 6: Collect relationship data for each class and interface */
            //foreach (ProgramClassType programClassType in this.codeAnalysisData.ProgramClassTypes)
            //    new RelationshipProcessor(programClassType, this.codeAnalysisData.ProgramClassTypes).ProcessRelationships();

            /* 7: Print the requested code analysis data to standard output and/or XML file */
            //outputWriter.WriteOutput(this.codeAnalysisData.ProcessedFiles, this.inputSessionData.DirectoryPath, this.inputSessionData.FileType, this.inputSessionData.PrintToXml, this.inputSessionData.SetRelationshipData);



            // Remove the temp directory
            while (Directory.Exists(directoryPath + "\\temp"))
            {
                try
                {
                    Directory.Delete(directoryPath + "\\temp");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to delete temp directory.\n{0}", e.ToString());
                }
            }
        }
    }
}
