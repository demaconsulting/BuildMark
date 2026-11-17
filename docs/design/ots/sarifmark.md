## SarifMark Integration Design

### Purpose

SarifMark converts the SARIF results produced by CodeQL code scanning into a Markdown report that
is compiled into the Code Quality document. It makes the static-analysis results part of the
release record without depending on the GitHub user interface.

### Features Used

- `--sarif <file>` to read the CodeQL SARIF output.
- `--report <markdown>`, `--heading <text>`, and `--report-depth <n>` to write the report.
- `--validate --results <trx>` self-validation.

### Integration Pattern

SarifMark is a dotnet local tool restored from `.config/dotnet-tools.json`. The `codeql` job
uploads `csharp.sarif`, and the `build-docs` job runs:

- `dotnet sarifmark --validate --results artifacts/sarifmark-self-validation.trx`
- `dotnet sarifmark --sarif artifacts/csharp.sarif --report
  docs/code_quality/generated/codeql-quality.md --heading "BuildMark CodeQL Analysis"
  --report-depth 1`

The report is compiled into the Code Quality document by Pandoc and WeasyPrint. No configuration
file is used.
