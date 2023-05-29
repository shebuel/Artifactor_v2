using Artifactor_v2.Models;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExcelDataReader;
using Newtonsoft.Json;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Data;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Artifactor_v2.Core.Contracts.Services;
using Aspose.Cells;
using Style = Microsoft.UI.Xaml.Style;
using Aspose.Cells.Drawing;

namespace Artifactor_v2.ViewModels;

public partial class ChecklistViewModel : ObservableRecipient
{
    public ChecklistViewModel(IFileService fileService)
    {
        //OutputFolder = "C:\\Users\\jsheb\\Documents\\CheckOutput\\";
        _fileService = fileService;
        StatusItemSource = new()
        {
            "Pass",
            "Fail",
            "Not Applicable"
        };

    }

    /// <summary>
    /// Gets the current collection of checks
    /// </summary>
    /// 

    public ObservableGroupedCollection<string, ObservableCheck> ObservableChecks = new();
    public ObservableGroupedCollection<string, ObservableCheck> ObservableChecksMaster = new();
    
    private List<ObservableCheck>? _checks;
    private ChecksQueryResponse? ChecksQuery;
    private readonly IFileService _fileService;
    private const string _defaultChecksFileName = "checklist.json";
    private TestDetails? TestDetailsFromMain {
        get;  set;
    }
    public List<string> StatusItemSource
    {
        get; set;
    }
    public List<string> FiltersItemSource
    {
        get; set;
    }

    

    /// <summary>
    /// Load the checks to dispaly
    /// </summary>
    /// 
    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task LoadChecksAsync()
    {
        //ObservableChecks.CollectionChanged 

        TestDetailsFromMain = WeakReferenceMessenger.Default.Send<TestDetailsRequestMessage>();

        if (TestDetailsFromMain == null) {
            ContentDialog dailog = new ContentDialog()
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Test Details Not Found",
                Content = "Please re-open the application and enter Test Details",
                CloseButtonText = "OK",
                Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                DefaultButton = ContentDialogButton.Close,
                RequestedTheme = ElementTheme.Dark
            };
            await dailog.ShowAsync();
        }
        else if (TestDetailsFromMain.Continue == true) 
        {
           
           var ObservableChecksRead = await Task.Run(() => _fileService.Read<List<ObservableCheck>>(TestDetailsFromMain.OutputFolderPath, _defaultChecksFileName));
           _checks = ObservableChecksRead;
           ChecksQuery = new(ObservableChecksRead);

           ObservableChecks = new ObservableGroupedCollection<string, ObservableCheck>(
               ChecksQuery.Checks
               .GroupBy(static c => c.TestType));

           ObservableChecksMaster = ObservableChecks;
           OnPropertyChanged(nameof(ObservableChecks));
        }
        else
        {

            _checks = new();

            ExcelToJson(TestDetailsFromMain.ApplicationType);
            ChecksQuery = new(_checks);


            ObservableChecks = new ObservableGroupedCollection<string, ObservableCheck>(
                ChecksQuery.Checks
                .GroupBy(static c => c.TestType));

            ObservableChecksMaster = ObservableChecks;

            OnPropertyChanged(nameof(ObservableChecks));
            //return Task.CompletedTask;
        }
       
    }

    
