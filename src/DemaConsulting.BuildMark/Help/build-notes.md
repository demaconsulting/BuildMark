---
title: Build notes
summary: How a run picks the versions, and what the Markdown build notes contain.
aliases:
  - report
  - notes
  - release-notes
  - markdown
tags:
  - reference
order: 3
see-also:
  - configuration
  - connectors
  - item-controls
examples:
  - name: bad-version
    kind: command
    language: shell
    description: A version without three numbers is an error. Exits 1.
    content: '{{tool}} --build-version 1.2'
    exit: 1
    expect: 'Invalid build version format: 1.2'
  - name: bad-depth
    kind: command
    language: shell
    description: A depth outside 1 to 6 is an error. Exits 1.
    content: '{{tool}} --depth 7'
    exit: 1
    expect: 'must be between 1 and 6'
---
A run without `--help`, `--validate`, or `--lint` writes build notes for
the Git clone in the current folder. It first reads `.buildmark.yaml` and
stops when the file has errors; see [[configuration]]. Then it asks
GitHub or Azure DevOps for the history; see [[connectors]]. A typical
run in a pipeline:

```bash
buildmark --build-version v1.2.3 --report build-notes.md --depth 2 --include-known-issues
```

**Version.** `--build-version` takes a version tag such as `1.2.3`,
`v1.2.3`, `release/1.2.3-rc.1`, or `1.2.3+build.5`: an optional prefix of
letters, `_`, `/`, or `-`, then three numbers, an optional pre-release, and
optional `+` metadata. Anything else is an error, as the command that
[[#bad-version]] prints shows. Without `--build-version`, GitHub uses the
newest release on the current branch when the current commit is its commit,
and Azure DevOps uses the newest version tag on the current commit.
Otherwise the run fails and asks for a version.

**Previous version.** The notes cover the changes since the
previous version:

- a release is compared with the newest earlier release,
  skipping pre-releases
- a pre-release is compared with the newest earlier version
  on a different commit
- with no earlier version, the history starts at the first commit

GitHub uses its releases on the current branch, and Azure DevOps its
version tags.

**Console.** The run prints `Build Version`, `Commit Hash`,
`Previous Version` and `Previous Commit Hash` (when there is a previous
version), and the counts `Changes`, `Bugs Fixed`, and `Known Issues`.

**Report file.** The report is written only with `--report <file>` or
`report.file` in `.buildmark.yaml`; the option wins. An existing file is
overwritten, and the run prints `Build report generated successfully.` The
folder of the file must exist.

**Layout.** The report has:

- a `Build Report` heading at level `--depth` (default 1); a value outside
  1 to 6 is an error ([[#bad-depth]])
- a `Version Information` table of the version, commit hash, previous
  version, and previous commit hash, with `N/A` for a first release
- one subsection per configured section, in order, one level deeper; an
  empty section shows `- N/A`
- a `Known Issues` subsection with `--include-known-issues` or
  `report.include-known-issues: true`; see [[item-controls]]
- a `Full Changelog` subsection with a link to the comparison,
  when one is available

Each item is one line: a link labeled with the item number, a dash,
and the title.
