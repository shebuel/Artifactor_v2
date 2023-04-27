using System.Text.Json.Serialization;

namespace Artifactor_v2.Models;

/// <summary>
/// A class for a query for checks.
/// </summary>
/// <param name="Checks">Gets the list of returned checks</param>
public sealed record ChecksQueryResponse(IList<Check> Checks);


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


public sealed record Check(
    [property: JsonPropertyName("testid")] string TestId,
    [property: JsonPropertyName("testname")] string TestName,
    [property: JsonPropertyName("testdescription")] string TestDescription,
    [property: JsonPropertyName("testtype")] string TestType,
    [property: JsonPropertyName("checkCompleted")] bool CheckCompleted,
    [property: JsonPropertyName("proofFilePath")] List<string> ProofFilePath,
    [property: JsonPropertyName("tags")] List<string> Tags,
    [property: JsonPropertyName("status")] string Status);

