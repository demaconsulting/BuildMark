## TextLint

### Verification Approach

TextLint is an OTS build tool. Verification is achieved through the tool's built-in
self-validation (`--validate`) executed in the CI pipeline, which writes its results to a TRX
file. The self-tests check that the pinned tool starts and reports its version and usage, that a
clean file passes, that a misspelling is reported, that the settings file is honored and an
invalid one is reported, and that inline directives are honored. TextLint is also exercised on
every run of `lint.ps1`.

### Evidence

The CI pipeline step `Run TextLint self-validation` in the `quality-checks` job executes:

```bash
dotnet textlint --validate --results artifacts/textlint-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the TextLint OTS requirements.

### Test Scenarios

- *TextLint starts*: The self-validation checks the version and help output. This scenario is
  tested by `TextLintTool_VersionDisplay` and `TextLintTool_HelpDisplay`.
- *TextLint reports misspellings*: The self-validation checks that a clean file passes and that a
  misspelled word is reported, including words split by cspell-style tokenization and binary files
  being skipped. This scenario is tested by `TextLintTool_CheckCleanFile`,
  `TextLintTool_CheckMisspelling`, `TextLintTool_CheckCspellTokenization`, and
  `TextLintTool_CheckBinaryFileSkipped`.
- *TextLint honors the settings file*: The self-validation checks that the settings file is read,
  that an invalid one is reported, and that its globs, overrides, technical terms, and language
  are applied. This scenario is tested by `TextLintTool_CheckSettingsFile`,
  `TextLintTool_CheckSettingsError`, `TextLintTool_CheckGlobsReplaceFiles`,
  `TextLintTool_CheckOverrides`, `TextLintTool_CheckTechnicalTerms`, and
  `TextLintTool_CheckLanguageOption`.
- *TextLint honors inline directives*: The self-validation checks that an inline directive
  suppresses a finding. This scenario is tested by `TextLintTool_CheckInlineDirective`.
