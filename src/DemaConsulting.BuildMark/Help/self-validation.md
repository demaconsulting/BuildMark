---
title: Self-validation
summary: Prove that the installed tool works, and save the evidence as test results.
aliases:
  - validate
  - validation
tags:
  - reference
order: 7
see-also:
  - command-line
  - troubleshooting
examples:
  - name: run
    kind: command
    language: shell
    description: Run the self-tests. Prints one line per test and exits 0 when all pass.
    content: '{{tool}} --validate'
    expect: 'Failed: 0'
  - name: trx
    kind: command
    language: shell
    description: Also write the results as a TRX file.
    content: '{{tool}} --validate --results validation.trx'
    expect: 'Results written to validation.trx'
  - name: junit
    kind: command
    language: shell
    description: Write the results as JUnit XML instead.
    content: '{{tool}} --validate --results validation.xml'
  - name: depth
    kind: command
    language: shell
    description: Start the report at heading level 2 to embed it in a larger document.
    content: '{{tool}} --validate --depth 2'
    expect: '## DEMA Consulting BuildMark Self-validation'
  - name: bad-extension
    kind: command
    language: shell
    description: Any other extension is an error. Exits 1.
    content: '{{tool}} --validate --results validation.txt'
    exit: 1
    expect: 'Unsupported results file format'
---
`--validate` runs a short set of tests of the tool itself, so a regulated project can show that the
installed version works on its build machine. The tests write build notes from built-in sample data
in a temporary folder, with no network access and no token.

**Report.** The output is Markdown. It starts with the heading
`DEMA Consulting BuildMark Self-validation` and a table of the tool version, machine name, operating
system, .NET runtime, and time stamp. Each test prints one line, `✓ <name> - Passed` or
`✗ <name> - Failed: <reason>`, and the report ends with `Total Tests`, `Passed`, and `Failed` lines.
The exit code is 0 only when every test passes. `--depth` sets the level of the heading, as
[[#depth]] shows.

**Results files.** `--results <file>` also writes the outcome; the extension chooses the format.
`.trx` writes a Visual Studio TRX file and `.xml` writes JUnit XML, both readable by CI test-report
tasks. Any other extension is an error after the tests ran
([[#bad-extension]]). The folder of the file must exist.

**Evidence.** The tests have stable names, so a requirements file can list them as the tests that
prove the tool works in its environment:

- `BuildMark_MarkdownReportGeneration`
- `BuildMark_GitIntegration`
- `BuildMark_IssueTracking`
- `BuildMark_KnownIssuesReporting`
- `BuildMark_RulesRouting`
