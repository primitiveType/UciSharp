using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace UciSharp;

[PublicAPI]
public struct Option
{
    public string Name { get; }
    public string? Value { get; }
    public string? Default { get; }
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
        @"^option\s+name\s+(?<name>[\S ]+?)(?=\s*(?:type|default|min|max|var|$))\s*";

    internal const string UCI_TYPE_REGEX =
        @"(?<=type\s)\S+(?=\s*(?:default|min|max|var|$))";

    internal const string UCI_DEFAULT_REGEX =
        @"(?<=default\s)\S+(?=\s*(?:min|max|var|$))";

    internal const string UCI_MIN_REGEX =
        @"(?<=min\s)\S+(?=\s*(?:max|var|$))";

    internal const string UCI_MAX_REGEX =
        @"(?<=max\s)\S+(?=\s*(?:var|$))";

    internal const string UCI_Value_REGEX =
        @"(?<=var\s)\S+(?<var>\S+)";

    public Option(string name, string? value)
    {
        Name = name;
        Value = value;
    }

    public Option(string uciString)
    {
        Name = GetNameFromUciString(uciString);
        Type = GetTypeFromUciString(uciString);
        Default = GetDefaultFromUciString(uciString);
        Min = GetMinFromUciString(uciString);
        Max = GetMaxFromUciString(uciString);
        Value = GetValueFromUciString(uciString);
    }

    private static string? GetValueFromUciString(string uciString)
    {
        string? valueTemp = null;
        Match varCapture = Regex.Match(uciString, UCI_Value_REGEX);
        if (varCapture.Groups.Count > 0)
        {
            Group defaultValue = varCapture.Groups[0];
            valueTemp = defaultValue.Value;
        }

        return valueTemp;
    }

    private static int GetMinFromUciString(string uciString)
    {
        Match minCapture = Regex.Match(uciString, UCI_MIN_REGEX);

        int minTemp = -1;
        if (minCapture.Groups.Count > 0)
        {
            Group min = minCapture.Groups[0];
            if (int.TryParse(min.Value, out int value))
            {
                minTemp = value;
            }
        }

        return minTemp;
    }
    
    private static int GetMaxFromUciString(string uciString)
    {
        int maxTemp = -1;
        Match maxCapture = Regex.Match(uciString, UCI_MAX_REGEX);
        if (maxCapture.Groups.Count > 0)
        {
            Group max = maxCapture.Groups[0];
            if (int.TryParse(max.Value, out int value))
            {
                maxTemp = value;
            }
        }

        return maxTemp;
    }

    private static string? GetDefaultFromUciString(string uciString)
    {
        string defaultTemp = null;
        Match defaultCapture = Regex.Match(uciString, UCI_DEFAULT_REGEX);
        if (defaultCapture.Groups.Count > 0)
        {
            Group defaultValue = defaultCapture.Groups[0];
            defaultTemp = defaultValue.Value;
        }

        return defaultTemp;
    }

    private static OptionType GetTypeFromUciString(string uciString)
    {
        OptionType tempType = OptionType.Check;
        Match typeCapture = Regex.Match(uciString, UCI_TYPE_REGEX);

        if (typeCapture.Groups.Count > 0)
        {
            Group type = typeCapture.Groups[0];
            if (Enum.TryParse(type.Value, true, out OptionType value))
            {
                tempType = value;
            }
        }

        return tempType;
    }

    private static string GetNameFromUciString(string uciString)
    {
        string namestr;
        Match nameCapture = Regex.Match(uciString, UCI_NAME_REGEX);

        if (nameCapture.Groups.TryGetValue("name", out Group? name))
        {
            namestr = name.Value;
        }
        else
        {
            throw new ArgumentException("No option name found!");
        }

        return namestr;
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
