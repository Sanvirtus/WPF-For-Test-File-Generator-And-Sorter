using System.IO;

namespace Wpf_For_Test_File_Generator_And_Sorter.Services.Sorter;

public class FileSorterByChunksService(long maxChunkSizeInBytes, long processorCount) : IFileSorterByChunksService
{
    public async Task SortFileByChunksAsync(
        string inputFilePath, string outputFilePath, CancellationToken token = default)
    {
        var totalMemoryLimit = maxChunkSizeInBytes * processorCount;
        var fileInfo = new FileInfo(inputFilePath);

        if (fileInfo.Length <= totalMemoryLimit)
        {
            Console.WriteLine("[Fast Path] File is small enough. Sorting in memory");
            await SortEntireFileInMemoryAsync(inputFilePath, outputFilePath, token);
            Console.WriteLine("In-Memory sorting complete!");

            return;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "SortFileByChunks_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Console.WriteLine("[Stage 1/2] Splitting and sorting chunks");
            var tempFiles = await Stage1SplitAndSortChunksParallelAsync(inputFilePath, tempDirectory, token);

            Console.WriteLine($"[Stage 2/2] Merging {tempFiles.Count} files");
            await Stage2MergeSortedFilesAsync(tempFiles, outputFilePath, token);

            Console.WriteLine("Sorting complete!");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    private async Task<List<string>> Stage1SplitAndSortChunksParallelAsync(
        string inputFilePath, string tempDirectory, CancellationToken token)
    {
        var tempFiles = new List<string>();
        var tasks = new List<Task>();

        using (var reader = new StreamReader(inputFilePath))
        {
            while (!reader.EndOfStream)
            {
                var lines = new List<string>();
                long currentChunkSize = 0;

                while (currentChunkSize < maxChunkSizeInBytes && !reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(token);

                    if (line == null) break;

                    lines.Add(line);
                    currentChunkSize += line.Length * sizeof(char);
                }

                if (lines.Count == 0) break;

                var linesToSort = lines;
                var chunkIndex = tempFiles.Count;
                var tempPath = Path.Combine(tempDirectory, $"chunk_{chunkIndex}.tmp");
                tempFiles.Add(tempPath);

                tasks.Add(Task.Run(() =>
                {
                    linesToSort.Sort(new StringComparer());
                    File.WriteAllLines(tempPath, linesToSort);
                }, token));

                if (tasks.Count >= processorCount)
                {
                    await Task.WhenAny(tasks);
                    tasks.RemoveAll(task => task.IsCompleted);
                }
            }
        }

        await Task.WhenAll(tasks);
        return tempFiles;
    }

    private async Task Stage2MergeSortedFilesAsync(
        List<string> tempFiles, string outputFilePath, CancellationToken token)
    {
        if (tempFiles.Count == 0) return;

        if (tempFiles.Count == 1)
        {
            File.Move(tempFiles[0], outputFilePath, true);
            return;
        }

        var readers = new List<StreamReader>();
        var queue = new PriorityQueue<StreamReader, string>(new StringComparer());

        try
        {
            foreach (var tempFile in tempFiles)
            {
                var reader = new StreamReader(tempFile);
                readers.Add(reader);

                if (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(token);

                    if (line != null)
                    {
                        queue.Enqueue(reader, line);
                    }
                }
            }

            using (var writer = new StreamWriter(outputFilePath))
            {
                while (queue.Count > 0)
                {
                    if (queue.TryDequeue(out var reader, out var smallestStr))
                    {
                        await writer.WriteLineAsync(smallestStr);

                        if (!reader.EndOfStream)
                        {
                            var nextLine = await reader.ReadLineAsync(token);

                            if (nextLine != null)
                            {
                                queue.Enqueue(reader, nextLine);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            foreach (var reader in readers) reader.Dispose();
        }
    }

    private static async Task SortEntireFileInMemoryAsync(string inputFilePath, string outputFilePath,
        CancellationToken token)
    {
        try
        {
            var allLines = await File.ReadAllLinesAsync(inputFilePath, token);

            Array.Sort(allLines, new StringComparer());

            await File.WriteAllLinesAsync(outputFilePath, allLines, token);
        }
        catch (Exception e)
        {
            Console.WriteLine("Fast Path Error: " + e.Message);
        }
    }
}