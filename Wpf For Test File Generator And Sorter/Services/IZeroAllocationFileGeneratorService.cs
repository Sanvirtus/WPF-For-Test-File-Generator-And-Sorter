namespace Wpf_For_Test_File_Generator_And_Sorter.Services;

public interface IZeroAllocationFileGeneratorService
{
    Task<bool> GenerateFileAsync(string filePath, long targetSizeBytes, CancellationToken cancellationToken = default);
}