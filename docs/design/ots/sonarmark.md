## SonarMark Integration Design

### Purpose

SonarMark retrieves the SonarCloud quality gate, issues, and security hot-spots for the
`demaconsulting_BuildMark` project and writes them as a Markdown report compiled into the Code
Quality document. It captures the SonarCloud analysis as part of the release record.

### Features Used

- `--server`, `--project-key`, `--branch`, and `--token` to query SonarCloud.
- `--report <markdown>` and `--report-depth <n>` to write the report.
- `--validate --results <trx>` self-validation.

### Integration Pattern

SonarMark is a dotnet local tool restored from `.config/dotnet-tools.json`. The SonarCloud
analysis itself is produced by `dotnet-sonarscanner` in the `build` job. The `build-docs` job
runs:

- `dotnet sonarmark --validate --results artifacts/sonarmark-self-validation.trx`
- `dotnet sonarmark --server https://sonarcloud.io --project-key demaconsulting_BuildMark
  --branch <branch> --token <SONAR_TOKEN> --report docs/code_quality/generated/sonar-quality.md
  --report-depth 1`

The token is supplied from the `SONAR_TOKEN` repository secret. No configuration file is used.
