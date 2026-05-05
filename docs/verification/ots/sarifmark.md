## SarifMark

### Verification Approach

SarifMark is an OTS SARIF report generation tool. Verification is achieved through
the tool's built-in self-validation (`--validate`) executed in the CI pipeline. The
self-validation runs all internal checks and writes results to a TRX file. Successful
completion of the self-validation step constitutes evidence that the tool is functioning
correctly in the build environment.

Additionally, SarifMark is exercised operationally when it processes the CodeQL SARIF
output and generates the CodeQL quality report markdown. Successful report generation
provides further evidence of correct operation.

### Evidence

The CI pipeline step `Run SarifMark self-validation` executes:

```bash
dotnet sarifmark --validate --results artifacts/sarifmark-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the OTS requirement.

### Requirements Coverage

- **BuildMark-OTS-SarifMark**: CI pipeline self-validation TRX evidence from
  `artifacts/sarifmark-self-validation.trx`
