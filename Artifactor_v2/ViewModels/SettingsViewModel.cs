using System.Reflection;
using System.Windows.Input;

using Artifactor_v2.Contracts.Services;
using Artifactor_v2.Helpers;
using Artifactor_v2.Services;
using Artifactor_v2.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ApplicationSettings;

namespace Artifactor_v2.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private ElementTheme _elementTheme;
    private string _versionDescription;
    //private readonly LocalSettingsService _localSettingsService;

    ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

    [ObservableProperty]
    private string checkListPath;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }



    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        //_localSettingsService = localSettingsService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        try
        {
            checkListPath = localSettings.Containers["checklist"].Values["checklistPath"] as string;
        }
        catch
        {
            checkListPath = "";
        }
        //_checkListPath = "C:\\Users\\jsheb\\Downloads\\Deloitte_Allianz_IAPT_Checklist_v.1.2.xlsx";
        
        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;
             
            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    [RelayCommand]
    private async void GetCheckListPath()
    {
        //var checkListPathLocal = await Task.Run(() => _localSettingsService.ReadSettingAsync<string>("CheckListPath"));

        // Create a file picker
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

        // Initialize the folder picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        openPicker.FileTypeFilter.Add(".xlsx");
        openPicker.FileTypeFilter.Add("*");

        // Open the picker for the user to pick a file
        var file = await openPicker.PickSingleFileAsync();
        if (file != null)
        {
            CheckListPath = file.Path;
            if (localSettings.Containers.ContainsKey("checklistPath"))
            {
                localSettings.Containers["checklist"].Values["checklistPath"] = CheckListPath;
            }
            else
            {
                localSettings.CreateContainer("checklist", ApplicationDataCreateDisposition.Always);
                localSettings.Containers["checklist"].Values["checklistPath"] = CheckListPath;
            }
            
            
        }
        else
        {
            CheckListPath = "Operation cancelled.";
        }

        await Task.CompletedTask;
    }
}
