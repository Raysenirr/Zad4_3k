using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Zad4_3k
{

        internal class Program
        {
            static async Task Main(string[] args)
            {
                string folderPath = args.Length > 0 ? args[0] : @"C:\DIR1";
                bool verbose = true;
                var ct = CancellationToken.None;

                Func<string, int> countStrategy = Count.CountAllWhitespaces;

                // Чтение файла
                FileReadStrategy wholeFile = File_operation.ReadWholeFileAsync;    // Task файл
                FileReadStrategy byLines = File_operation.ReadFileByLinesAsync; //Task строка

                int maxLineTasks = Math.Max(128, Environment.ProcessorCount * 8);

            // Task файл
            Console.WriteLine("Обработка файла");
                var sw = Stopwatch.StartNew();
                int total1 = await Folder.ProcessFolderAsync(
                    folderPath,
                    fileStrategy: wholeFile,
                    countStrategy: countStrategy,
                    maxLineTasks: 0,
                    verbose: verbose,
                    ct: ct);
                sw.Stop();
                Console.WriteLine($"Время выполнения: {sw.ElapsedMilliseconds} мс");

            //Task строка
            Console.WriteLine("Обработка строк");
                sw.Restart();
                int total2 = await Folder.ProcessFolderAsync(
                    folderPath,
                    fileStrategy: byLines,
                    countStrategy: countStrategy,
                    maxLineTasks: maxLineTasks,
                    verbose: verbose,
                    ct: ct);
                sw.Stop();
                Console.WriteLine($"Время выполнения: {sw.ElapsedMilliseconds} мс");
            }
        }
    }