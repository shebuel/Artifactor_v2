using System.Collections.ObjectModel;
using Artifactor_v2.Core.Contracts.Services;
using Artifactor_v2.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.AccessCache;
using Windows.Storage;
using Windows.Storage.Pickers;
using Artifactor_v2.Contracts.Services;


namespace Artifactor_v2.ViewModels;


public partial class MainViewModel : ObservableRecipient
{
    
    public ObservableCollection<string> ApplicationType;
    private const string _defaultTestDetailsFileName = "test_details.json";
    private readonly IFileService _fileService;
    private readonly INavigationService _navigationService;



    public MainViewModel(IFileService fileService, INavigationService navigationService)
    {
        ApplicationType = new ObservableCollection<string>()
        {
            "Web Application", "Mobile Application", "Thick Client", "Cloud", "Infra"
        };
        var SelectedApplicationType = ApplicationType.First();

        PickFolderCommand = new AsyncRelayCommand(pickFolder);

        _fileService = fileService;
        _navigationService = navigationService;
        
        
    }



    public TestDetails? testDetails;

    [ObservableProperty]
    private string? _outputFolder;

    [ObservableProperty]
    private string? _testerName;

    [ObservableProperty]
    private string? _applicationName;

    [ObservableProperty]
    private string? _clientName;

    [ObservableProperty]
    private string? _selectedApplicationType;

    public IAsyncRelayCommand PickFolderCommand
    {
        get;
    }

    [RelayCommand]
    private async Task Save()
    {   
        
        testDetails = new TestDetails
        {
            TesterName = TesterName,
            ApplicationName = ApplicationName,
            ClientName = ClientName,
            ApplicationType = SelectedApplicationType,
            OutputFolderPath = OutputFolder
        };

        Console.WriteLine(testDetails.TesterName);

        await Task.Run(() => _fileService.Save(OutputFolder, _defaultTestDetailsFileName, testDetails));

        _navigationService.NavigateTo(typeof(ChecklistViewModel).FullName!);


    }

    private async Task pickFolder()
    {
        // Create a folder picker
        var openPicker = new Windows.Storage.Pickers.FolderPicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

        // Initialize the folder picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your folder picker
        openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
        openPicker.FileTypeFilter.Add("*");

        // Open the picker for the user to pick a folder
        StorageFolder folder = await openPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            OutputFolder = folder.Path;

        }
        else
        {
            OutputFolder = "Operation cancelled. Please try again";
        }



    }
}
