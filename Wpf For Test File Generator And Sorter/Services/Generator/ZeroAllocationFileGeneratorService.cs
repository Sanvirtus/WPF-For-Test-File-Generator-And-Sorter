using System.Buffers.Text;
using System.IO;
using Wpf_For_Test_File_Generator_And_Sorter.Helpers;

namespace Wpf_For_Test_File_Generator_And_Sorter.Services.Generator;

public class ZeroAllocationFileGeneratorService : IZeroAllocationFileGeneratorService
{
    private const int MaxRandomNumber = int.MaxValue;
    private const int Blocks = 64;
    private const int BytesInBlock = 1024;

    public async Task<bool> GenerateFileAsync(
        string filePath, long targetSizeBytes, CancellationToken cancellationToken = default)
    {
        if (targetSizeBytes <= 0)
            return false;

        var buffer = new byte[Blocks * BytesInBlock];
        var offset = 0;
        long totalWritten = 0;

        try
        {
            await using var fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 0,
                useAsync: true);

            while (totalWritten < targetSizeBytes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (buffer.Length - offset < 256)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, offset), cancellationToken);
                    totalWritten += offset;
                    offset = 0;
                }

                var span = buffer.AsSpan(offset);
                var randomNumber = Random.Shared.Next(1, MaxRandomNumber);

                Utf8Formatter.TryFormat(randomNumber, span, out var bytesWritten);
                offset += bytesWritten;

                WordLibrary.DotSpace.CopyTo(span.Slice(bytesWritten));
                offset += WordLibrary.DotSpace.Length;

                var word = WordLibrary.GetRandomWord();
                word.CopyTo(span.Slice(bytesWritten + WordLibrary.DotSpace.Length));
                offset += word.Length;

                WordLibrary.NewLine.CopyTo(span.Slice(bytesWritten + WordLibrary.DotSpace.Length + word.Length));
                offset += WordLibrary.NewLine.Length;
            }

            if (offset > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, offset), cancellationToken);
            }

            await fileStream.FlushAsync(cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // ignored
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while generating file: {ex.Message}");
            return false;
        }
    }
}