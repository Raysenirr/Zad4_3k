using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zad4_3k
{
    internal static class File_operation
    {            // читать весь файл целиком и применить стратегию подсчёта
            public static async Task<int> ReadWholeFileAsync(
                string filePath,
                Func<string, int> countStrategy,
                int _ ,
                CancellationToken ct)
            {
                if (countStrategy is null) throw new ArgumentNullException(nameof(countStrategy));

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                              bufferSize: 4096, useAsync: true);
                using var reader = new StreamReader(fs);
                string content = await reader.ReadToEndAsync().ConfigureAwait(false);
                ct.ThrowIfCancellationRequested();

                return countStrategy(content);
            }

            //читать построчно, считать для каждой строки
            public static async Task<int> ReadFileByLinesAsync(
                string filePath,
                Func<string, int> countStrategy,
                int maxLineTasks,
                CancellationToken ct)
            {
                if (countStrategy is null) throw new ArgumentNullException(nameof(countStrategy));
                if (maxLineTasks <= 0) maxLineTasks = 128;

                var limiter = new SemaphoreSlim(maxLineTasks);
                var tasks = new List<Task<int>>();

                try
                {
                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                                  bufferSize: 4096, useAsync: true);
                    using var reader = new StreamReader(fs);

                    string? line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        ct.ThrowIfCancellationRequested();
                        string current = line;
                        await limiter.WaitAsync(ct).ConfigureAwait(false);

                        tasks.Add(Task.Run(() =>
                        {
                            try { return countStrategy(current); }
                            finally { limiter.Release(); }
                        }, ct));
                    }

                    var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                    int total = 0;
                    for (int i = 0; i < results.Length; i++) total += results[i];
                    return total;
                }
                finally
                {
                    limiter.Dispose();
                }
            }
        }
    }
