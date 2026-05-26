## ReviewMark

### Verification Approach

ReviewMark is an OTS code review enforcement tool. Verification is achieved through
the tool's built-in self-validation (`--validate`) executed in the CI pipeline. The
self-validation runs all internal checks and writes results to a TRX file. Successful
completion of the self-validation step constitutes evidence that the tool is functioning
correctly in the build environment.

Additionally, ReviewMark is exercised operationally when it generates the Review Plan
and Review Report documents from the `.reviewmark.yaml` configuration. Successful
document generation provides further evidence of correct operation.

### Evidence

The CI pipeline step `Run ReviewMark self-validation` executes:

```bash
dotnet reviewmark --validate --results artifacts/reviewmark-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the OTS requirement.

### Test Scenarios

- *ReviewMark self-validation*: CI pipeline executes
  `dotnet reviewmark --validate --results artifacts/reviewmark-self-validation.trx`;
  expects exit code 0 and a non-empty TRX file containing self-test results.
- *ReviewMark document generation*: CI pipeline uses ReviewMark to generate the Review
  Plan and Review Report documents from the `.reviewmark.yaml` configuration; expects
  exit code 0 and non-empty output documents for both the plan and report.
