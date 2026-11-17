## VersionMark Integration Design

### Purpose

VersionMark records which version of each tool was used in each CI job and publishes that record
as part of the build notes. It provides the audit trail that links each release to the tools that
produced it.

### Features Used

- `--capture --job-id <id> --output <json> -- <tools...>` to capture tool versions per job.
- `--publish --report <markdown> --report-depth <n> -- <json-globs>` to merge the captured
  versions into one Markdown report.
- `--validate --results <trx>` self-validation.
- `--lint` to check the `.versionmark.yaml` configuration.

### Integration Pattern

VersionMark is a dotnet local tool restored from `.config/dotnet-tools.json`. The commands used
to query each tool's version are defined in `.versionmark.yaml`.

- Each CI job (`quality`, `build`, `integration-test`, `build-docs`) runs `--capture` and
  uploads its `versionmark-*.json` file.
- The `quality` and `build` jobs run `--validate` and write `versionmark-self-validation*.trx`
  files, and the `build-docs` job runs it again to write `artifacts/versionmark-self-validation.trx`.
- The `build-docs` job runs `--publish` to write `docs/build_notes/generated/versions.md`, which
  is compiled into the Build Notes document.
- `lint.ps1` runs `dotnet versionmark --lint`.
