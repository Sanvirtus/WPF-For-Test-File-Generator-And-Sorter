namespace Wpf_For_Test_File_Generator_And_Sorter.Services;

public interface IFileSorterByChunksService
{
    Task SortFileByChunksAsync(string inputFilePath, string outputFilePath, CancellationToken token = default);
}