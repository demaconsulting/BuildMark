---
title: Configuration file
summary: Sections, routing rules, and report defaults in the optional .buildmark.yaml.
aliases:
  - config
  - buildmark-yaml
  - sections
  - rules
  - routing
  - lint
tags:
  - reference
order: 4
see-also:
  - connectors
  - build-notes
  - troubleshooting
examples:
  - name: lint
    kind: command
    language: shell
    description: Check .buildmark.yaml in the current folder. Prints only the banner and exits 0 when it is valid.
    content: '{{tool}} --lint'
---
{{tool}} reads `.buildmark.yaml` from the current folder, normally the root
of the repository. The file is optional. Without it, the report has the
sections `changes`, `bugs-fixed`, and `dependency-updates`, and built-in
rules route dependency updates and bugs to them, leave out internal items
and tasks, and put everything else in `changes`. A file that exists
replaces these defaults completely, so a file without `rules` turns routing
off and the report falls back to fixed Changes and Bugs Fixed sections.

A complete file for a GitHub repository:

```yaml
---
connector:
  type: github
  github:
    repository: owner/repo

sections:
  - id: changes
    title: Changes
  - id: bugs-fixed
    title: Bugs Fixed
  - id: dependency-updates
    title: Dependency Updates

rules:
  - match:
      label: dependencies
    route: dependency-updates
  - match:
      label: bug
    route: bugs-fixed
  - match:
      label: internal
    route: suppressed
  - route: changes
```

**Keys.** The top-level keys are `connector` (see [[connectors]]),
`sections`, `rules`, and `report`. Any other key is an error.

**sections** is a list of entries with an `id` and a `title`, both
required. The report shows the sections in list order.

**rules** is a list of rules, each with an optional `match` and a `route`,
which is a section `id` or `suppressed` to leave the item out:

- the first rule that matches wins, and a rule without `match` matches
  every item
- an item that no rule matches goes to the first section
- `match` takes `label` and `work-item-type`, each one value or a list;
  when both are given, both must match
- both compare, ignoring case, with the item type that {{tool}} derives,
  not with the raw labels:
  - GitHub labels `bug` and `defect` give `bug`; `feature` and
    `enhancement` give `feature`; `dependencies`, `renovate`, and
    `dependabot` give `dependencies`; `internal` and `chore` give
    `internal`; `documentation`, `performance`, and `security` keep their
    names; any other label gives `other`
  - Azure DevOps types `Bug` and `Issue` give `bug`; `User Story`,
    `Feature`, and `Epic` give `feature`; other types keep their names
  - a `type:` item control replaces the derived type; see [[item-controls]]

**report** sets defaults that the command line overrides: `file` (the
report file), `depth` (a positive integer), and `include-known-issues`
(`true` or `false`).

**Lint.** `{{tool}} --lint` checks the file and stops ([[#lint]]). Each
problem prints as `<file>:<line>: Error: <message>`, and any problem
exits 1. A run that writes build notes makes the same check first and
stops on any problem.
