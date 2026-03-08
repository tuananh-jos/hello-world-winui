using App7.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App7.Presentation.ViewModels;

public partial class BorrowDialogViewModel : ObservableObject
{
    // ── Model info (set before opening dialog) ────────────────────────
    public Guid   ModelId   { get; private set; }
    public string ModelName { get; private set; } = string.Empty;

    // ── Hardcoded fields (per spec) ───────────────────────────────────
    public string Inventory { get; } = "PM P/Technology Strategy G";
    public string Manage    { get; } = "thithe.ha";
    public string Address   { get; } = "2F phòng Device";

    // ── Number combobox ───────────────────────────────────────────────
    public System.Collections.ObjectModel.ObservableCollection<int> QuantityOptions { get; } = new();

    [ObservableProperty]
    private int _selectedQuantity = 1;

    // ── Result ────────────────────────────────────────────────────────
    /// <summary>True when user pressed OK, false for Cancel/X.</summary>
    public bool Confirmed { get; private set; }

    // ── Initialise ────────────────────────────────────────────────────
    public void Init(Model model)
    {
        ModelId   = model.Id;
        ModelName = model.Name;
        Confirmed = false;
        QuantityOptions.Clear();
        int maxQuantity = Math.Min(5, model.Available);
        for (int i = 1; i <= maxQuantity; i++)
        {
            QuantityOptions.Add(i);
        }

        SelectedQuantity = 1;
    }

    // ── Commands (called from code-behind after dialog closes) ────────
    public void Confirm()  { Confirmed = true; }
    public void Cancel()   { Confirmed = false; }

    public string? ErrorMessage { get; private set; }
    public void SetError(string message) { ErrorMessage = message; }
}
