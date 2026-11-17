## BuildMark Integration Design

### Purpose

The BuildMark OTS item is the previously released `DemaConsulting.BuildMark` dotnet tool pinned
in `.config/dotnet-tools.json`. The CI pipeline uses it to generate the build-notes report for
each build. Using the released tool rather than the code under development keeps the release
evidence independent of the changes being built.

### Features Used

- `--build-version`, `--report`, and `--report-depth` to write the build-notes Markdown report.
- GitHub repository data access authenticated through the `GH_TOKEN` environment variable.
- `--validate --results` self-validation, which writes a TRX file of self-test results.

### Integration Pattern

The tool is restored with `dotnet tool restore` in the `build-docs` job of
`.github/workflows/build.yaml`, which then runs:

- `dotnet buildmark --validate --results artifacts/buildmark-self-validation.trx`
- `dotnet buildmark --build-version <version> --report docs/build_notes/generated/build_notes.md
  --report-depth 1`

The generated Markdown is compiled into the Build Notes document by Pandoc and WeasyPrint. No
configuration file is passed; the tool reads the repository's optional `.buildmark.yaml` if
present.
