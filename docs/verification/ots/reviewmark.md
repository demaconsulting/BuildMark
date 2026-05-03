# ReviewMark

## Verification Approach

ReviewMark is an OTS code review enforcement tool. Verification is achieved through
the tool's built-in self-validation (`--validate`) executed in the CI pipeline. The
self-validation runs all internal checks and writes results to a TRX file. Successful
completion of the self-validation step constitutes evidence that the tool is functioning
correctly in the build environment.

Additionally, ReviewMark is exercised operationally when it generates the Review Plan
and Review Report documents from the `.reviewmark.yaml` configuration. Successful
document generation provides further evidence of correct operation.

## Evidence

The CI pipeline step `Run ReviewMark self-validation` executes:

```bash
dotnet reviewmark --validate --results artifacts/reviewmark-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the OTS requirement.

## Requirements Coverage

- **BuildMark-OTS-ReviewMark**: CI pipeline self-validation TRX evidence from
  `artifacts/reviewmark-self-validation.trx`
