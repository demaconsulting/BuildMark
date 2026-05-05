## ReqStream

### Verification Approach

ReqStream is an OTS requirements traceability tool. Verification is achieved through
the tool's built-in self-validation (`--validate`) executed in the CI pipeline. The
self-validation runs all internal checks and writes results to a TRX file. Successful
completion of the self-validation step constitutes evidence that the tool is functioning
correctly in the build environment.

Additionally, ReqStream is exercised operationally when it processes the project's
requirements YAML files against TRX evidence and enforces that all requirements are
satisfied (`--enforce`). Successful execution of ReqStream in enforcement mode provides
further evidence of correct operation.

### Evidence

The CI pipeline step `Run ReqStream self-validation` executes:

```bash
dotnet reqstream --validate --results artifacts/reqstream-self-validation.trx
```

The resulting TRX file is consumed by ReqStream itself to satisfy the OTS requirement.

### Requirements Coverage

- **BuildMark-OTS-ReqStream**: CI pipeline self-validation TRX evidence from
  `artifacts/reqstream-self-validation.trx`
