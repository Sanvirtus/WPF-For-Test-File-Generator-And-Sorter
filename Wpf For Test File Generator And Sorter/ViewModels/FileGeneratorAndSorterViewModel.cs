using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Wpf_For_Test_File_Generator_And_Sorter.Helpers;
using Wpf_For_Test_File_Generator_And_Sorter.Services;
using Wpf_For_Test_File_Generator_And_Sorter.Services.RelayCommand;

namespace Wpf_For_Test_File_Generator_And_Sorter.ViewModels;

public class FileGeneratorAndSorterViewModel : BaseViewModel
{
    private readonly IFileSorterByChunksService _fileSorterByChunksService;
    private readonly IZeroAllocationFileGeneratorService _zeroAllocationFileGeneratorService;

    public FileGeneratorAndSorterViewModel(ISettingsService settingsService,
        IFileSorterByChunksService fileSorterByChunksService,
        IZeroAllocationFileGeneratorService zeroAllocationFileGeneratorService) : base(settingsService)
    {
        _fileSorterByChunksService = fileSorterByChunksService;
        _zeroAllocationFileGeneratorService = zeroAllocationFileGeneratorService;

        Files = new ObservableCollection<string>();
        SortedFiles = new ObservableCollection<string>();

        var generateCommand = new AsyncRelayCommand(GenerateFile, CanGenerate);

        Gen01GbCommand = generateCommand;
        Gen1GbCommand = generateCommand;
        Gen5GbCommand = generateCommand;
        Gen10GbCommand = generateCommand;
        Gen50GbCommand = generateCommand;
        Gen100GbCommand = generateCommand;
        GenCustomGbCommand = generateCommand;
        OpenFolderCommand = new RelayCommand(OpenFolder);
        SortCommand = new AsyncRelayCommand(SortFileByChunks, CanSort);

        RefreshLists();
    }

    public ObservableCollection<string> Files { get; }

    public ObservableCollection<string> SortedFiles { get; }

    public string? SelectedFile
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand Gen01GbCommand { get; }

    public ICommand Gen1GbCommand { get; }

    public ICommand Gen5GbCommand { get; }

    public ICommand Gen10GbCommand { get; }

    public ICommand Gen50GbCommand { get; }

    public ICommand Gen100GbCommand { get; }

    public ICommand GenCustomGbCommand { get; }

    public ICommand OpenFolderCommand { get; }

    public ICommand SortCommand { get; }

    public bool IsGenerating
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    private bool CanGenerate(object? parameter) => !IsGenerating;

    private bool CanSort(object? parameter)
        => !IsGenerating && !string.IsNullOrEmpty(SelectedFile);

    private void RefreshLists()
    {
        RefreshGeneratedFileList();
        RefreshSortedFileList();
    }

    private void RefreshGeneratedFileList()
    {
        Files.Clear();
        var currentDir = Directory.GetCurrentDirectory();
        var files = Directory.GetFiles(currentDir, "*.txt")
            .Select(Path.GetFileName)
            .Where(name => name != null
                           && name.Contains("GB_")
                           && !name.Contains("_Sorted"))
            .OrderByDescending(name => name);

        foreach (var file in files)
        {
            Files.Add(file!);
        }
    }

    private void RefreshSortedFileList()
    {
        SortedFiles.Clear();
        var currentDir = Directory.GetCurrentDirectory();
        var files = Directory.GetFiles(currentDir, "*.txt")
            .Select(Path.GetFileName)
            .Where(name => name != null && name.Contains("_Sorted"))
            .OrderByDescending(name => name);

        foreach (var file in files)
        {
            SortedFiles.Add(file!);
        }
    }

    private void OpenFolder(object? parameter)
    {
        var path = Directory.GetCurrentDirectory();
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    private async Task GenerateFile(object? parameter)
    {
        if (IsGenerating) return;

        IsGenerating = true;

        try
        {
            double? sizeInGb = null;

            if (parameter != null)
            {
                if (parameter is double d)
                    sizeInGb = d;
                else if (parameter is int i)
                    sizeInGb = i;
                else if (parameter is string s && !string.IsNullOrWhiteSpace(s))
                {
                    s = s.Replace(',', '.').Trim();
                    if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double parsed))
                    {
                        sizeInGb = parsed;
                    }
                }
            }

            if (sizeInGb == null || sizeInGb <= 0)
            {
                return;
            }

            var fileName = sizeInGb.GetFileName();
            var targetSizeBytes = (long)(sizeInGb * 1024 * 1024 * 1024);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            await Task.Run(async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                await _zeroAllocationFileGeneratorService.GenerateFileAsync(filePath, targetSizeBytes);

                stopwatch.Stop();
                Console.WriteLine($"- Total Time: {stopwatch.Elapsed.TotalSeconds:F2} sec");
            });

            RefreshLists();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private async Task SortFileByChunks(object? parameter)
    {
        if (!CanSort(null)) return;

        IsGenerating = true;

        try
        {
            if (SelectedFile == null) return;

            var currentDir = Directory.GetCurrentDirectory();
            var inputPath = Path.Combine(currentDir, SelectedFile);

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(SelectedFile);
            var extension = Path.GetExtension(SelectedFile);
            var outputName = $"{fileNameWithoutExt}_SortedByChunks{extension}";
            var outputPath = Path.Combine(currentDir, outputName);

            await Task.Run(async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                await _fileSorterByChunksService.SortFileByChunksAsync(inputPath, outputPath, CancellationToken.None);

                stopwatch.Stop();
                Console.WriteLine($"Sorting by chunks completed in {stopwatch.Elapsed.TotalSeconds:F2} sec");
            });

            RefreshLists();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            IsGenerating = false;
        }
    }
}