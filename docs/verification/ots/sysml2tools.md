## SysML2Tools

### Verification Approach

SysML2Tools is an OTS build tool. Verification is achieved through the tool's built-in
self-validation (`--validate`) executed in the CI pipeline, which writes its results to a TRX
file. The self-tests lint a model and render its views to SVG. SysML2Tools is also exercised by
`lint.ps1`, which lints the BuildMark model, and by the design document build, which renders its
views.

### Evidence

The CI pipeline step `Run SysML2Tools self-validation` in the `build-docs` job executes:

```bash
dotnet sysml2tools --validate --results artifacts/sysml2tools-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the SysML2Tools OTS requirements.

### Test Scenarios

- *SysML2Tools self-validation*: CI pipeline executes
  `dotnet sysml2tools --validate --results artifacts/sysml2tools-self-validation.trx`;
  expects exit code 0 and a TRX file in which every self-test passes.
