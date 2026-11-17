# OTS Integration Design

BuildMark relies on off-the-shelf (OTS) software items that fall into two groups: one runtime
library compiled into the shipped tool, and the tools and frameworks that build, test, analyze,
and document it. This chapter describes the overall OTS integration strategy and introduces each
item's role.

## Integration Strategy

The OTS items used by this project fall into four functional groups:

- **Production code library** — YamlDotNet is referenced by the `DemaConsulting.BuildMark`
  project and is consumed only by the `BuildMarkConfigReader` unit of the Configuration
  subsystem. No YamlDotNet type appears in any public signature, so an upgrade or replacement
  affects only that unit.
- **Test framework** — the xUnit packages referenced by the test project and consumed by
  `dotnet test`.
- **Build pipeline tools** — BuildMark, VersionMark, SarifMark, SonarMark, ReviewMark, ReqStream,
  and FileAssert are dotnet local tools invoked by the GitHub Actions workflow and, for linting,
  by `lint.ps1`. Each performs one compliance step such as capturing build metadata, recording
  tool versions, publishing quality reports, or enforcing review and traceability coverage.
- **Document generation** — Pandoc converts Markdown to HTML and WeasyPrint converts HTML to PDF;
  both are dotnet local tools.

## OTS Item Summary

| OTS Item | Role |
| :--- | :--- |
| BuildMark | Generates the build-notes report from repository metadata |
| FileAssert | Validates the generated HTML and PDF documents |
| Pandoc | Converts Markdown document collections to HTML |
| ReqStream | Enforces requirements traceability against TRX test-result files |
| ReviewMark | Generates the review plan and review report from `.reviewmark.yaml` |
| SarifMark | Converts CodeQL SARIF results into a Markdown quality report |
| SonarMark | Generates a SonarCloud quality and security metrics report |
| VersionMark | Captures and publishes tool-version information for each CI job |
| WeasyPrint | Converts HTML documents to PDF for release artifacts |
| xUnit | Discovers, executes, and reports unit tests; produces TRX output |
| YamlDotNet | Parses the optional `.buildmark.yaml` configuration file |

> **Note**: The BuildMark OTS item is the previously released `DemaConsulting.BuildMark` dotnet
> tool pinned in `.config/dotnet-tools.json`, not the code built from this repository.

## Per-Item Design

Detailed design for each OTS item is provided in the following sections of this document:

- See *BuildMark Integration Design* for the build-notes generation tool.
- See *FileAssert Integration Design* for the document assertion tool.
- See *Pandoc Integration Design* for the Markdown-to-HTML conversion tool.
- See *ReqStream Integration Design* for the requirements traceability tool.
- See *ReviewMark Integration Design* for the review plan and report tool.
- See *SarifMark Integration Design* for the CodeQL SARIF report tool.
- See *SonarMark Integration Design* for the SonarCloud quality report tool.
- See *VersionMark Integration Design* for the tool-version capture and publish tool.
- See *WeasyPrint Integration Design* for the HTML-to-PDF conversion tool.
- See *xUnit Integration Design* for the unit-testing framework.
- See *YamlDotNet Integration Design* for the YAML parsing library.

## Selection Criteria

OTS items are selected against the following criteria:

- **License compatibility**: the license must permit use in the build pipeline and, for the
  runtime library, redistribution in the BuildMark package without reciprocal obligations on
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

- **NuGet library** (YamlDotNet) — restored with the main project and called through its public
  API from `BuildMarkConfigReader` only, which converts every YamlDotNet result and exception
  into BuildMark configuration types.
- **NuGet test framework** (xUnit) — referenced only by the test project; it never flows to the
  published package.
- **Dotnet tools** (all other items) — invoked as `dotnet <tool>` with documented command-line
  arguments. No wrapper code is written. A non-zero exit code fails the CI step, so any tool
  failure stops the pipeline before artifacts are published.

## Qualification Strategy

- **Integration tests**: YamlDotNet is qualified by the Configuration tests that load real YAML
  files through `BuildMarkConfigReader` (see *YamlDotNet Verification*).
- **Self-validation**: BuildMark, VersionMark, SarifMark, SonarMark, ReviewMark, ReqStream, and
  FileAssert run `--validate` in CI and write TRX results consumed by ReqStream.
- **Downstream assertion**: Pandoc and WeasyPrint outputs are checked by FileAssert for every
  document collection.
- **Implicit qualification**: xUnit is qualified by the passing of the full test suite on every
  supported platform.
