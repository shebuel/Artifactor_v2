using System.Text.Json.Serialization;

namespace Artifactor_v2.Models;

/// <summary>
/// A class for a query for checks.
/// </summary>
/// <param name="Checks">Gets the list of returned checks</param>


/// <summary>
/// A simple model for a check.
/// </summary>
/// <param name="CheckCompleted">Sets whether the check is completed</param>
/// <param name="ProofFilePath">Gets the path of proof file for the respective check</param>
/// <param name="Tags">Gets the tags associated with the particular check</param>
/// <param name="TestDescription">Gets the description of the test case</param>
/// <param name="TestId">Gets the test ID of the test case. Will be used as the key</param>
/// <param name="TestName">Get the name of the testcase</param>
/// <param name="TestType">Gets the type of the test case</param>
public class ObservableCheck
{
    [property: JsonPropertyName("testid")]
    public string? TestId
    {
        get; init;
    }
    
    [property: JsonPropertyName("testname")]
    public string? TestName
    {
        get; init;
    }
    [property: JsonPropertyName("testdescription")]
    public string? TestDescription
    {
        get; init;
    }
    [property: JsonPropertyName("testtype")]
    public string TestType
    {
        get; init;
    }
    [property: JsonPropertyName("checkCompleted")]
    public bool CheckCompleted
    {
        get; set;
    }
    [property: JsonPropertyName("proofFilePath")]
    public List<string>? ProofFilePath
    {
        get; set;
    }
    [property: JsonPropertyName("tags")]
    public List<string>? Tags
    {
        get; init;
    }
    [property: JsonPropertyName("status")]
    public string? Status
    {
        get; set;
    }
    [property: JsonPropertyName("comment")]
    public string? Comment
    {
        get; set;
    }
}
