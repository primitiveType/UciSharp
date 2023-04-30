namespace UciSharp;

public class UciOptionsCommand : CommandObserver<IReadOnlyList<Option>>
{
    private Dictionary<string, Option> AvailableOptionsInternal { get; } = new();
    public IReadOnlyList<Option> AvailableOptions => AvailableOptionsInternal.Values.ToList();

    private void UpdateAvailableOptions(string line)
    {
        Option option = new(line);
        AvailableOptionsInternal[option.Name] = option;
    }

    protected override void HandleChessEngineResponse(string stdOutText)
    {
        if (!stdOutText.StartsWith("option"))
        {
            return;
        }

        UpdateAvailableOptions(stdOutText);
        ResolveResponse(AvailableOptions);
    }

    protected override async Task SendCommandAsync()
    {
        await UciBridge.SendCommandAsync("uci");
    }

    public UciOptionsCommand(UciBridge uciBridge) : base(uciBridge) { }
}
