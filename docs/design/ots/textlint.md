## TextLint Integration Design

### Purpose

TextLint spell-checks the repository's documentation, requirements, and source comments so that
misspellings are caught in the lint gate instead of in the published documents.

### Features Used

- Checking the files matched by the globs in `.textlint.yaml` against its dictionary of
  technical terms.
- `--validate --results <trx>` self-validation.

### Integration Pattern

TextLint is the dotnet local tool `DemaConsulting.TextLint.Tool`, restored from
`.config/dotnet-tools.json` and configured by `.textlint.yaml`.

- `lint.ps1` runs `dotnet textlint`; any finding fails the lint gate.
- The `quality-checks` job runs `dotnet textlint --validate --results
  artifacts/textlint-self-validation.trx` before `lint.ps1`. The self-validation exercises the
  spell-checking, the settings file, and inline directives, so its results are the evidence for
  the TextLint OTS requirement.
