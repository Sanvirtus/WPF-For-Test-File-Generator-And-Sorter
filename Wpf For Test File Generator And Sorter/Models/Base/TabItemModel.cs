namespace Wpf_For_Test_File_Generator_And_Sorter.Models.Base;

public class TabItemModel
{
    public string HeaderKey { get; set; }

    public int Index { get; set; }

    public object Content { get; set; }

    public string IndexSuffix => Index > 1 ? $" - {Index}" : "";
}