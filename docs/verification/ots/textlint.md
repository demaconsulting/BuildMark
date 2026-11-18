## TextLint

### Verification Approach

TextLint is an OTS build tool. Verification is achieved through the tool's built-in
self-validation (`--validate`) executed in the CI pipeline, which writes its results to a TRX
file. The self-tests check that a clean file passes, that a misspelling is reported, and that the
settings file and inline directives are honored. TextLint is also exercised on every run of
`lint.ps1`.

### Evidence

The CI pipeline step `Run TextLint self-validation` in the `quality-checks` job executes:

```bash
dotnet textlint --validate --results artifacts/textlint-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the TextLint OTS requirements.

### Test Scenarios

- *TextLint self-validation*: CI pipeline executes
  `dotnet textlint --validate --results artifacts/textlint-self-validation.trx`;
  expects exit code 0 and a TRX file in which every self-test passes.
