---
title: Command-line options
summary: Every option of the tool, and the order in which a run handles them.
aliases:
  - options
  - flags
tags:
  - reference
order: 2
see-also:
  - build-notes
  - self-validation
  - troubleshooting
examples:
  - name: usage
    kind: command
    language: shell
    description: Print the short usage text. It ends with a pointer to this help.
    content: '{{tool}} --help'
    expect: "Run '{{tool}} help' for topics and examples."
  - name: silent-log
    kind: command
    language: shell
    description: Hide the console output and keep it in a log file.
    content: '{{tool}} --silent --log buildmark.log --version'
  - name: unknown-option
    kind: command
    language: shell
    description: An unknown argument is an error, printed even with --silent. Exits 1.
    content: '{{tool}} --silent --unknown'
    exit: 1
    expect: "Error: Unsupported argument '--unknown'"
---
Options start with `--` (or a single character after `-`) and may be given in any order.

| Option | Meaning |
| --- | --- |
| `-v`, `--version` | Print only the version. |
| `-?`, `-h`, `--help` | Print the short usage text. |
| `--silent` | Hide console output. Errors still set exit code 1. |
| `--log <file>` | Also write the output to a file. |
| `--build-version <version>` | The version to report on; see [[build-notes]]. |
| `--report <file>` | Write the build notes as a Markdown file. |
| `--depth <n>` | First heading level of a report, 1 to 6; default 1. |
| `--include-known-issues` | Add the Known Issues section to the report. |
| `--lint` | Check `.buildmark.yaml` and stop; see [[configuration]]. |
| `--validate` | Run the self-tests; see [[self-validation]]. |
| `--results <file>` | Write the self-test results as `.trx` or `.xml`. |

`--report-depth` is an older name of `--depth`, and `--result` of `--results`; both still work.

**Order.** `--version` wins over everything and prints only the version. Otherwise the tool prints a
banner, then handles the first of `--help`, `--validate`, `--lint`, or the build notes. The command
that [[#silent-log]] prints writes the version to the log file
only.

**Errors.** An unknown argument, or an option without its value, prints `Error: ...` and exits 1.
The command that [[#unknown-option]] prints shows this. These
errors are found before `--silent` and `--log` take effect, so they are always printed and never
logged.

**Help.** `help` is a command, not an option, and must be the first argument. `--silent` and `--log`
do not apply to it, and an unknown topic exits 2.