/// <summary>
/// Implements the paste command to show PoC file name in file picker
/// </summary>
/// 
[RelayCommand]
    private async Task PasteAsync(ObservableCheck checkPaste)
    {
        var _index = 0;


        if (checkPaste != null)
        {
            _index = ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).IndexOf(checkPaste);
        }

        var dataPackageView = Clipboard.GetContent();
        
        if (dataPackageView !=null && dataPackageView.Contains("Bitmap"))
        {
            IRandomAccessStreamReference? imageReceived = await dataPackageView.GetBitmapAsync();
            if (imageReceived != null)
            {
                using var imageStream = await imageReceived.OpenReadAsync();
                
                var fileCount = 1;

                if (ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).ElementAt(_index).ProofFilePath != null)
                    fileCount = ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).ElementAt(_index).ProofFilePath.Count + 1;

                var fileSave = new FileSavePicker();
                fileSave.FileTypeChoices.Add("Image", new string[] { ".png" });
                fileSave.DefaultFileExtension = ".png";
                fileSave.SuggestedFileName = ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).ElementAt(_index).TestId + "_" + fileCount.ToString();
                fileSave.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Retrieve the window handle (HWND) of the current WinUI 3 window. 
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

                // Initialize the folder picker with the window handle (HWND).
                WinRT.Interop.InitializeWithWindow.Initialize(fileSave, hWnd);

                var storageFile = await fileSave.PickSaveFileAsync();

                //ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).ElementAt(_index).ProofFilePath.Add(storageFile.Path);

                if(storageFile != null) {

                    using var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
                    await imageStream.AsStreamForRead().CopyToAsync(stream.AsStreamForWrite());


                    ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).RemoveAt(_index);
                    checkPaste.ProofFilePath.Add(storageFile.Path);
                    ObservableChecks.FirstGroupByKey(checkPaste.TestType.ToString()).Insert(_index, checkPaste);

                    //consoleLog.Text = checksSanitized[_index].filePath[0];

                    //checksSanitized.GetEnumerator().MoveNext();
                    //TODO: Create a null exception 
                }
                else
                {
                    Console.WriteLine("Operation Cancelled");
                }   
            }
        }
        else
        {
            ContentDialog dailog = new ContentDialog()
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "No Image Found",
                Content = "Please copy an image into the clipboard and try again",
                CloseButtonText = "OK",
                Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                DefaultButton = ContentDialogButton.Close,
                RequestedTheme = ElementTheme.Dark
            };
            await dailog.ShowAsync();
        }

        //OnPropertyChanged(nameof(ObservableChecks));
    }


    [RelayCommand]
    private async Task MarkCheckCompleted(ObservableCheck observableCheck)
    {
        /*var _index = ObservableChecks.FirstGroupByKey(ObservableCheck.TestType.ToString()).IndexOf(ObservableCheck);
        //ObservableChecks.FirstGroupByKey(ObservableCheck.TestType.ToString()).ElementAt(_index).CheckCompleted = true;
        
        ObservableChecks.FirstGroupByKey(ObservableCheck.TestType.ToString()).RemoveAt(_index);
        ObservableCheck.CheckCompleted = true;
        ObservableChecks.FirstGroupByKey(ObservableCheck.TestType.ToString()).Insert(_index, ObservableCheck);

        //OnPropertyChanged(nameof(ObservableChecks));*/

        //await Task.Run(() => _fileService.Save(OutputFolder, _defaultChecksFileName, ObservableChecks));
        if(observableCheck.CheckCompleted) {
            var _index = _checks.IndexOf(observableCheck);
            _checks.RemoveAt(_index);
            _checks.Insert(_index, observableCheck);
            await Task.Run(() => _fileService.Save(TestDetailsFromMain.OutputFolderPath, _defaultChecksFileName, _checks));
        }
        
    }

    [RelayCommand]
    private async Task Save()
    {
        var _checksSave = new List<ObservableCheck>();
        foreach ( var group in ObservableChecksMaster)
        {
            foreach (var check in group)
            {
                _checksSave.Add(check);
            }
        }
        await Task.Run(() => _fileService.Save(TestDetailsFromMain.OutputFolderPath, _defaultChecksFileName, _checksSave));

    }




    [RelayCommand]
    private async void Complete()
    {
        List<ObservableCheck> _checksList = new List<ObservableCheck>();
        List<ObservableCheck> _completedChecksList = new List<ObservableCheck>();
        var excelFilePath = TestDetailsFromMain.OutputFolderPath + "\\Artifact_Output.xlsx";

        foreach (var group in ObservableChecks)
        {
            _checksList.AddRange(group.ToList());
        }

        var _count = _checksList.Count;

        foreach (var check in _checksList)
        {
            if (check.CheckCompleted)
            {
                _completedChecksList.Add(check);
            }
        }

        

        // Instantiate a Workbook object that represents Excel file.
        Workbook wb = new Workbook();

        // When you create a new workbook, a default "Sheet1" is added to the workbook.
        Worksheet workSheet = wb.Worksheets[0];

        workSheet.Name = "CheckList";
          

        //workSheet.Cells.ImportCustomObjects(_completedChecksList, 0, 0, imp);

        var _rowIndex = 0;
        var _columnOffsetGlobal = 0;
        var _oleindex = 0;

        //Get Image Data
        FileStream fs = File.OpenRead("C:\\Users\\jsheb\\source\\repos\\Artifactor_v2\\Artifactor_v2\\Assets\\scroll-icon.png");
        byte[] imageData = new Byte[fs.Length];
        fs.Read(imageData, 0, imageData.Length);
        fs.Close();

        var style = workSheet.Cells[_rowIndex, 0].GetStyle();
        style.Font.Name = "Verdana";
        style.Font.Size = 12;

        var styleHeader = workSheet.Cells[_rowIndex, 0].GetStyle();
        var styleTitle = workSheet.Cells[_rowIndex, 0].GetStyle();


        //Style for Vulnerability type header
        styleTitle.ForegroundColor = System.Drawing.Color.FromArgb(0x548235);
        styleTitle.Pattern = BackgroundType.Solid;
        styleTitle.Font.Color = System.Drawing.Color.White;
        styleTitle.Font.IsBold = true;
        styleTitle.HorizontalAlignment = TextAlignmentType.Left;
        styleTitle.Font.Name = "Verdana";
        styleTitle.Font.Size = 12;


        //Style for Column title 
        styleHeader.ForegroundColor = System.Drawing.Color.FromArgb(0x548235); ;
        styleHeader.Pattern = BackgroundType.Solid;
        styleHeader.Font.Color = System.Drawing.Color.White;
        styleHeader.Font.IsBold = true;
        styleHeader.Font.CapsType = TextCapsType.All;
        styleHeader.Font.Name = "Verdana";
        styleHeader.Font.Size = 12;

        foreach (var group in ObservableChecks)
        {
            workSheet.Cells.Merge(_rowIndex, 0, 1, 5);
            workSheet.Cells[_rowIndex, 0].PutValue(group.Key);
            workSheet.Cells[_rowIndex, 0].SetStyle(styleTitle);
            _rowIndex++;
            var range = workSheet.Cells.CreateRange(_rowIndex, 0, 1, 5);
            range[0, 0].PutValue("Test ID");
            range[0, 1].PutValue("Test Description");
            range[0, 2].PutValue("Test Status");
            range[0, 3].PutValue("Tester Comments");
            range[0, 4].PutValue("Proof");
            range.SetStyle(styleHeader);
            _rowIndex++;
            foreach (var item in group.ToList())
            {
                if (item.CheckCompleted)
                {   
                    var checkRange = workSheet.Cells.CreateRange(_rowIndex, 0, 1, 5);
                    workSheet.Cells[_rowIndex, 0].PutValue(item.TestId);
                    workSheet.Cells[_rowIndex, 1].PutValue(item.TestDescription);
                    workSheet.Cells[_rowIndex, 2].PutValue(item.Status);
                    workSheet.Cells[_rowIndex, 3].PutValue(item.Comment);
                    if(item.ProofFilePath.Count > 0)
                    {
                        var _columnOffset = 0;
                        foreach(var filePath in item.ProofFilePath)
                        {
                            if (File.Exists(filePath))
                            {
                                fs = File.OpenRead(filePath);
                                byte[] objectData = new Byte[fs.Length];
                                fs.Read(objectData, 0, objectData.Length);
                                fs.Close();

                                workSheet.OleObjects.Add(_rowIndex, 4, 30, 30, imageData);
                                workSheet.OleObjects[_oleindex].ObjectData = objectData;
                                workSheet.OleObjects[_oleindex].FileFormatType = FileFormatType.Ole10Native;
                                workSheet.OleObjects[_oleindex].ObjectSourceFullName = filePath;
                                workSheet.OleObjects[_oleindex].ProgID = "Packager Shell Object";
                                workSheet.OleObjects[_oleindex].Title = item.TestId;
                                workSheet.OleObjects[_oleindex].Left = _columnOffset + 10;

                                Guid gu = new Guid("0003000c-0000-0000-c000-000000000046");
                                workSheet.OleObjects[_oleindex].ClassIdentifier = gu.ToByteArray();
                                //workSheet.OleObjects[_oleindex].ProgID = "Packager Shell Object";
                                //workSheet.OleObjects[_oleindex].SetEmbeddedObject(false, imageData, filePath, true, item.TestId);
                                _oleindex++;
                                _columnOffset += 50;

                            }
                        }

                        if (_columnOffsetGlobal < _columnOffset)
                        {
                            _columnOffsetGlobal = _columnOffset;
                        }
                        workSheet.Cells.SetRowHeight(_rowIndex, 40);        
                    }
                    checkRange.SetStyle(style);
                    _rowIndex++;
                }
            }
            _rowIndex += 3;

        }

        wb.Worksheets[0].AutoFitColumns();
        workSheet.Cells.SetColumnWidth(4, _columnOffsetGlobal + 20);


        // Save the Excel file.
        try 
        {
            wb.Save(excelFilePath);
        }
        catch 
        {
            ContentDialog dailog = new ContentDialog()
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "Artifact Creation Failed",
                Content = "Please check if the following file is open: " + excelFilePath,
                CloseButtonText = "OK",
                Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                DefaultButton = ContentDialogButton.Close,
                RequestedTheme = ElementTheme.Dark
            };
            await dailog.ShowAsync();
        }
        

       
        ContentDialog dialogSuccess = new ContentDialog()
        {
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Title = "Artifact Created",
            Content = "Artifact Document created in following path: " + TestDetailsFromMain.OutputFolderPath,
            CloseButtonText = "OK",
            Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
            DefaultButton = ContentDialogButton.Close,
            RequestedTheme = ElementTheme.Dark
        };
        await dialogSuccess.ShowAsync();

        Console.WriteLine(_count.ToString());



        
        /*var contentDialog = new ContentDialog
        {
            Title = "Finish",
            Content = "Please fill in all the checks",
            PrimaryButtonText = "Ok",
            CloseButtonText = "Cancel"
        };
        _ = await contentDialog.ShowAsync();*/

    }

    private void ExcelToJson(string testType)
    {
        using var stream = File.Open("C:\\Users\\jsheb\\Downloads\\Deloitte_Allianz_IAPT_Checklist_v.1.2.xlsx", FileMode.Open, FileAccess.Read);

        //Had to include to stop the reader from breaking coz not supporting encoding
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);


        using var reader = ExcelReaderFactory.CreateReader(stream);
        // Choose one of either 1 or 2:

        // 1. Use the reader methods
        do
        {

            if (reader.Name == testType)
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
                                Status = "Pass",
                                Comment = ""
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
}

    
