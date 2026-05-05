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

### Requirements Coverage

- **BuildMark-OTS-SonarMark**: CI pipeline self-validation TRX evidence from
  `artifacts/sonarmark-self-validation.trx`
