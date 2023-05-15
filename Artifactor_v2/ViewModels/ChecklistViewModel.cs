using System.Reflection.Metadata.Ecma335;
using Artifactor_v2.Core.Helpers;
using Artifactor_v2.Helpers;
using Artifactor_v2.Models;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExcelDataReader;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;

namespace Artifactor_v2.ViewModels;

public partial class ChecklistViewModel : ObservableRecipient
{
    public ChecklistViewModel()
    {
        /*PasteCommand = new AsyncRelayCommand(Paste);*/
    }        

    /// <summary>
    /// Gets the current collection of checks
    /// </summary>
    public ObservableGroupedCollection<string, ObservableCheck> ObservableChecks = new();

    private List<ObservableCheck>? _checks;
    private ChecksQueryResponse? ChecksQuery;
    private readonly ObservableCheck check1 = new()
    {
        TestId = "testID",
        TestName = "testName",
        TestDescription = "testDescription",
        TestType = "testType",
        CheckCompleted = true,
        ProofFilePath = new List<string> { "filepath1", "filepath2" },
        Tags = new List<string> { "tag1", "tag2" },
        Status = "Pass"
    };
    private readonly ObservableCheck check2 = new()
    {
        TestId = "testID1",
        TestName = "testName1",
        TestDescription = "testDescription1",
        TestType = "testType",
        CheckCompleted = true,
        ProofFilePath = new List<string> { "filepath1", "filepath2" },
        Tags = new List<string> { "tag1", "tag2" },
        Status = "Pass"
    };



    /// <summary>
    /// Load the checks to dispaly
    /// </summary>
    /// 
    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private Task LoadChecksAsync()
    {
        _checks = new()
        {
            check1,
            check2
        };

        ExcelToJson();
        ChecksQuery = new(_checks);


        ObservableChecks = new ObservableGroupedCollection<string, ObservableCheck>(
            ChecksQuery.Checks
            .GroupBy(static c=> c.TestType));

        //await LoadChecksAsync();

        OnPropertyChanged(nameof(ObservableChecks));
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void MarkCheckCompleted(ObservableCheck ObservableCheck)
    {
        var _index = ObservableChecks.FirstGroupByKey(ObservableCheck.TestType.ToString()).IndexOf(ObservableCheck);
        ObservableChecks.FirstGroupByKey(ObservableCheck.TestType.ToString()).ElementAt(_index).CheckCompleted = true;
        OnPropertyChanged(nameof(ObservableChecks));
    }

    private void ExcelToJson()
    {
        using var stream = File.Open("C:\\Users\\jsheb\\Downloads\\Deloitte_Allianz_IAPT_Checklist_v.1.2.xlsx", FileMode.Open, FileAccess.Read);
        //Had to include to stop the reader from breaking coz not supporting encoding
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);


        using var reader = ExcelReaderFactory.CreateReader(stream);
        // Choose one of either 1 or 2:

        // 1. Use the reader methods
        do
        {

            if (reader.Name == "Web Application")
            {
                while (reader.Read())
                {
                    if (reader.GetString(0) != null && reader.GetString(0) != "TEST CASE ID")
                    {
                        try
                        {
                            var _getCheck = new ObservableCheck()
                            {
                                TestId = reader.GetString(0),
                                TestName = reader.GetString(1),
                                TestDescription = reader.GetString(2) != null ? reader.GetString(2) : "",
                                TestType = reader.GetString(3),
                                CheckCompleted = false,
                                ProofFilePath = new List<string>() { },
                                Tags = new List<string>(reader.GetString(4).Split(',')),
                                Status = "Pass"
                            };
                            _checks.Add(_getCheck);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                    }

                }
            }
        } while (reader.NextResult());


        // 2. Use the AsDataSet extension method
        var result = reader.AsDataSet().Tables["Web Application"];


        //Serialize the Checks list object to json string
        var json = JsonConvert.SerializeObject(_checks, Formatting.Indented);


        // The result of each spreadsheet is in result.Tables


        //var xmloutputFile = File.OpenWrite("C:\\Users\\jsheb\\Downloads\\xmlOutput.txt");
        //result.WriteXml(xmloutputFile);

        //var jsonoutputFile = File.OpenWrite("C:\\Users\\jsheb\\Downloads\\jsonOutput.txt");
        File.WriteAllText(@"C:\Users\jsheb\Downloads\jsonOutput.txt", json);
    }

    /*public IAsyncRelayCommand PasteCommand
    {
    get; private set; }*/

    /*private async Task Paste()
    {
        var _index = 0;
        
        if (checkPaste != null)
        {
            _index = ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).IndexOf(checkPaste);
        }

        var dataPackageView = Clipboard.GetContent();
        if (dataPackageView != null && dataPackageView.Contains("Bitmap"))
        {
            IRandomAccessStreamReference? imageReceived = null;
            imageReceived = await dataPackageView.GetBitmapAsync();
            if (imageReceived != null)
            {
                using var imageStream = await imageReceived.OpenReadAsync();
                *//*WriteableBitmap bitmapImage = new WriteableBitmap(500, 500);
                await bitmapImage.SetSourceAsync(imageStream);

                StorageFile outputFile = new StorageFile.GetFileFromPathAsync("C:\\Users\\jsheb\\Downloads");


                FileStream imageFileStream = File.OpenWrite(OutputFolder + "\\" + checks[0].testID + "_001");
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, imageFileStream);*//*

                var fileCount = 1;


                if (ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).ElementAt(_index).ProofFilePath != null)
                    fileCount = ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).ElementAt(_index).ProofFilePath.Count + 1;




                var fileSave = new FileSavePicker();
                fileSave.FileTypeChoices.Add("Image", new string[] { ".png" });
                fileSave.DefaultFileExtension = ".png";
                fileSave.SuggestedFileName = ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).ElementAt(_index).TestId + "_" + fileCount.ToString();
                fileSave.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Retrieve the window handle (HWND) of the current WinUI 3 window. 
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

                // Initialize the folder picker with the window handle (HWND).
                WinRT.Interop.InitializeWithWindow.Initialize(fileSave, hWnd);

                var storageFile = await fileSave.PickSaveFileAsync();

                ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).ElementAt(_index).ProofFilePath.Add(storageFile.Path);

                //consoleLog.Text = checksSanitized[_index].filePath[0];

                //checksSanitized.GetEnumerator().MoveNext();
                //TODO: Create a null exception

                using var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
                await imageStream.AsStreamForRead().CopyToAsync(stream.AsStreamForWrite());
            }
        }
    }*/

    [RelayCommand]
    private void Complete()
    {
        /*var contentDialog = new ContentDialog
        {
            Title = "Finish",
            Content = "Please fill in all the checks",
            PrimaryButtonText = "Ok",
            CloseButtonText = "Cancel"
        };
        _ = await contentDialog.ShowAsync();

    }*/
}

    
