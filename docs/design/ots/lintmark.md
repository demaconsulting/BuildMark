## LintMark Integration Design

### Purpose

LintMark checks the formatting of every Markdown file so that the documents compile consistently
with Pandoc and read consistently in the repository.

### Features Used

- Checking the Markdown files against the rules configured in `.lintmark.yaml`.
- Fixing formatting in place, used by `fix.ps1`.
- `--validate --results <trx>` self-validation.

### Integration Pattern

LintMark is the dotnet local tool `DemaConsulting.LintMark`, restored from
`.config/dotnet-tools.json` and configured by `.lintmark.yaml`.

- `lint.ps1` runs `dotnet lintmark`; any finding fails the lint gate.
- `fix.ps1` runs LintMark to apply its formatting fixes.
- The `quality-checks` job runs `dotnet lintmark --validate --results
  artifacts/lintmark-self-validation.trx` before `lint.ps1`. The self-validation exercises the
  lint rules, the configuration file, and the fix mode, so its results are the evidence for the
  LintMark OTS requirements.
