---
title: Getting started
summary: Install the tool, check that it works, and write first build notes.
aliases:
  - start
  - quickstart
  - examples
tags:
  - tutorial
order: 1
see-also:
  - command-line
  - build-notes
  - connectors
examples:
  - name: install-local
    kind: snippet
    language: shell
    save-as: install-local.sh
    description: Add the tool to the tool manifest of a repository.
    content: |
      dotnet new tool-manifest
      dotnet tool install --local DemaConsulting.BuildMark
  - name: run-in-ci
    kind: snippet
    language: shell
    save-as: run-in-ci.sh
    description: Restore the manifest tools in CI, then run the tool.
    content: |
      dotnet tool restore
      dotnet {{tool}} --build-version v1.2.3 --report build-notes.md
  - name: version
    kind: command
    language: shell
    description: Print the version and exit.
    content: '{{tool}} --version'
  - name: validate
    kind: command
    language: shell
    description: Run the built-in self-tests. Exits 0 when every test passes.
    content: '{{tool}} --validate'
    expect: 'Failed: 0'
  - name: topic
    kind: command
    language: shell
    description: Print a help topic, here the list of options.
    content: '{{tool}} help command-line'
    expect: '--build-version'
---
{{tool}} {{version}} is a .NET tool.
It reads the history of the Git repository in the current folder, asks
GitHub or Azure DevOps for the pull requests, issues, and work items since
the previous version, and writes them as Markdown build notes.

1. **Install it.** The tool needs the .NET 10 SDK or later. In CI, set it
   up first when the runner has an older .NET, for example with
   `actions/setup-dotnet` and `dotnet-version: 10.x`.
   For a repository or CI, install it as a local tool. Run the commands
   that [[#install-local]] prints once, without
   `dotnet new tool-manifest` when `.config/dotnet-tools.json` exists, and
   commit that file. In CI, run `dotnet tool restore` first, then run the
   tool as `dotnet {{tool}}`, as in the commands that
   [[#run-in-ci]] prints. For one machine, install it globally and
   run it as `{{tool}}`:

   ```bash
   dotnet tool install -g DemaConsulting.BuildMark
   ```

2. **Check it.** The command that [[#version]] prints shows the version.
   The command from [[#validate]] runs the self-tests on built-in
   sample data, without network access, and exits 0 when all
   pass; see [[self-validation]].
3. **Write build notes.** In a clone of the repository, with an `origin`
   remote and an access token (see [[connectors]]), run:

   ```bash
   buildmark --build-version v1.2.3 --report build-notes.md
   ```

   [[build-notes]] says how the versions are chosen and what the report
   holds. An optional `.buildmark.yaml` file sets the sections and routing
   rules; see [[configuration]].

Print any help topic with `{{tool}} help <topic>`, as in the command that
[[#topic]] prints. Add `--search <words>` to find a topic, and `--json` to
get any help answer as JSON.
