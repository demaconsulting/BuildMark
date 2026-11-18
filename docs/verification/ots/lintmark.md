## LintMark

### Verification Approach

LintMark is an OTS build tool, verified in two ways in the CI pipeline. Its built-in
self-validation (`--validate`) checks only its version and help output, so it proves that the
pinned tool starts in the build environment but does not exercise any lint rule. Rule checking is
verified separately: after `lint.ps1`, the `quality-checks` job runs the pinned LintMark on a
conforming and a malformed Markdown fixture with the repository's `.lintmark.yaml`, captures each
output and exit code, and checks them with FileAssert.

### Evidence

The CI pipeline step `Run LintMark self-validation` in the `quality-checks` job executes:

```bash
dotnet lintmark --validate --results artifacts/lintmark-self-validation.trx
```

The step `Check LintMark rule detection` in the same job writes the fixtures to
`artifacts/lintmark-check/`, runs `dotnet lintmark` on each, appends `exit-code: <code>` to the
captured output, and executes:

```bash
dotnet fileassert --results artifacts/fileassert-lintmark.trx lintmark
```

Both TRX files are consumed by ReqStream to satisfy the LintMark OTS requirements.

### Test Scenarios

- *LintMark self-validation*: CI pipeline executes
  `dotnet lintmark --validate --results artifacts/lintmark-self-validation.trx`;
  expects exit code 0 and passing `LintMark_VersionDisplay` and `LintMark_HelpDisplay` results.
- *LintMark passes a conforming file*: A fixture with a correctly spaced heading and one
  paragraph is checked. The captured output must contain `exit-code: 0`. This scenario is tested
  by `LintMark_PassesConformingMarkdown`.
- *LintMark reports rule violations*: A fixture whose heading has no space after `#` and whose
  second line ends with trailing spaces is checked. The captured output must report
  `malformed.md:1:1: heading-missing-space` and `malformed.md:2:26: trailing-whitespace` and
  contain `exit-code: 1`. This scenario is tested by `LintMark_ReportsRuleViolations`.
