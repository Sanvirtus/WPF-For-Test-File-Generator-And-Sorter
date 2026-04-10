namespace Wpf_For_Test_File_Generator_And_Sorter.Helpers;

public static class FileHelper
{
    public static string GetFileName(this double? targetGb)
    {
        var timestamp = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");

        return $"{targetGb:F1}GB_{timestamp}.txt";
    }
}