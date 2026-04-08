// Copyright (c) DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DemaConsulting.BuildMark.Configuration;

/// <summary>
///     Reads the optional BuildMark configuration file.
/// </summary>
public static class BuildMarkConfigReader
{
    /// <summary>
    ///     Reads and parses the optional configuration file.
    /// </summary>
    /// <param name="path">A repository root directory or a configuration file path.</param>
    /// <returns>The configuration load result.</returns>
    public static async Task<ConfigurationLoadResult> ReadAsync(string path)
    {
        // Resolve the input path to the expected .buildmark.yaml file.
        var filePath = ResolveFilePath(path);
        if (!File.Exists(filePath))
        {
            return new ConfigurationLoadResult(null, []);
        }

        // Read the configuration text before parsing.
        var rawLines = await File.ReadAllLinesAsync(filePath).ConfigureAwait(false);
        var lines = CreateLines(rawLines);
        var issues = new List<ConfigurationIssue>();
        var config = ParseFile(filePath, lines, issues);

        // Return null config whenever parsing produced one or more errors.
        return issues.Any(issue => issue.Severity == ConfigurationIssueSeverity.Error)
            ? new ConfigurationLoadResult(null, issues)
            : new ConfigurationLoadResult(config, issues);
    }

    /// <summary>
    ///     Resolves a repository root or file path to the configuration file.
    /// </summary>
    /// <param name="path">A repository root or file path.</param>
    /// <returns>The resolved configuration file path.</returns>
    private static string ResolveFilePath(string path)
    {
        // Accept either a direct file path or a repository root.
        return Directory.Exists(path)
            ? Path.Combine(path, ".buildmark.yaml")
            : path;
    }

    /// <summary>
    ///     Parses the raw file lines into logical YAML lines.
    /// </summary>
    /// <param name="rawLines">The raw file lines.</param>
    /// <returns>The logical YAML lines.</returns>
    private static List<YamlLine> CreateLines(IReadOnlyList<string> rawLines)
    {
        // Keep only non-empty, non-comment lines while retaining source line numbers.
        var lines = new List<YamlLine>();
        for (var index = 0; index < rawLines.Count; index++)
        {
            var line = StripInlineComment(rawLines[index]);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var indent = 0;
            while (indent < line.Length && line[indent] == ' ')
            {
                indent++;
            }

            lines.Add(new YamlLine(index + 1, indent, line[indent..].TrimEnd()));
        }

        return lines;
    }

    /// <summary>
    ///     Strips an inline comment from a YAML line.
    /// </summary>
    /// <param name="line">The raw line.</param>
    /// <returns>The uncommented content.</returns>
    private static string StripInlineComment(string line)
    {
        // Honor quoted strings before treating # as a comment delimiter.
        var quote = '\0';
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (quote == '\0' && (character == '"' || character == '\''))
            {
                quote = character;
                continue;
            }

            if (quote != '\0' && character == quote)
            {
                quote = '\0';
                continue;
            }

            if (quote == '\0' && character == '#')
            {
                return line[..index].TrimEnd();
            }
        }

