namespace Wpf_For_Test_File_Generator_And_Sorter.Services.Sorter;

public class StringComparer : IComparer<string>
{
    private const char DotCharacter = '.';

    public int Compare(string? firstStr, string? secondStr)
    {
        if (firstStr == null || secondStr == null) return 0;

        var firstStrDotIndex = GetCharIndex(firstStr, DotCharacter);
        var secondStrDotIndex = GetCharIndex(secondStr, DotCharacter);

        var firstCharSpan = GetCharSpan(firstStr, firstStrDotIndex + 2);
        var secondCharSpan = GetCharSpan(secondStr, secondStrDotIndex + 2);

        var stringComparison = firstCharSpan.CompareTo(secondCharSpan, StringComparison.Ordinal);

        if (stringComparison != 0) return stringComparison;

        var firstNum = int.Parse(firstStr.AsSpan(0, firstStrDotIndex));
        var secondNum = int.Parse(secondStr.AsSpan(0, secondStrDotIndex));

        return firstNum.CompareTo(secondNum);
    }

    private static int GetCharIndex(string line, char value)
    {
        return line.IndexOf(value);
    }

    private static ReadOnlySpan<char> GetCharSpan(string line, int start)
    {
        return line.AsSpan(start);
    }
}