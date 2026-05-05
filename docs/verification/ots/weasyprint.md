## WeasyPrint

### Verification Approach

WeasyPrint is an OTS HTML-to-PDF rendering tool. Verification is achieved through
repeated document generation in the CI pipeline. WeasyPrint converts HTML files to PDF
for each document collection (Build Notes, Code Quality, Review Plan, Review Report,
Design, User Guide, and Verification). FileAssert then validates that each generated
PDF file contains expected content and metadata, providing evidence of correct WeasyPrint
operation.

### Evidence

The CI pipeline generates PDF output using WeasyPrint for each document section, and
FileAssert validates the output. For example, for the Design document:

```bash
dotnet weasyprint --pdf-variant pdf/a-3u docs/design/generated/design.html \
  "docs/generated/BuildMark Software Design.pdf"
dotnet fileassert --results artifacts/fileassert-design.trx design
```

FileAssert TRX files (`fileassert-build-notes.trx`, `fileassert-code-quality.trx`,
`fileassert-code-review.trx`, `fileassert-design.trx`, `fileassert-user-guide.trx`,
`fileassert-verification.trx`) are consumed by ReqStream to satisfy the OTS requirement.

### Requirements Coverage

- **BuildMark-OTS-WeasyPrint**: CI pipeline document generation evidence from multiple
  FileAssert TRX results confirming successful PDF rendering
