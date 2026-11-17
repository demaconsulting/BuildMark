## ReqStream Integration Design

### Purpose

ReqStream reads the requirements tree rooted at `requirements.yaml` and the TRX test results
from the pipeline, and produces the requirements report, the justifications document, and the
trace matrix. With `--enforce` it fails the build when any requirement lacks passing test
evidence or is not traceable to a root requirement.

### Features Used

- `--requirements <file>` with the `includes:` list and `root-tags` (`system`, `quality`).
- `--tests <glob>` to read TRX results.
- `--report`, `--justifications`, and `--matrix` to write the generated documents.
- `--enforce` to make missing coverage and orphaned requirements an error.
- `--lint` to validate the requirements files.
- `--validate --results <trx>` self-validation.

### Integration Pattern

ReqStream is a dotnet local tool restored from `.config/dotnet-tools.json`. Requirements files
live under `docs/reqstream/` and are included from `requirements.yaml`.

- `lint.ps1` runs `dotnet reqstream --lint --enforce --requirements requirements.yaml`.
- The `build-docs` job runs `dotnet reqstream --validate --results
  artifacts/reqstream-self-validation.trx`, then `dotnet reqstream --requirements
  requirements.yaml --tests "artifacts/**/*.trx" --report
  docs/requirements_doc/generated/requirements.md --justifications
  docs/requirements_doc/generated/justifications.md --matrix
  docs/requirements_report/generated/trace_matrix.md --enforce`.

This step runs after all test, self-validation, and FileAssert TRX files are collected, so it
sees all evidence. The generated Markdown is compiled into the Requirements and Trace Matrix
documents by Pandoc and WeasyPrint.
