using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace UciSharp;

[PublicAPI]
public struct Option
{
    public string Name { get; }
    public string Value { get; }
    public string Default { get; }
    public int Min { get; } = -1;
    public int Max { get; } = -1;

    public OptionType Type { get; }

    public string SetCommandString =>
        $"setoption name {{Name}} {GetValueNameString("value", Value)}";

    private string GetValueNameString(string nameString, string? valueString)
    {
        if (string.IsNullOrWhiteSpace(valueString))
        {
            return "";
        }

        return nameString + $" {valueString}";
    }

    internal const string UCI_NAME_REGEX =
        @"^option\s+name\s+(?<name>[\S ]+?)(?=\s*(?:type|default|min|max|$))\s*";

    internal const string UCI_TYPE_REGEX =
        @"(?<=type\s)\S+(?=\s*(?:default|min|max|$))";

    internal const string UCI_DEFAULT_REGEX =
        @"(?<=default\s)\S+(?=\s*(?:min|max|$))";

    internal const string UCI_MIN_REGEX =
        @"(?<=min\s)\S+(?=\s(?:max|$))";

    internal const string UCI_MAX_REGEX =
        @"(?<=max\s)\S+(?<max>\S+)";

    public Option(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public Option(string uciString)
    {
        Match nameCapture = Regex.Match(uciString, UCI_NAME_REGEX);

        if (nameCapture.Groups.TryGetValue("name", out Group? name))
        {
            Name = name.Value;
        }
        else
        {
            throw new ArgumentException("No option name found!");
        }

        Match typeCapture = Regex.Match(uciString, UCI_TYPE_REGEX);


        if (typeCapture.Groups.Count > 0)
        {
            Group type = typeCapture.Groups[0];
            if (Enum.TryParse(type.Value, true, out OptionType value))
            {
                Type = value;
            }
        }

        Match defaultCapture = Regex.Match(uciString, UCI_DEFAULT_REGEX);
        if (defaultCapture.Groups.Count > 0)
        {
            Group defaultValue = defaultCapture.Groups[0];
            Default = defaultValue.Value;
        }

        Match minCapture = Regex.Match(uciString, UCI_MIN_REGEX);

        if (minCapture.Groups.Count > 0)
        {
            Group min = minCapture.Groups[0];
            if (int.TryParse(min.Value, out int value))
            {
                Min = value;
            }
        }

        Match maxCapture = Regex.Match(uciString, UCI_MAX_REGEX);


        if (maxCapture.Groups.Count > 0)
        {
            Group max = maxCapture.Groups[0];
            if (int.TryParse(max.Value, out int value))
            {
                Max = value;
            }
        }
    }

    private void ParseUciOption(string uciString) { }
}

[PublicAPI]
public enum OptionType
{
    Check,
    Spin,
    Combo,
    Button,
    String
}
