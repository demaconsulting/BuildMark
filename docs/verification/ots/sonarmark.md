## SonarMark

### Verification Approach

SonarMark is an OTS SonarCloud quality report generation tool. Verification is achieved
through the tool's built-in self-validation (`--validate`) executed in the CI pipeline.
The self-validation runs all internal checks and writes results to a TRX file. Successful
completion of the self-validation step constitutes evidence that the tool is functioning
correctly in the build environment.

Additionally, SonarMark is exercised operationally when it queries the SonarCloud API
and generates the SonarCloud quality report markdown. Successful report generation
provides further evidence of correct operation.

### Evidence

The CI pipeline step `Run SonarMark self-validation` executes:

```bash
dotnet sonarmark --validate --results artifacts/sonarmark-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the OTS requirement.

### Test Scenarios

- *SonarMark self-validation*: CI pipeline executes
  `dotnet sonarmark --validate --results artifacts/sonarmark-self-validation.trx`;
  expects exit code 0 and a non-empty TRX file containing self-test results.
- *SonarMark quality report generation*: CI pipeline uses SonarMark to query the
  SonarCloud API and generate the SonarCloud quality report markdown; expects exit
  code 0 and a non-empty markdown report.
