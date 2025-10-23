using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zad4_3k
{
    internal class Folder { 
            /// <summary>
            /// файлы в папке парааллельно
            /// </summary>
            /// <param name="folderPath">Путь к папке с файлами</param>
            /// <param name="fileStrategy">Стратегия чтения файла (целиком / построчно)</param>
            /// <param name="countStrategy">Стратегия подсчёта пробелов в строке</param>
            /// <param name="maxLineTasks">Лимит параллельных задач по строкам</param>
            /// <param name="ct">Токен отмены (необязательно)</param>
            /// <returns>Суммарное количество пробелов по всем файлам</returns>
            public static async Task<int> ProcessFolderAsync(
                string folderPath,
                FileReadStrategy fileStrategy,
                Func<string, int> countStrategy,
                int maxLineTasks,
                bool verbose,
                CancellationToken ct = default)
            {
                //Проверяем существование папки
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Папка не существует: {folderPath}");
                    return 0;
                }

                // Собираем список файлов
                var files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                                     .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                     .ToArray();

                if (files.Length == 0)
                {
                    Console.WriteLine(" В папке нет файлов для обработки.");
                    return 0;
                }

                if (verbose)
                    Console.WriteLine($"Найдено файлов: {files.Length}\n");

                //Засекаем начало сценария
                long scenarioStart = Stopwatch.GetTimestamp();
                static double ToMs(long ticks) => ticks * 1000.0 / Stopwatch.Frequency;

                //Создаём задачи для каждого файла
                var tasks = files.Select(async file =>
                {
                    long start = Stopwatch.GetTimestamp();

                    string fileName = Path.GetFileName(file);
                    int spaces = 0;

                    try
                    {
                        spaces = await fileStrategy(
                            file,
                            countStrategy,
                            maxLineTasks,
                            ct
                        ).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"Обработка отменена: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке {fileName}: {ex.Message}");
                    }

                    long end = Stopwatch.GetTimestamp();

                    // Возвращаем детальную информацию о файле
                    return new FileResult(
                        File: file,
                        Spaces: spaces,
                        StartMs: ToMs(start - scenarioStart),
                        EndMs: ToMs(end - scenarioStart),
                        ElapsedMs: ToMs(end - start)
                    );
                });

                // Ожидаем завершения всех задач
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);

                //Подсчёт общего количества пробелов и вывод
                int total = 0;
                foreach (var r in results.OrderBy(r => r.File, StringComparer.OrdinalIgnoreCase))
                {
                    total += r.Spaces;
                if (verbose)
                {
                    Console.WriteLine(
                        $"Файл: {Path.GetFileName(r.File)} — {r.Spaces} пробелов " +
                        $"Старт: {r.StartMs:F1} мс Финиш: {r.EndMs:F1} мс " +
                        $"Длительность: {r.ElapsedMs:F1} мс");
                }
            }

            //Итоговая статистика
            if (verbose)
            {
                Console.WriteLine($"Общее количество пробелов: {total}");

                var maxFile = results.OrderByDescending(x => x.ElapsedMs).First();
                double avg = results.Average(x => x.ElapsedMs);

                Console.WriteLine($"Среднее время на файл: {avg:F1} мс");
                Console.WriteLine($"Самый долгий файл: {Path.GetFileName(maxFile.File)} — {maxFile.ElapsedMs:F1} мс\n");
            }

            return total;
        }
    }
}