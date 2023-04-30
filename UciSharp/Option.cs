using System.Text.RegularExpressions;

namespace UciSharp;

public struct Option
{
    public string Name { get; }
    public string Value { get; }
    public int Min { get; } = -1;
    public int Max { get; } = -1;

    public OptionType Type { get; }

    private string _uciRegex =
        @"^option\s+name\s+(?<name>\S+)\s+(?:type\s+)?(?<type>\S+)\s+(?:min\s+(?<min>\S+)\s+)?(?:max\s+(?<max>\S+)\s+)?(?:default\s+(?<default>\S+)\s+)?(?:var\s+(?<var>\S+)\s+)?";

    public Option(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public Option(string uciString)
    {
        Regex regex = new Regex(_uciRegex);
        var answer = regex.Match(uciString);

        if (answer.Groups.TryGetValue("name", out var name))
        {
            Name = name.Value;
        }
        else
        {
            throw new ArgumentException("No option name found!");
        }

        if (answer.Groups.TryGetValue("type", out var type))
        {
            if(Enum.TryParse<OptionType>(type.Value, true, out var value))
            {
                Type = value;
            }
        }

        if (answer.Groups.TryGetValue("min", out var min))
        {
            if (!String.IsNullOrWhiteSpace(min.Value))
            {
                Min = Int32.Parse(min.Value);
            }
        }

        if (answer.Groups.TryGetValue("max", out var max))
        {
            if (!String.IsNullOrWhiteSpace(max.Value))
            {
                Max = Int32.Parse(max.Value);
            }
        }
    }

    private void ParseUciOption(string uciString) { }
}

public enum OptionType
{
    Check,
    Button,
    Spin,
    String,
    TableBases
}
