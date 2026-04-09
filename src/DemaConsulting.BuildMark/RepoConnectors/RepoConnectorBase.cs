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

using DemaConsulting.BuildMark.BuildNotes;
using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.Utilities;

namespace DemaConsulting.BuildMark.RepoConnectors;

/// <summary>
///     Base class for repository connectors with common functionality.
/// </summary>
public abstract class RepoConnectorBase : IRepoConnector
{
    /// <summary>
    ///     The routing rules for distributing items into report sections.
    /// </summary>
    private IReadOnlyList<RuleConfig> _rules = [];

    /// <summary>
    ///     The report section definitions.
    /// </summary>
    private IReadOnlyList<SectionConfig> _sections = [];

    /// <summary>
    ///     Gets a value indicating whether routing rules have been configured.
    /// </summary>
    protected bool HasRules => _rules.Count > 0;

    /// <summary>
    ///     Configures the routing rules and section definitions for this connector.
    /// </summary>
    /// <param name="rules">Routing rules to apply when distributing items.</param>
    /// <param name="sections">Section definitions that determine output structure.</param>
    public void Configure(IReadOnlyList<RuleConfig> rules, IReadOnlyList<SectionConfig> sections)
    {
        // Store the provided rules for use when routing items into sections
        _rules = rules;

        // Store the provided sections for use when building the output structure
        _sections = sections;
    }

    /// <summary>
    ///     Routes items into report sections using the configured rules and sections.
    /// </summary>
    /// <param name="allItems">All items to be routed.</param>
    /// <returns>Ordered list of section data with items assigned to each section.</returns>
    protected IReadOnlyList<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo> Items)> ApplyRules(
        IEnumerable<ItemInfo> allItems)
    {
        // Route all items into section buckets using configured rules
        var routedDict = ItemRouter.Route(allItems.ToList(), _rules, _sections);

        // Build ordered list of sections with their items, using configured section order
        var result = new List<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo> Items)>();

        // Process each configured section in order
        foreach (var section in _sections)
        {
            // Get items routed to this section (empty list if no items)
            var items = routedDict.TryGetValue(section.Id, out var bucket)
                ? (IReadOnlyList<ItemInfo>)bucket
                : [];

            // Add section to result list with its title and items
            result.Add((section.Id, section.Title, items));
        }

        // Include any extra sections created by rules that don't have corresponding SectionConfig entries
        foreach (var kvp in routedDict)
        {
            // Skip sections already included via configured section definitions
            if (_sections.Any(s => s.Id == kvp.Key))
            {
                continue;
            }

            // Add extra section using its ID as the title (no SectionConfig title available)
            result.Add((kvp.Key, kvp.Key, kvp.Value));
        }

        // Return the ordered section results
        return result;
    }

    /// <summary>
    ///     Runs a command and returns its output.
    /// </summary>
    /// <param name="command">Command to run.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <returns>Command output.</returns>
    protected virtual Task<string> RunCommandAsync(string command, string arguments)
    {
        // Delegate to ProcessRunner for command execution
        return ProcessRunner.RunAsync(command, arguments);
    }

    /// <summary>
    ///     Gets build information for a release.
    /// </summary>
    /// <param name="version">Optional target version. If not provided, uses the most recent tag if it matches current commit.</param>
    /// <returns>BuildInformation record with all collected data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    public abstract Task<BuildInformation> GetBuildInformationAsync(VersionInfo? version = null);

    /// <summary>
    ///     Finds the index of a version in a version list by normalized version string.
    /// </summary>
    /// <param name="versions">List of versions to search.</param>
    /// <param name="normalizedVersion">Normalized version string to find (e.g., "1.0.0" or "2.0.0-beta.1").</param>
    /// <returns>Index of the version in the list, or -1 if not found.</returns>
    /// <remarks>
    ///     This method is protected to allow repository connectors to determine version positions
    ///     when constructing BuildInformation objects.
    /// </remarks>
    protected static int FindVersionIndex(List<VersionInfo> versions, string normalizedVersion)
    {
        // Search for version matching the normalized version string
        for (var i = 0; i < versions.Count; i++)
        {
            if (versions[i].FullVersion.Equals(normalizedVersion, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        // VersionInfo not found in list
        return -1;
    }
}
