# BuildMark

## Verification Approach

BuildMark is an OTS tool. Verification is achieved through the tool's built-in
self-validation (`--validate`) executed in the CI pipeline. The self-validation
runs all internal checks and writes results to a TRX file. Successful completion
of the self-validation step constitutes evidence that the tool is functioning
correctly in the build environment.

## Evidence

The CI pipeline step `Run BuildMark self-validation` executes:

```bash
dotnet buildmark --validate --results artifacts/buildmark-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the OTS requirement.

## Requirements Coverage

- **BuildMark-OTS-BuildMark**: CI pipeline self-validation TRX evidence from
  `artifacts/buildmark-self-validation.trx`
