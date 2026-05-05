## FileAssert

### Verification Approach

FileAssert is an OTS tool. Verification is achieved through the tool's built-in
self-validation (`--validate`) executed in the CI pipeline. The self-validation
runs all internal checks and writes results to a TRX file. Successful completion
of the self-validation step constitutes evidence that the tool is functioning
correctly in the build environment.

Additionally, FileAssert is exercised indirectly by asserting the content of each
generated document (Build Notes, Code Quality, Review Plan, Review Report, Design,
User Guide, and Verification). Each successful assertion confirms that FileAssert
is able to inspect and validate file content as required.

### Evidence

The CI pipeline step `Run FileAssert self-validation` executes:

```bash
dotnet fileassert --validate --results artifacts/fileassert-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the OTS requirement.

### Requirements Coverage

- **BuildMark-OTS-FileAssert**: CI pipeline self-validation TRX evidence from
  `artifacts/fileassert-self-validation.trx`
