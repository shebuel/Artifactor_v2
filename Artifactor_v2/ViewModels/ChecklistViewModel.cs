using System.Reflection.Metadata.Ecma335;
using Artifactor_v2.Core.Helpers;
using Artifactor_v2.Models;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExcelDataReader;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using Windows.ApplicationModel.Contacts;

namespace Artifactor_v2.ViewModels;

public partial class ChecklistViewModel : ObservableRecipient
{
    public ChecklistViewModel()
    {
    }

    /// <summary>
    /// Gets the current collection of checks
    /// </summary>
    public ObservableGroupedCollection<string, Check> Checks = new();

    private List<Check> _checks;


    Check check1 = new("testID", "testName", "testDescription", "testType", false, new List<string> { "filepath1", "filepath2" }, new List<string> { "tag1", "tag2" }, "Pass");
    Check check2 = new("testID1", "testName1", "testDescription1", "testType", false, new List<string> { "filepath1", "filepath2" }, new List<string> { "tag1", "tag3" }, "Fail");

    ChecksQueryResponse ChecksQuery;

    /// <summary>
    /// Load the checks to dispaly
    /// </summary>
    /// 
    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    private async Task LoadChecksAsync()
    {
        _checks = new()
        {
            check1,
            check2
        };

        ChecksQuery = new(_checks);

        Checks = new ObservableGroupedCollection<string, Check>(
            ChecksQuery.Checks
            .GroupBy(static c=> c.TestType));

        //await LoadChecksAsync();

        OnPropertyChanged(nameof(Checks));
    }

    [RelayCommand]
    private void MarkCheckCompleted(Check check)
    {
        Checks.FirstGroupByKey(char.ToUpperInvariant(check.TestId[0]).ToString()).Remove(check);
    }

    /*private async Task ExcelToJson()
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
                    if (reader.GetString(0) != null)
                    {
                        _checks.Add(item: new Check()
                        {
                            TestName = reader.GetString(1),
                            TestDescription = reader.GetString(2),
                            TestType = reader.GetString(3),
                            CheckCompleted = false,
                            ProofFilePath = new List<string>() { },
                            Tags = new List<string>(reader.GetString(4).Split(',')),
                            Status = "Pass",
                            TestId = reader.GetString(0)
                        });
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
    }*/
}

    
