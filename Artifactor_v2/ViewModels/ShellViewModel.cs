using Artifactor_v2.Contracts.Services;
using Artifactor_v2.Views;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;

namespace Artifactor_v2.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;
    private object? _selected;

    [ObservableProperty]
    private bool _isMainEnabled;
    [ObservableProperty]
    private bool _isChecklistEnabled;
    [ObservableProperty]
    private bool _isSettingsEnabled;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        IsMainEnabled = true;
        IsChecklistEnabled = false;
        IsSettingsEnabled = true;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {   
            if(selectedItem.GetType() == typeof(ChecklistPage)) 
            {
                IsMainEnabled = false;
            }
            Selected = selectedItem;
        }
    }
}
