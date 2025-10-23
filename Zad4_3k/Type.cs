using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zad4_3k
{

    // Делегат стратегии чтения файла (целиком / построчно)
    public delegate Task<int> FileReadStrategy(
        string filePath,
        Func<string, int> countStrategy,
        int maxLineTasks,
        CancellationToken ct);

    // Итог по файлу
    internal record FileResult(
        string File,
        int Spaces,
        double StartMs,
        double EndMs,
        double ElapsedMs);

}
