# OTS Integration Design

BuildMark relies on off-the-shelf (OTS) software items that fall into two groups: the runtime
libraries compiled into the shipped tool, and the tools and frameworks that build, test, analyze,
and document it. This chapter describes the overall OTS integration strategy and introduces each
item's role.

## Integration Strategy

The OTS items used by this project fall into five functional groups:

- **Production code libraries** — YamlDotNet, GraphQL.Client, and DemaConsulting.HelpCards are
  referenced by the `DemaConsulting.BuildMark` project. YamlDotNet is consumed only by the
  `BuildMarkConfigReader` unit of the Configuration subsystem, GraphQL.Client only by the
  `GitHubGraphQLClient` unit of the GitHub subsystem, and DemaConsulting.HelpCards only by the
  `HelpCommand` unit of the Cli subsystem. No library's types appear in any public signature, so
  an upgrade or replacement affects only that unit.
- **Test framework** — the xUnit packages referenced by the test project and consumed by
  `dotnet test`.
- **Build pipeline tools** — BuildMark, VersionMark, SarifMark, SonarMark, ReviewMark, ReqStream,
  and FileAssert are dotnet local tools invoked by the GitHub Actions workflow and, for linting,
  by `lint.ps1`. Each performs one compliance step such as capturing build metadata, recording
  tool versions, publishing quality reports, or enforcing review and traceability coverage.
- **Lint tools** — TextLint (spelling), LintMark (Markdown formatting), and SysML2Tools (model
  checking) are dotnet local tools run by `lint.ps1` in the quality-checks job.
- **Document generation** — Pandoc converts Markdown to HTML, CanvasNet draws the diagrams as a
  Pandoc filter, SysML2Tools renders the architecture views, and WeasyPrint converts HTML to PDF;
  all are dotnet local tools.

## OTS Item Summary

| OTS Item | Role |
| :--- | :--- |
| BuildMark | Generates the build-notes report from repository metadata |
| CanvasNet | Draws the Mermaid diagrams of the documents as a Pandoc filter |
| DemaConsulting.HelpCards | Serves the built-in `help` command from embedded help cards |
| FileAssert | Validates the generated HTML and PDF documents |
| GraphQL.Client | Queries the GitHub GraphQL API for repository data |
| LintMark | Checks the formatting of the Markdown files |
| Pandoc | Converts Markdown document collections to HTML |
| ReqStream | Enforces requirements traceability against TRX test-result files |
| ReviewMark | Generates the review plan and review report from `.reviewmark.yaml` |
| SarifMark | Converts CodeQL SARIF results into a Markdown quality report |
| SonarMark | Generates a SonarCloud quality and security metrics report |
| SysML2Tools | Checks the SysML2 model and renders its views to SVG |
| TextLint | Spell-checks the documentation, requirements, and source comments |
| VersionMark | Captures and publishes tool-version information for each CI job |
| WeasyPrint | Converts HTML documents to PDF for release artifacts |
| xUnit | Discovers, executes, and reports unit tests; produces TRX output |
| YamlDotNet | Parses the optional `.buildmark.yaml` configuration file |

> **Note**: The BuildMark OTS item is the previously released `DemaConsulting.BuildMark` dotnet
> tool pinned in `.config/dotnet-tools.json`, not the code built from this repository.

## Per-Item Design

Detailed design for each OTS item is provided in the following sections of this document:

- See *BuildMark Integration Design* for the build-notes generation tool.
- See *CanvasNet Integration Design* for the diagram drawing tool.
- See *DemaConsulting.HelpCards Integration Design* for the help card library.
- See *FileAssert Integration Design* for the document assertion tool.
- See *GraphQL.Client Integration Design* for the GraphQL client library.
- See *LintMark Integration Design* for the Markdown formatting checker.
- See *Pandoc Integration Design* for the Markdown-to-HTML conversion tool.
- See *ReqStream Integration Design* for the requirements traceability tool.
- See *ReviewMark Integration Design* for the review plan and report tool.
- See *SarifMark Integration Design* for the CodeQL SARIF report tool.
- See *SonarMark Integration Design* for the SonarCloud quality report tool.
- See *SysML2Tools Integration Design* for the model checker and diagram renderer.
- See *TextLint Integration Design* for the spell checker.
- See *VersionMark Integration Design* for the tool-version capture and publish tool.
- See *WeasyPrint Integration Design* for the HTML-to-PDF conversion tool.
- See *xUnit Integration Design* for the unit-testing framework.
- See *YamlDotNet Integration Design* for the YAML parsing library.

## Selection Criteria

OTS items are selected against the following criteria:

- **License compatibility**: the license must permit use in the build pipeline and, for the
  runtime libraries, redistribution in the BuildMark package without reciprocal obligations on
  this project's MIT-licensed source.
- **Active maintenance**: recent releases and an active public issue tracker.
- **Community adoption**: broad .NET community use, reducing abandonment risk.
- **Security track record**: vulnerabilities are disclosed and patched promptly.
- **Functional fit**: the item covers the required capability through a documented API or
  command-line interface without significant adapter code.

## Version Management Policy

Dotnet tools are pinned in `.config/dotnet-tools.json` and restored with `dotnet tool restore` at
the start of each CI job and by `lint.ps1`. NuGet package versions are pinned in the project
files. Version changes arrive through pull requests (including Dependabot) and must pass the full
build, test, lint, and documentation pipeline before merging. Version numbers are not recorded in
the design documentation; they are recorded by VersionMark in the build notes and in the
published SBOM.

## General Integration Approach

- **NuGet libraries** (YamlDotNet, GraphQL.Client, DemaConsulting.HelpCards) — restored with the
  main project and called through their public APIs from one unit each: `BuildMarkConfigReader`
  converts every YamlDotNet result and exception into BuildMark configuration types,
  `GitHubGraphQLClient` converts every GraphQL.Client response and exception into lists of
  BuildMark values, and `HelpCommand` answers the `help` command and returns its exit code.
- **NuGet test framework** (xUnit) — referenced only by the test project; it never flows to the
  published package.
- **Dotnet tools** (all other items) — invoked as `dotnet <tool>` with documented command-line
  arguments. No wrapper code is written. A non-zero exit code fails the CI step, so any tool
  failure stops the pipeline before artifacts are published.

## Qualification Strategy

- **Integration tests**: YamlDotNet is qualified by the Configuration tests that load real YAML
  files through `BuildMarkConfigReader` (see *YamlDotNet Verification*), and GraphQL.Client by
  the `GitHubGraphQLClient` tests that query a mocked GitHub endpoint and by OTS tests that drive
  `GraphQLHttpClient` directly against it (see *GraphQL.Client Verification*).
  DemaConsulting.HelpCards is qualified by the `HelpCommand` unit tests and by the integration
  test that runs every help example against the built tool (see *DemaConsulting.HelpCards
  Verification*).
- **Self-validation**: BuildMark, VersionMark, SarifMark, SonarMark, ReviewMark, ReqStream,
  FileAssert, TextLint, LintMark, SysML2Tools, and CanvasNet run `--validate` in CI and write TRX
  results consumed by ReqStream.
- **Downstream assertion**: Pandoc and WeasyPrint outputs are checked by FileAssert for every
  document collection.
- **Implicit qualification**: xUnit is qualified by the passing of the full test suite on every
  supported platform.
