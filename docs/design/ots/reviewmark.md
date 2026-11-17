## ReviewMark Integration Design

### Purpose

ReviewMark tracks which files require formal review, groups them into review-sets, and checks
the review evidence store. It produces the Review Plan and Review Report documents that show
review coverage and currency for each release.

### Features Used

- `--plan <markdown>` and `--plan-depth <n>` to write the review plan.
- `--report <markdown>` and `--report-depth <n>` to write the review report.
- `--lint` to validate the `.reviewmark.yaml` configuration.
- `--validate --results <trx>` self-validation.

### Integration Pattern

ReviewMark is a dotnet local tool restored from `.config/dotnet-tools.json` and configured by
`.reviewmark.yaml`, which defines the `needs-review` file patterns, the review evidence source
(the `reviews` branch `index.json`), global context, and the review-sets.

- `lint.ps1` runs `dotnet reviewmark --lint`.
- The `build-docs` job runs `dotnet reviewmark --validate --results
  artifacts/reviewmark-self-validation.trx`, then `dotnet reviewmark --plan
  docs/code_review_plan/generated/plan.md --plan-depth 1 --report
  docs/code_review_report/generated/report.md --report-depth 1`.

The plan and report are compiled into PDFs by Pandoc and WeasyPrint. `--enforce` is not yet
passed because the review evidence store is still being populated.
