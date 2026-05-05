## Pandoc

### Verification Approach

Pandoc is an OTS document conversion tool. Verification is achieved through
repeated document generation in the CI pipeline. Pandoc converts markdown source
files to HTML for each document collection (Build Notes, Code Quality, Review Plan,
Review Report, Design, User Guide, and Verification). FileAssert then validates that
each generated HTML file contains expected content, providing evidence of correct
Pandoc operation.

### Evidence

The CI pipeline generates HTML output using Pandoc for each document section, and
FileAssert validates the output. For example, for the Design document:

```bash
dotnet pandoc --defaults docs/design/definition.yaml ... --output docs/design/generated/design.html
dotnet fileassert --results artifacts/fileassert-design.trx design
```

FileAssert TRX files (`fileassert-build-notes.trx`, `fileassert-code-quality.trx`,
`fileassert-code-review.trx`, `fileassert-design.trx`, `fileassert-user-guide.trx`,
`fileassert-verification.trx`) are consumed by ReqStream to satisfy the OTS requirement.

### Requirements Coverage

- **BuildMark-OTS-Pandoc**: CI pipeline document generation evidence from multiple
  FileAssert TRX results confirming successful HTML conversion
