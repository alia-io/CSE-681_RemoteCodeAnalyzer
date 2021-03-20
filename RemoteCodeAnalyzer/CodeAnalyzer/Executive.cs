using System;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CodeAnalyzer
{
    /* Stores central data and controls the flow of execution through the application */
    public class Executive
    {
        /* Input data */
        private readonly int numberOfFiles;
        private readonly string directoryPath;
        private readonly string project;
        private readonly string version;

        /* Queues and saved data for intermediate processing */
        private readonly SizeLimitedBlockingQueue<string> inputFiles;
        private readonly SizeLimitedBlockingQueue<string> parsedFiles;
        private readonly ProgramClassTypeCollection programClassTypes = new ProgramClassTypeCollection();

        /* Output data */
        private readonly ConcurrentOrderedList processedFiles = new ConcurrentOrderedList();

        public Executive(int numberOfFiles, string directoryPath, string project, string version)
        {
            this.numberOfFiles = numberOfFiles;
            this.directoryPath = directoryPath;
            this.project = project;
            this.version = version;

            inputFiles = new SizeLimitedBlockingQueue<string>(numberOfFiles);
            parsedFiles = new SizeLimitedBlockingQueue<string>(numberOfFiles);
        }

        /* Creates the main executive objects to perform all major tasks and activities, limiting access to private data members */
        public bool PerformCodeAnalysis()
        {
            ThreadPool.SetMaxThreads(24, 24);

            // Create temp directory to store intermediate data
            try
            {
                Directory.CreateDirectory(directoryPath + "\\temp");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create temp directory.\n{0}", e.ToString());
                return false;
            }

            // Task to dequeue inputFiles, parse them, and enqueue them into parsedFiles queue
            _ = ParseInputFiles();

            // Dequeue parsedFiles and analyze them, establishing hierarchy of types and collecting function data
            AnalyzeParsedFiles();

            // Task to analyze relationship data
            Task analyzer = AnalyzeAllRelationships();

            // Task to function analysis XML file
            Task writer = WriteFunctionAnalysis();

            // Write relationship analysis XML file
            analyzer.Wait();
            WriteRelationshipAnalysis();

            writer.Wait(); // Wait for all tasks to complete

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

            return true;
        }

        /* Enqueues files onto the inputFiles queue */
        public async Task EnqueueInputFiles(List<XElement> fileNames)
        {
            await Task.Run(() =>
            {
                foreach (XElement file in fileNames)
                    inputFiles.Enqueue(file.Attribute("name") + "." + file.Attribute("type"));
            });
        }

        /* Dequeues files from inputFiles queue, sends them to TextParser for preprocessing, and enqueues them onto parsedFiles queue */
        private async Task ParseInputFiles()
        {
            await Task.Run(() =>
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
        }

        /* Dequeues files from parsedFiles queue, sends them to FileAnalyzer for processing, and adds them onto processedFiles list */
        private void AnalyzeParsedFiles()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numberOfFiles; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    string filename = parsedFiles.Dequeue();
                    ProgramFile file = new ProgramFile(filename, directoryPath + "\\temp");
                    new FileAnalyzer(file, programClassTypes).ProcessFileCode();
                    processedFiles.Add(file);
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private async Task AnalyzeAllRelationships()
        {
            await Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();
                foreach (ProgramClassType programClassType in programClassTypes)
                {
                    ProgramClassType current = programClassType;
                    tasks.Add(Task.Run(() => new ClassAnalyzer(current, programClassTypes).ProcessRelationships()));
                }
                Task.WaitAll(tasks.ToArray());
            });
        }

        private async Task WriteFunctionAnalysis() =>
            await Task.Run(() => FunctionAnalysisWriter.WriteOutput(processedFiles, directoryPath, project, version));

        private void WriteRelationshipAnalysis() =>
            RelationshipAnalysisWriter.WriteOutput(processedFiles, directoryPath, project, version);
    }
}
