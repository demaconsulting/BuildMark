## LintMark

### Verification Approach

LintMark is an OTS build tool. Verification is achieved through the tool's built-in
self-validation (`--validate`) executed in the CI pipeline, which writes its results to a TRX
file. The self-validation of the pinned LintMark version checks only its version and help output, so
it proves that the tool starts in the build environment; it does not exercise the lint rules.
LintMark is also exercised on every run of `lint.ps1`, which fails on any formatting violation.

### Evidence

The CI pipeline step `Run LintMark self-validation` in the `quality-checks` job executes:

```bash
dotnet lintmark --validate --results artifacts/lintmark-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the LintMark OTS requirements.

### Test Scenarios

- *LintMark self-validation*: CI pipeline executes
  `dotnet lintmark --validate --results artifacts/lintmark-self-validation.trx`;
  expects exit code 0 and a TRX file in which every self-test passes.
