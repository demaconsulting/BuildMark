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

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

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

        // Read the file content and parse with YamlDotNet.
        var text = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        List<ConfigurationIssue> issues = [];

        YamlDocument document;
        try
        {
            var stream = new YamlStream();
            using var reader = new StringReader(text);
            stream.Load(reader);

            // An empty or comment-only file produces no documents.
            if (stream.Documents.Count == 0)
            {
                return new ConfigurationLoadResult(new BuildMarkConfig(), []);
            }

            document = stream.Documents[0];
        }
        catch (YamlException ex)
        {
            // Convert YamlDotNet parse errors into ConfigurationIssue records.
            var line = (int)(ex.Start.Line + 1);
            AddError(issues, filePath, line > 0 ? line : 1, ex.InnerException?.Message ?? ex.Message);
            return new ConfigurationLoadResult(null, issues);
        }

        var config = ParseDocument(filePath, document, issues);

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
    ///     Parses the YAML document root into a <see cref="BuildMarkConfig" />.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="document">The parsed YAML document.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed configuration.</returns>
    private static BuildMarkConfig ParseDocument(
        string filePath,
        YamlDocument document,
        List<ConfigurationIssue> issues)
    {
        // The root must be a mapping node.
        if (document.RootNode is not YamlMappingNode root)
        {
            AddError(issues, filePath, GetLine(document.RootNode), "Configuration file must be a YAML mapping.");
            return new BuildMarkConfig();
        }

        var config = new BuildMarkConfig();
        ConnectorConfig? connector = null;
        ReportConfig? report = null;

        foreach (var entry in root.Children)
        {
            var key = GetScalarValue(entry.Key);
            switch (key)
            {
                case "connector":
                    connector = ParseConnector(filePath, entry.Value, issues);
                    break;

                case "sections":
                    ParseSections(filePath, entry.Value, config.Sections, issues);
                    break;

                case "rules":
                    ParseRules(filePath, entry.Value, config.Rules, issues);
                    break;

                case "report":
                    report = ParseReport(filePath, entry.Value, issues);
                    break;

                default:
                    AddError(issues, filePath, GetLine(entry.Key), $"Unsupported top-level key '{key}'.");
                    break;
            }
        }

        return config with { Connector = connector, Report = report };
    }

    /// <summary>
    ///     Parses the connector block.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the connector block.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed connector configuration.</returns>
    private static ConnectorConfig ParseConnector(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        // The connector value must be a mapping node.
        if (node is not YamlMappingNode mapping)
        {
            AddError(issues, filePath, GetLine(node), "Connector must be a YAML mapping.");
            return new ConnectorConfig();
        }

        var connector = new ConnectorConfig();
        GitHubConnectorConfig? gitHub = null;
        AzureDevOpsConnectorConfig? azureDevOps = null;

        foreach (var entry in mapping.Children)
        {
            var key = GetScalarValue(entry.Key);
            switch (key)
            {
                case "type":
                    connector = connector with { Type = GetScalarValue(entry.Value) };
                    break;

                case "github":
                    gitHub = ParseGitHubConnector(filePath, entry.Value, issues);
                    break;

                case "azure-devops":
                    azureDevOps = ParseAzureDevOpsConnector(filePath, entry.Value, issues);
                    break;

                default:
                    AddError(issues, filePath, GetLine(entry.Key), $"Unsupported connector key '{key}'.");
                    break;
            }
        }

        return connector with { GitHub = gitHub, AzureDevOps = azureDevOps };
    }

    /// <summary>
    ///     Parses the report block.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the report block.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed report configuration.</returns>
    private static ReportConfig ParseReport(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        // The report value must be a mapping node.
        if (node is not YamlMappingNode mapping)
        {
            AddError(issues, filePath, GetLine(node), "Report must be a YAML mapping.");
            return new ReportConfig();
        }

        string? file = null;
        int? depth = null;
        bool? includeKnownIssues = null;

        foreach (var entry in mapping.Children)
        {
            var key = GetScalarValue(entry.Key);
            switch (key)
            {
                case "file":
                    file = GetOptionalScalarValue(entry.Value);
                    if (file == null)
                    {
                        AddError(issues, filePath, GetLine(entry.Value),
                            "Report file must be a non-empty string.");
                    }
                    break;

                case "depth":
                    depth = ParseReportDepth(filePath, entry.Value, issues);
                    break;

                case "include-known-issues":
                    includeKnownIssues = ParseReportIncludeKnownIssues(filePath, entry.Value, issues);
                    break;

                default:
                    AddError(issues, filePath, GetLine(entry.Key),
                        $"Unsupported report key '{key}'.");
                    break;
            }
        }

        return new ReportConfig
        {
            File = file,
            Depth = depth,
            IncludeKnownIssues = includeKnownIssues
        };
    }

    /// <summary>
    ///     Parses the report depth value from a YAML node.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the depth value.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed depth value, or null if invalid.</returns>
    private static int? ParseReportDepth(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        var depthStr = GetScalarValue(node);
        if (!int.TryParse(depthStr, out var depthValue) || depthValue < 1)
        {
            AddError(issues, filePath, GetLine(node),
                "Report depth must be a positive integer.");
            return null;
        }

        return depthValue;
    }

    /// <summary>
    ///     Parses the report include-known-issues value from a YAML node.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the include-known-issues value.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed boolean value, or null if invalid.</returns>
    private static bool? ParseReportIncludeKnownIssues(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        var boolStr = GetScalarValue(node);
        if (!bool.TryParse(boolStr, out var boolValue))
        {
            AddError(issues, filePath, GetLine(node),
                "Report include-known-issues must be 'true' or 'false'.");
            return null;
        }

        return boolValue;
    }

    /// <summary>
    ///     Parses the GitHub connector block.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the GitHub connector.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed GitHub connector configuration.</returns>
    private static GitHubConnectorConfig ParseGitHubConnector(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        // The GitHub connector value must be a mapping node.
        if (node is not YamlMappingNode mapping)
        {
            AddError(issues, filePath, GetLine(node), "GitHub connector must be a YAML mapping.");
            return new GitHubConnectorConfig();
        }

        string? owner = null;
        string? repo = null;
        string? baseUrl = null;

        foreach (var entry in mapping.Children)
        {
            var key = GetScalarValue(entry.Key);
            var value = GetScalarValue(entry.Value);
            switch (key)
            {
                case "owner":
                    owner = value;
                    break;

                case "repo":
                    repo = value;
                    break;

                case "repository":
                    ApplyRepositoryOverride(value, ref owner, ref repo, filePath, GetLine(entry.Value), issues);
                    break;

                case "base-url":
                case "baseurl":
                case "url":
                    baseUrl = value;
                    break;

                default:
                    AddError(issues, filePath, GetLine(entry.Key), $"Unsupported GitHub connector key '{key}'.");
                    break;
            }
        }

        return new GitHubConnectorConfig { Owner = owner, Repo = repo, BaseUrl = baseUrl };
    }

    /// <summary>
    ///     Applies a combined repository override.
    /// </summary>
    /// <param name="repository">The repository value to parse.</param>
    /// <param name="owner">The owner output parameter.</param>
    /// <param name="repo">The repo output parameter.</param>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="lineNumber">The source line number.</param>
    /// <param name="issues">The collected issues.</param>
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
    ///     Gets an optional scalar value from the specified YAML node.
    /// </summary>
    /// <param name="node">The YAML node to extract a value from.</param>
    /// <returns>The scalar value, or null if the value is empty or whitespace.</returns>
    private static string? GetOptionalScalarValue(YamlNode node)
    {
        var value = GetScalarValue(node);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    ///     Parses the Azure DevOps connector block.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the Azure DevOps connector.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed Azure DevOps connector configuration.</returns>
    private static AzureDevOpsConnectorConfig ParseAzureDevOpsConnector(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        // The Azure DevOps connector value must be a mapping node.
        if (node is not YamlMappingNode mapping)
        {
            AddError(issues, filePath, GetLine(node), "Azure DevOps connector must be a YAML mapping.");
            return new AzureDevOpsConnectorConfig();
        }

        string? organizationUrl = null;
        string? organization = null;
        string? project = null;
        string? repository = null;

        foreach (var entry in mapping.Children)
        {
            var key = GetScalarValue(entry.Key);
            var value = GetOptionalScalarValue(entry.Value);
            switch (key)
            {
                case "url":
                    organizationUrl = value;
                    break;

                case "organization":
                case "org":
                    organization = value;
                    break;

                case "project":
                    project = value;
                    break;

                case "repository":
                case "repo":
                    repository = value;
                    break;

                default:
                    AddError(issues, filePath, GetLine(entry.Key),
                        $"Unsupported Azure DevOps connector key '{key}'.");
                    break;
            }
        }

        return new AzureDevOpsConnectorConfig
        {
            OrganizationUrl = organizationUrl,
            Organization = organization,
            Project = project,
            Repository = repository
        };
    }

    /// <summary>
    ///     Parses the sections block.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the sections block.</param>
    /// <param name="sections">The target section list.</param>
    /// <param name="issues">The collected issues.</param>
    private static void ParseSections(
        string filePath,
        YamlNode node,
        List<SectionConfig> sections,
        List<ConfigurationIssue> issues)
    {
        // The sections value must be a sequence node.
        if (node is not YamlSequenceNode sequence)
        {
            AddError(issues, filePath, GetLine(node), "Sections must be a YAML sequence.");
            return;
        }

        foreach (var item in sequence.Children)
        {
            var section = ParseSectionItem(filePath, item, issues);
            if (!string.IsNullOrEmpty(section.Id) && !string.IsNullOrEmpty(section.Title))
            {
                sections.Add(section);
            }
        }
    }

    /// <summary>
    ///     Parses one section item.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the section item.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed section configuration.</returns>
    private static SectionConfig ParseSectionItem(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        // Each section item must be a mapping node with id and title.
        if (node is not YamlMappingNode mapping)
        {
            AddError(issues, filePath, GetLine(node), "Section entries must be YAML mappings.");
            return new SectionConfig();
        }

        string? id = null;
        string? title = null;

        foreach (var entry in mapping.Children)
        {
            var key = GetScalarValue(entry.Key);
            switch (key)
            {
                case "id":
                    id = GetScalarValue(entry.Value);
                    break;

                case "title":
                    title = GetScalarValue(entry.Value);
                    break;

                default:
                    AddError(issues, filePath, GetLine(entry.Key), $"Unsupported section key '{key}'.");
                    break;
            }
        }

        if (string.IsNullOrEmpty(id))
        {
            AddError(issues, filePath, GetLine(node), "Section entries require an id.");
        }

        if (string.IsNullOrEmpty(title))
        {
            AddError(issues, filePath, GetLine(node), "Section entries require a title.");
        }

        return new SectionConfig { Id = id ?? string.Empty, Title = title ?? string.Empty };
    }

    /// <summary>
    ///     Parses the rules block.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the rules block.</param>
    /// <param name="rules">The target rule list.</param>
    /// <param name="issues">The collected issues.</param>
    private static void ParseRules(
        string filePath,
        YamlNode node,
        List<RuleConfig> rules,
        List<ConfigurationIssue> issues)
    {
        // The rules value must be a sequence node.
        if (node is not YamlSequenceNode sequence)
        {
            AddError(issues, filePath, GetLine(node), "Rules must be a YAML sequence.");
            return;
        }

        foreach (var item in sequence.Children)
        {
            rules.Add(ParseRuleItem(filePath, item, issues));
        }
    }

    /// <summary>
    ///     Parses one routing rule item.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the rule item.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed rule configuration.</returns>
    private static RuleConfig ParseRuleItem(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        // Each rule item must be a mapping node.
        if (node is not YamlMappingNode mapping)
        {
            AddError(issues, filePath, GetLine(node), "Rule entries must be YAML mappings.");
            return new RuleConfig();
        }

        RuleMatchConfig? match = null;
        string? route = null;

        foreach (var entry in mapping.Children)
        {
            var key = GetScalarValue(entry.Key);
            switch (key)
            {
                case "route":
                    route = GetScalarValue(entry.Value);
                    break;

                case "match":
                    match = ParseRuleMatch(filePath, entry.Value, issues);
                    break;

                default:
                    AddError(issues, filePath, GetLine(entry.Key), $"Unsupported rule key '{key}'.");
                    break;
            }
        }

        return new RuleConfig { Match = match, Route = route };
    }

    /// <summary>
    ///     Parses a nested rule match block.
    /// </summary>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="node">The YAML node for the match block.</param>
    /// <param name="issues">The collected issues.</param>
    /// <returns>The parsed rule match configuration.</returns>
    private static RuleMatchConfig ParseRuleMatch(
        string filePath,
        YamlNode node,
        List<ConfigurationIssue> issues)
    {
        // The match value must be a mapping node.
        if (node is not YamlMappingNode mapping)
        {
            AddError(issues, filePath, GetLine(node), "Match must be a YAML mapping.");
            return new RuleMatchConfig();
        }

        var match = new RuleMatchConfig();

        foreach (var entry in mapping.Children)
        {
            var key = GetScalarValue(entry.Key);
            switch (key)
            {
                case "label":
                    match.Label.AddRange(ParseNodeStringList(entry.Value));
                    break;

                case "work-item-type":
                    match.WorkItemType.AddRange(ParseNodeStringList(entry.Value));
                    break;

                default:
                    AddError(issues, filePath, GetLine(entry.Key), $"Unsupported match key '{key}'.");
                    break;
            }
        }

        return match;
    }

    /// <summary>
    ///     Parses a YAML node as a string list (scalar or sequence).
    /// </summary>
    /// <param name="node">The YAML value node.</param>
    /// <returns>The parsed strings.</returns>
    private static IEnumerable<string> ParseNodeStringList(YamlNode node)
    {
        // Accept either a scalar value or a sequence of scalars.
        if (node is YamlSequenceNode sequence)
        {
            return sequence.Children
                .Select(GetScalarValue)
                .Where(value => !string.IsNullOrWhiteSpace(value));
        }

        var scalar = GetScalarValue(node);
        return string.IsNullOrWhiteSpace(scalar) ? [] : [scalar];
    }

    /// <summary>
    ///     Gets the scalar string value from a YAML node.
    /// </summary>
    /// <param name="node">The YAML node.</param>
    /// <returns>The scalar value, or an empty string if not a scalar node.</returns>
    private static string GetScalarValue(YamlNode node)
    {
        return node is YamlScalarNode scalar ? scalar.Value ?? string.Empty : string.Empty;
    }

    /// <summary>
    ///     Gets the 1-based line number from a YAML node.
    /// </summary>
    /// <param name="node">The YAML node.</param>
    /// <returns>The 1-based line number.</returns>
    private static int GetLine(YamlNode node)
    {
        // YamlDotNet uses 0-based line numbers; convert to 1-based.
        return (int)node.Start.Line + 1;
    }

    /// <summary>
    ///     Adds an error issue to the issue list.
    /// </summary>
    /// <param name="issues">The issue list.</param>
    /// <param name="filePath">The configuration file path.</param>
    /// <param name="lineNumber">The 1-based line number.</param>
    /// <param name="description">The error description.</param>
    private static void AddError(List<ConfigurationIssue> issues, string filePath, int lineNumber, string description)
    {
        issues.Add(new ConfigurationIssue(filePath, lineNumber, ConfigurationIssueSeverity.Error, description));
    }
}