        return line;
    }

    /// <summary>
    ///     Parses the full configuration file.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="lines">The logical lines.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed configuration.</returns>
    private static BuildMarkConfig ParseFile(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        List<ConfigurationIssue> issues)
    {
        // Parse each supported top-level section in order.
        var index = 0;
        var config = new BuildMarkConfig();
        ConnectorConfig? connector = null;

        while (index < lines.Count)
        {
            var line = lines[index];
            if (line.Indent != 0)
            {
                AddError(issues, filePath, line.Number, "Top-level keys must start at column 1.");
                index++;
                continue;
            }

            var (key, value) = SplitKeyValue(line);
            switch (key)
            {
                case "connector":
                    connector = ParseConnector(filePath, lines, ref index, issues);
                    continue;

                case "sections":
                    ParseSections(filePath, lines, ref index, config.Sections, issues);
                    continue;

                case "rules":
                    ParseRules(filePath, lines, ref index, config.Rules, issues);
                    continue;

                default:
                    AddError(issues, filePath, line.Number, $"Unsupported top-level key '{key}'.");
                    if (!string.IsNullOrEmpty(value))
                    {
                        index++;
                    }
                    else
                    {
                        SkipBlock(lines, ref index, line.Indent);
                    }
                    continue;
            }
        }

        // Return the assembled configuration object.
        return config with { Connector = connector };
    }

    /// <summary>
    ///     Parses the connector block.
    /// </summary>
    private static ConnectorConfig ParseConnector(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        ref int index,
        List<ConfigurationIssue> issues)
    {
        // Parse connector-level properties and nested connector settings.
        var connector = new ConnectorConfig();
        GitHubConnectorConfig? gitHub = null;
        AzureDevOpsConnectorConfig? azureDevOps = null;
        var parentIndent = lines[index].Indent;
        index++;

        while (index < lines.Count && lines[index].Indent > parentIndent)
        {
            var line = lines[index];
            if (line.Indent != parentIndent + 2)
            {
                AddError(issues, filePath, line.Number, "Connector properties must be indented by two spaces.");
                index++;
                continue;
            }

            var (key, value) = SplitKeyValue(line);
            switch (key)
            {
                case "type":
                    connector = connector with { Type = Unquote(value) };
                    index++;
                    break;

                case "github":
                    gitHub = ParseGitHubConnector(filePath, lines, ref index, issues);
                    break;

                case "azure-devops":
                    azureDevOps = ParseAzureDevOpsConnector(lines, ref index);
                    break;

                default:
                    AddError(issues, filePath, line.Number, $"Unsupported connector key '{key}'.");
                    if (!string.IsNullOrEmpty(value))
                    {
                        index++;
                    }
                    else
                    {
                        SkipBlock(lines, ref index, line.Indent);
                    }
                    break;
            }
        }

        return connector with { GitHub = gitHub, AzureDevOps = azureDevOps };
    }

    /// <summary>
    ///     Parses the GitHub connector block.
    /// </summary>
    private static GitHubConnectorConfig ParseGitHubConnector(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        ref int index,
        List<ConfigurationIssue> issues)
    {
        // Parse GitHub-specific settings.
        string? owner = null;
        string? repo = null;
        string? baseUrl = null;
        var parentIndent = lines[index].Indent;
        index++;

        while (index < lines.Count && lines[index].Indent > parentIndent)
        {
            var line = lines[index];
            if (line.Indent != parentIndent + 2)
            {
                AddError(issues, filePath, line.Number, "GitHub connector properties must be indented by two spaces.");
                index++;
                continue;
            }

            var (key, value) = SplitKeyValue(line);
            var parsedValue = Unquote(value);
            switch (key)
            {
                case "owner":
                    owner = parsedValue;
                    index++;
                    break;

                case "repo":
                    repo = parsedValue;
                    index++;
                    break;

                case "repository":
                    ApplyRepositoryOverride(parsedValue, ref owner, ref repo, filePath, line.Number, issues);
                    index++;
                    break;

                case "base-url":
                case "baseurl":
                case "url":
                    baseUrl = parsedValue;
                    index++;
                    break;

                default:
                    AddError(issues, filePath, line.Number, $"Unsupported GitHub connector key '{key}'.");
                    index++;
                    break;
            }
        }

        return new GitHubConnectorConfig { Owner = owner, Repo = repo, BaseUrl = baseUrl };
    }

    /// <summary>
    ///     Applies a combined repository override.
    /// </summary>
    private static void ApplyRepositoryOverride(
        string repository,
        ref string? owner,
        ref string? repo,
        string filePath,
        int lineNumber,
        List<ConfigurationIssue> issues)
    {
        // Split repository strings of the form owner/repo.
        var parts = repository.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            AddError(issues, filePath, lineNumber, "GitHub repository must be in 'owner/repo' form.");
            return;
        }

        owner = parts[0];
        repo = parts[1];
    }

    /// <summary>
    ///     Parses the Azure DevOps connector block.
    /// </summary>
    private static AzureDevOpsConnectorConfig ParseAzureDevOpsConnector(
        IReadOnlyList<YamlLine> lines,
        ref int index)
    {
        // Skip the placeholder block while preserving the shape of the configuration.
        var parentIndent = lines[index].Indent;
        index++;
        while (index < lines.Count && lines[index].Indent > parentIndent)
        {
            index++;
        }

        return new AzureDevOpsConnectorConfig();
    }

    /// <summary>
    ///     Parses the sections block.
    /// </summary>
    private static void ParseSections(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        ref int index,
        List<SectionConfig> sections,
        List<ConfigurationIssue> issues)
    {
        // Parse each section sequence entry.
        var parentIndent = lines[index].Indent;
        index++;

        while (index < lines.Count && lines[index].Indent > parentIndent)
        {
            var line = lines[index];
            if (line.Indent != parentIndent + 2 || !line.Text.StartsWith("- ", StringComparison.Ordinal))
            {
                AddError(issues, filePath, line.Number, "Sections must be YAML sequence entries.");
                index++;
                continue;
            }

            var section = ParseSectionItem(filePath, lines, ref index, issues);
            if (!string.IsNullOrEmpty(section.Id) && !string.IsNullOrEmpty(section.Title))
            {
                sections.Add(section);
            }
        }
    }

    /// <summary>
    ///     Parses one section item.
    /// </summary>
    private static SectionConfig ParseSectionItem(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        ref int index,
        List<ConfigurationIssue> issues)
    {
        // Parse id/title values for one section.
        string? id = null;
        string? title = null;
        var line = lines[index];
        var itemIndent = line.Indent;
        ParseInlineItem(line.Text[2..], ref id, ref title);
        index++;

        while (index < lines.Count && lines[index].Indent > itemIndent)
        {
            var child = lines[index];
            if (child.Indent != itemIndent + 2)
            {
                AddError(issues, filePath, child.Number, "Section properties must be indented by two spaces.");
                index++;
                continue;
            }

            var (key, value) = SplitKeyValue(child);
            switch (key)
            {
                case "id":
                    id = Unquote(value);
                    break;

                case "title":
                    title = Unquote(value);
                    break;

                default:
                    AddError(issues, filePath, child.Number, $"Unsupported section key '{key}'.");
                    break;
            }

            index++;
        }

        if (string.IsNullOrEmpty(id))
        {
            AddError(issues, filePath, line.Number, "Section entries require an id.");
        }

        if (string.IsNullOrEmpty(title))
        {
            AddError(issues, filePath, line.Number, "Section entries require a title.");
        }

        return new SectionConfig { Id = id ?? string.Empty, Title = title ?? string.Empty };
    }

    /// <summary>
    ///     Parses the rules block.
    /// </summary>
    private static void ParseRules(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        ref int index,
        List<RuleConfig> rules,
        List<ConfigurationIssue> issues)
    {
        // Parse each routing rule sequence entry.
        var parentIndent = lines[index].Indent;
        index++;

        while (index < lines.Count && lines[index].Indent > parentIndent)
        {
            var line = lines[index];
            if (line.Indent != parentIndent + 2 || !line.Text.StartsWith("- ", StringComparison.Ordinal))
            {
                AddError(issues, filePath, line.Number, "Rules must be YAML sequence entries.");
                index++;
                continue;
            }

            rules.Add(ParseRuleItem(filePath, lines, ref index, issues));
        }
    }

    /// <summary>
    ///     Parses one routing rule item.
    /// </summary>
    private static RuleConfig ParseRuleItem(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        ref int index,
        List<ConfigurationIssue> issues)
    {
        // Parse one rule and any nested match block.
        RuleMatchConfig? match = null;
        string? route = null;
        var line = lines[index];
        var itemIndent = line.Indent;
        var inline = line.Text[2..].Trim();
        var originalIndex = index;
        if (!string.IsNullOrEmpty(inline))
        {
            ParseRuleProperty(filePath, lines, ref index, issues, inline, itemIndent, ref match, ref route, consumeCurrentLine: false);
        }
        if (index == originalIndex)
        {
            index++;
        }
        while (index < lines.Count && lines[index].Indent > itemIndent)
        {
            ParseRuleProperty(filePath, lines, ref index, issues, lines[index].Text, itemIndent, ref match, ref route, consumeCurrentLine: true);
        }

        return new RuleConfig { Match = match, Route = route };
    }

    /// <summary>
    ///     Parses one rule property.
    /// </summary>
    private static void ParseRuleProperty(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        ref int index,
        List<ConfigurationIssue> issues,
        string text,
        int itemIndent,
        ref RuleMatchConfig? match,
        ref string? route,
        bool consumeCurrentLine)
    {
        // Parse route or nested match values.
        var currentLine = lines[index];
        if (consumeCurrentLine && currentLine.Indent != itemIndent + 2)
        {
            AddError(issues, filePath, currentLine.Number, "Rule properties must be indented by two spaces.");
            index++;
            return;
        }

        var (key, value) = SplitKeyValue(text);
        switch (key)
        {
            case "route":
                route = Unquote(value);
                if (consumeCurrentLine)
                {
                    index++;
                }
                break;

            case "match":
                match = ParseRuleMatch(filePath, lines, ref index, issues, consumeCurrentLine ? currentLine.Indent : itemIndent + 2);
                break;

            default:
                AddError(issues, filePath, currentLine.Number, $"Unsupported rule key '{key}'.");
                if (consumeCurrentLine)
                {
                    index++;
                }
                break;
        }
    }

    /// <summary>
    ///     Parses a nested rule match block.
    /// </summary>
    private static RuleMatchConfig ParseRuleMatch(
        string filePath,
        IReadOnlyList<YamlLine> lines,
        ref int index,
        List<ConfigurationIssue> issues,
        int matchIndent)
    {
        // Parse label and work-item-type lists from the match block.
        var match = new RuleMatchConfig();
        var parentIndent = matchIndent;
        index++;

        while (index < lines.Count && lines[index].Indent > parentIndent)
        {
            var line = lines[index];
            if (line.Indent != matchIndent + 2)
            {
                AddError(issues, filePath, line.Number, "Match properties must be indented by two spaces.");
                index++;
                continue;
            }

            var (key, value) = SplitKeyValue(line);
            switch (key)
            {
                case "label":
                    match.Label.AddRange(ParseStringList(value));
                    break;

                case "work-item-type":
                    match.WorkItemType.AddRange(ParseStringList(value));
                    break;

                default:
                    AddError(issues, filePath, line.Number, $"Unsupported match key '{key}'.");
                    break;
            }

            index++;
        }

        return match;
    }

    /// <summary>
    ///     Parses an inline section item fragment.
    /// </summary>
    private static void ParseInlineItem(string text, ref string? id, ref string? title)
    {
        // Support the common "- id: value" inline form.
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var line = new YamlLine(0, 0, text);
        var (key, value) = SplitKeyValue(line);
        if (key == "id")
        {
            id = Unquote(value);
        }
        else if (key == "title")
        {
            title = Unquote(value);
        }
    }

    /// <summary>
    ///     Parses a scalar-or-array string list.
    /// </summary>
    /// <param name="value">The YAML value.</param>
    /// <returns>The parsed strings.</returns>
    private static IEnumerable<string> ParseStringList(string value)
    {
        // Accept either a scalar or a simple inline array.
        var trimmed = Unquote(value);
        if (!trimmed.StartsWith('[', StringComparison.Ordinal) || !trimmed.EndsWith(']', StringComparison.Ordinal))
        {
            return string.IsNullOrWhiteSpace(trimmed) ? [] : [trimmed];
        }

        return trimmed[1..^1]
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(Unquote)
            .Where(entry => !string.IsNullOrWhiteSpace(entry));
    }

    /// <summary>
    ///     Splits a YAML key/value line.
    /// </summary>
    private static (string Key, string Value) SplitKeyValue(YamlLine line)
    {
        return SplitKeyValue(line.Text);
    }

    /// <summary>
    ///     Splits a YAML key/value string.
    /// </summary>
    private static (string Key, string Value) SplitKeyValue(string text)
    {
        // Split on the first colon only.
        var separatorIndex = text.IndexOf(':');
        if (separatorIndex < 0)
        {
            return (text.Trim(), string.Empty);
        }

        return (text[..separatorIndex].Trim(), text[(separatorIndex + 1)..].Trim());
    }

    /// <summary>
    ///     Skips a nested YAML block.
    /// </summary>
    private static void SkipBlock(IReadOnlyList<YamlLine> lines, ref int index, int parentIndent)
    {
        // Advance beyond the current line and any deeper-indented children.
        index++;
        while (index < lines.Count && lines[index].Indent > parentIndent)
        {
            index++;
        }
    }

    /// <summary>
    ///     Removes surrounding quotes from a scalar.
    /// </summary>
    private static string Unquote(string value)
    {
        // Remove matching single or double quotes when present.
        var trimmed = value.Trim();
        return trimmed.Length >= 2 &&
               ((trimmed[0] == '"' && trimmed[^1] == '"') || (trimmed[0] == '\'' && trimmed[^1] == '\''))
            ? trimmed[1..^1]
            : trimmed;
    }

    /// <summary>
    ///     Adds an error issue to the issue list.
    /// </summary>
    private static void AddError(List<ConfigurationIssue> issues, string filePath, int lineNumber, string description)
    {
        issues.Add(new ConfigurationIssue(filePath, lineNumber, ConfigurationIssueSeverity.Error, description));
    }

    /// <summary>
    ///     Represents one parsed YAML line.
    /// </summary>
    /// <param name="Number">The source line number.</param>
    /// <param name="Indent">The indentation depth.</param>
    /// <param name="Text">The trimmed YAML text.</param>
    private sealed record YamlLine(int Number, int Indent, string Text);
}
