using CommunityToolkit.Mvvm.ComponentModel;

namespace App7.Presentation.ViewModels;

/// <summary>Represents one column in the "Columns" popup checkbox list.</summary>
public partial class ColumnVisibilityItem : ObservableObject
{
    /// <summary>Matches the Tag on the DataGrid column (used in code-behind lookup).</summary>
    public string ColumnTag { get; init; } = string.Empty;

    /// <summary>Human-readable label shown in the checkbox.</summary>
    public string DisplayName { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isVisible = true;
}
