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
///     Represents the top-level BuildMark configuration.
/// </summary>
public sealed record BuildMarkConfig
{
    /// <summary>
    ///     Gets or sets the optional connector configuration.
    /// </summary>
    public ConnectorConfig? Connector { get; init; }

    /// <summary>
    ///     Gets the configured report sections.
    /// </summary>
    public List<SectionConfig> Sections { get; } = [];

    /// <summary>
    ///     Gets the configured routing rules.
    /// </summary>
    public List<RuleConfig> Rules { get; } = [];

    /// <summary>
    ///     Creates a default configuration with built-in sections and routing rules.
    /// </summary>
    /// <remarks>
    ///     The default configuration provides three report sections (Changes, Bugs Fixed,
    ///     and Dependency Updates) along with routing rules that classify items by label
    ///     and work-item type. This is used when no <c>.buildmark.yaml</c> file is present.
    /// </remarks>
    /// <returns>A new <see cref="BuildMarkConfig"/> with default sections and rules.</returns>
    public static BuildMarkConfig CreateDefault()
    {
        var config = new BuildMarkConfig();

        // Define the default report sections
        config.Sections.Add(new SectionConfig { Id = "changes", Title = "Changes" });
        config.Sections.Add(new SectionConfig { Id = "bugs-fixed", Title = "Bugs Fixed" });
        config.Sections.Add(new SectionConfig { Id = "dependency-updates", Title = "Dependency Updates" });

        // Route dependency-manager labels to the dependency-updates section
        var dependencyMatch = new RuleMatchConfig();
        dependencyMatch.Label.AddRange(["dependencies", "renovate", "dependabot"]);
        config.Rules.Add(new RuleConfig { Match = dependencyMatch, Route = "dependency-updates" });

        // Route Bug work-item types to the bugs-fixed section
        var bugWorkItemMatch = new RuleMatchConfig();
        bugWorkItemMatch.WorkItemType.Add("Bug");
        config.Rules.Add(new RuleConfig { Match = bugWorkItemMatch, Route = "bugs-fixed" });

        // Route bug-related labels to the bugs-fixed section
        var bugLabelMatch = new RuleMatchConfig();
        bugLabelMatch.Label.AddRange(["bug", "defect", "regression"]);
        config.Rules.Add(new RuleConfig { Match = bugLabelMatch, Route = "bugs-fixed" });

        // Suppress internal and chore labels from the report
        var internalLabelMatch = new RuleMatchConfig();
        internalLabelMatch.Label.AddRange(["internal", "chore"]);
        config.Rules.Add(new RuleConfig { Match = internalLabelMatch, Route = "suppressed" });

        // Suppress Task and Epic work-item types from the report
        var taskWorkItemMatch = new RuleMatchConfig();
        taskWorkItemMatch.WorkItemType.AddRange(["Task", "Epic"]);
        config.Rules.Add(new RuleConfig { Match = taskWorkItemMatch, Route = "suppressed" });

        // Catch-all rule routes unmatched items to the changes section
        config.Rules.Add(new RuleConfig { Route = "changes" });

        return config;
    }
}
