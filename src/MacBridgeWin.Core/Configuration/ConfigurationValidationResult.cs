namespace MacBridgeWin.Core.Configuration;

public sealed class ConfigurationValidationResult
{
    private readonly List<string> _errors = [];

    public IReadOnlyList<string> Errors => _errors;

    public bool IsValid => _errors.Count == 0;

    public void AddError(string error)
    {
        _errors.Add(error);
    }
}
