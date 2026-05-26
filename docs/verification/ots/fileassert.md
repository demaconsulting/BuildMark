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

### Test Scenarios

- *FileAssert self-validation*: CI pipeline executes
  `dotnet fileassert --validate --results artifacts/fileassert-self-validation.trx`;
  expects exit code 0 and a non-empty TRX file containing self-test results.
- *FileAssert document assertions*: FileAssert is invoked for each generated document
  collection (Build Notes, Code Quality, Review Plan, Review Report, Design, User Guide,
  Verification); expects that each assertion set passes and a non-empty TRX file is
  produced per collection.
