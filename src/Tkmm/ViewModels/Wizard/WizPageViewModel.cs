using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Models.Wizard;

namespace Tkmm.ViewModels.Wizard;

public partial class WizPageViewModel(int id, WizPageViewModel? lastPage, string title, object? pageContent, List<WizAction> actions) : ObservableObject
{
    
    [ObservableProperty]
    private int _id = id;

    public WizPageViewModel? LastPage { get; set; } = lastPage;
    
    [ObservableProperty]
    private string _title = title;
    
    [ObservableProperty]
    private object? _pageContent = pageContent;
    
    [ObservableProperty]
    private List<WizAction> _actions = actions;

    [RelayCommand]
    private void MoveBack()
    {
        if (LastPage is null) {
            return;
        }
        
        WizLayout.UpdatePage(this, LastPage);
    }
}