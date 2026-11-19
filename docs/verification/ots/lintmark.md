## LintMark

### Verification Approach

LintMark is an OTS build tool. Verification is achieved through the tool's built-in
self-validation (`--validate`) executed in the CI pipeline, which writes its results to a TRX
file. The self-tests check that the pinned tool starts and reports its version and usage, that a
conforming file produces no findings, that rule violations are reported with a non-zero exit code,
that rule settings from the configuration file are honored, and that the fix mode rewrites
violations to the canonical form. LintMark is also exercised on every run of `lint.ps1`.

### Evidence

The CI pipeline step `Run LintMark self-validation` in the `quality-checks` job executes:

```bash
dotnet lintmark --validate --results artifacts/lintmark-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the LintMark OTS requirements.

### Test Scenarios

- *LintMark starts*: The self-validation checks the version and help output. This scenario is
  tested by `LintMark_VersionDisplay` and `LintMark_HelpDisplay`.
- *LintMark checks rules*: The self-validation checks that a conforming file produces no findings,
  that a file with violations is reported with a non-zero exit code, and that rule settings from
  the configuration file are honored. This scenario is tested by `LintMark_CleanFileNoFindings`,
  `LintMark_ViolationsReported`, and `LintMark_ConfigurationHonored`.
- *LintMark fixes formatting*: The self-validation checks that `--fix` rewrites a file with
  violations to the canonical form. This scenario is tested by
  `LintMark_FixRewritesToCanonicalForm`.
