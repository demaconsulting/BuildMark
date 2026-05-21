## VersionMark

### Verification Approach

VersionMark is an OTS tool version capture tool. Verification is achieved through the
tool's built-in self-validation (`--validate`) executed in the CI pipeline across
multiple jobs. The self-validation runs all internal checks and writes results to TRX
files. Successful completion of the self-validation steps constitutes evidence that the
tool is functioning correctly across all build environments.

Additionally, VersionMark is exercised operationally in every job to capture and publish
tool version information. Successful version capture and publication provides further
evidence of correct operation.

### Evidence

The CI pipeline runs VersionMark self-validation in multiple jobs:

```bash
dotnet versionmark --validate --results artifacts/versionmark-self-validation-quality.trx
dotnet versionmark --validate --results artifacts/versionmark-self-validation.trx
```

The resulting TRX files are consumed by ReqStream to satisfy the OTS requirement.

### Test Scenarios

- *VersionMark self-validation (quality job)*: CI pipeline executes
  `dotnet versionmark --validate --results artifacts/versionmark-self-validation-quality.trx`;
  expects exit code 0 and a non-empty TRX file.
- *VersionMark self-validation (main job)*: CI pipeline executes
  `dotnet versionmark --validate --results artifacts/versionmark-self-validation.trx`;
  expects exit code 0 and a non-empty TRX file.
- *VersionMark operational use*: VersionMark is invoked in every CI job to capture and
  publish tool version information; expects exit code 0 and correct version output for
  each job.

### Requirements Coverage

- **BuildMark-OTS-VersionMark**: CI pipeline self-validation TRX evidence from
  `artifacts/versionmark-self-validation.trx` and
  `artifacts/versionmark-self-validation-quality.trx`
