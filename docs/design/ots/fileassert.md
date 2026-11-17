## FileAssert Integration Design

### Purpose

FileAssert checks that each generated HTML and PDF document exists, is well formed, and contains
expected content. Its passing results are the evidence that Pandoc and
WeasyPrint produced correct output.

### Features Used

- Named tests in `.fileassert.yaml`, tagged by document group (`build-notes`, `code-quality`,
  `code-review`, `design`, `verification`, `user-guide`, `requirements`) and selected by tag on
  the command line.
- File count, HTML XPath query, text, and PDF metadata and page assertions.
- `--results <trx>` to write each assertion run as TRX evidence.
- `--validate --results <trx>` self-validation.

### Integration Pattern

FileAssert is a dotnet local tool restored from `.config/dotnet-tools.json` and configured by
`.fileassert.yaml`, which defines a Pandoc HTML test and a WeasyPrint PDF test for each document
collection. After WeasyPrint renders a collection, the `build-docs` job runs
`dotnet fileassert --results artifacts/fileassert-<collection>.trx <tag>`. Before ReqStream runs,
the job also runs `dotnet fileassert --validate --results artifacts/fileassert-self-validation.trx`.
ReqStream reads these TRX files as evidence. The `requirements` group runs after ReqStream and
only confirms that the final documents are well formed.
