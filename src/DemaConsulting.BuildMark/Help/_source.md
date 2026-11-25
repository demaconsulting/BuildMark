---
name: buildmark
schema: 2
title: BuildMark
summary: Built-in help of BuildMark, which writes Markdown build notes from GitHub or Azure DevOps.
---
{{tool}} reads the history of a Git repository hosted on GitHub or Azure DevOps, collects the pull
requests, issues, and work items since the previous version, and writes them as Markdown build
notes.

Every operation is an option. The only command is `{{tool}} help`, which must be the first argument
and takes topic names, such as `{{tool}} help command-line`.
