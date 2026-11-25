---
title: Errors and exit codes
summary: What the exit codes mean, and how to fix the common error messages.
aliases:
  - errors
  - exit-codes
tags:
  - troubleshooting
order: 8
see-also:
  - command-line
  - configuration
  - connectors
examples:
  - name: silent-error
    kind: command
    language: shell
    description: An argument error is printed even with --silent. Exits 1.
    content: '{{tool}} --silent --unknown'
    exit: 1
    expect: "Error: Unsupported argument '--unknown'"
  - name: bad-config
    kind: command
    language: shell
    description: Check a .buildmark.yaml whose section says name instead of title. Exits 1.
    content: '{{tool}} --lint'
    exit: 1
    expect: "Error: Unsupported section key 'name'."
  - name: bad-log
    kind: command
    language: shell
    description: A log file in a folder that does not exist. Exits 1.
    content: '{{tool}} --log missing/buildmark.log --version'
    exit: 1
    expect: 'Failed to open log file'
  - name: unknown-topic
    kind: command
    language: shell
    description: A help topic that does not exist. Exits 2.
    content: '{{tool}} help no-such-topic'
    exit: 2
    expect: "Unknown help card 'no-such-topic'"
---
**Exit codes.**

| Code | Meaning |
| --- | --- |
| 0 | Success. |
| 1 | An error, a problem in `.buildmark.yaml`, or a failed self-test. |
| 2 | A wrong `help` argument or an unknown help topic. |

Errors are printed to standard error as `Error: ...` and set the exit code. Most errors found
during a run, such as a missing token, a version that cannot be determined, or a problem in
`.buildmark.yaml`, are hidden by `--silent` and kept in the `--log` file.
Errors in the arguments, and a log file that cannot be opened, are found before the run starts: they
are always printed and never reach the log file. The command that
[[#silent-error]] prints shows one despite `--silent`.

**Common messages.**

- `Unsupported argument`: check the spelling against [[command-line]]. `help` must come
  first.
- `requires a filename argument` and similar: the option before it needs a value.
- `must be between 1 and 6` or `requires a positive integer`: fix the value of `--depth`.
- `Failed to open log file`: the folder of `--log` does not exist or the file is locked
  ([[#bad-log]]).
- `<file>:<line>: Error: ...`: a problem in `.buildmark.yaml`, such as an unknown key, as the
  command that [[#bad-config]] prints shows; see
  [[configuration]].
- `Invalid build version format`: `--build-version` needs three numbers, such as `v1.2.3`.
- `Command 'git ...' failed`: the current folder is not a Git clone, or it has no `origin` remote.
- `Command 'git' was not found`: install Git and add it to the `PATH`.
- `Unable to get GitHub token` or `Unable to get Azure DevOps token`, and
  `token variable '...' is not set`: set the token; see [[connectors]].
- `No releases found`, `No version tags found`, or `current commit does not match`: add
  `--build-version`.
- `Failed to write report`: the folder of `--report` does not exist or the file is locked.
- `Unsupported results file format`: `--results` needs a `.trx` or `.xml` file.
- `Unknown help card`: the topic does not exist. The message lists the topics. The command that
  [[#unknown-topic]] prints shows the error.
- `Unexpected error: ...`: a failure that {{tool}} does not handle. It is always printed, never
  logged, and ends the process with an exit code from the .NET runtime instead of 1.
