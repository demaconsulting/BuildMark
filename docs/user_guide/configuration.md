<!-- markdownlint-disable MD025 -->

# Configuration File

BuildMark can be configured with a `.buildmark.yaml` file placed in the repository root. This file
separates persistent repository settings from runtime arguments, simplifying CI invocations and
enabling version-controlled configuration.

Example `.buildmark.yaml`:

```yaml
# Repository Connector Settings
connector:
  # Type of repository
  type: github

  # GitHub settings
  github:
    url: https://github.mycompany.com   # optional; defaults to https://api.github.com
    repository: owner/repo

# Build Notes sections
sections:
  - id: changes
    title: Changes
  - id: bugs-fixed
    title: Bugs Fixed
  - id: dependency-updates
    title: Dependency Updates

# Item routing rules
rules:
  # Labels of 'dependencies', 'renovate', or 'dependabot' get routed to the 'dependency-updates' section
  - match:
      label: [dependencies, renovate, dependabot]
    route: dependency-updates

  # Bug work-items get routed to the 'bugs-fixed' section
  - match:
      work-item-type: [Bug]
    route: bugs-fixed

  # Labels of 'bug', 'defect', or 'regression' get routed to the 'bugs-fixed' section
  - match:
      label: [bug, defect, regression]
    route: bugs-fixed

  # Labels of 'internal' or 'chore' get suppressed
  - match:
      label: [internal, chore]
    route: suppressed

  # Task and Epic work-items get suppressed
  - match:
      work-item-type: [Task, Epic]
    route: suppressed

  # Everything else gets routed to the 'changes' section
  - route: changes
```

Example `.buildmark.yaml` for Azure DevOps:

```yaml
# Repository Connector Settings
connector:
  # Type of repository
  type: azure-devops

  # Azure DevOps settings
  azure-devops:
    url: https://dev.azure.com/myorg
    project: MyProject
    repository: MyRepo

# Build Notes sections
sections:
  - id: changes
    title: Changes
  - id: bugs-fixed
    title: Bugs Fixed

# Item routing rules
rules:
  # Bug work-items get routed to the 'bugs-fixed' section
  - match:
      work-item-type: [Bug]
    route: bugs-fixed

  # Task and Epic work-items get suppressed
  - match:
      work-item-type: [Task, Epic]
    route: suppressed

  # Everything else gets routed to the 'changes' section
  - route: changes
```

## Connector Settings

The `connector` section declares how BuildMark connects to source-control and work-item systems.

The `type` key selects the connector:

| Value | Description |
| :---- | :---------- |
| `github` | GitHub or GitHub Enterprise |
| `azure-devops` | Azure DevOps (cloud or on-premises) |

### GitHub connector settings

BuildMark resolves the GitHub access token automatically from `GH_TOKEN`, then
`GITHUB_TOKEN`, then `gh auth token`.

| Key | Required | Description |
| :-- | :------- | :---------- |
| `url` | No | Base URL of the GitHub instance. Defaults to `https://api.github.com`. |
| `repository` | Yes | Repository in `owner/repo` format. |

### Azure DevOps connector settings

BuildMark resolves the Azure DevOps access token automatically in this order:

1. `AZURE_DEVOPS_PAT` environment variable (Personal Access Token)
2. `AZURE_DEVOPS_TOKEN` environment variable (PAT or Entra ID token)
3. `AZURE_DEVOPS_EXT_PAT` environment variable (Azure DevOps CLI extension variable)
4. `SYSTEM_ACCESSTOKEN` environment variable (Azure Pipelines service connection)
5. `az account get-access-token` (Azure CLI, if logged in)

| Key | Required | Description |
| :-- | :------- | :---------- |
| `url` | Yes | Azure DevOps organization URL, e.g. `https://dev.azure.com/myorg`. |
| `project` | Yes | Azure DevOps project name. |
| `repository` | Yes | Repository name within the project. |

## Report Sections

The `sections` sequence defines which sections appear in the generated build notes and in what
order. Each entry has two keys:

| Key | Description |
| :-- | :---------- |
| `id` | Unique identifier used to reference this section in routing rules. |
| `title` | Human-readable heading that appears in the generated report. |

Sections are rendered in the order they are listed. Any section that receives no items is omitted
from the output.

## Item Routing Rules

The `rules` sequence controls how individual work items are categorized into report sections.
Rules are evaluated in order and the **first matching rule wins**.

Each rule may contain:

| Key | Description |
| :-- | :---------- |
| `match` | Optional. Criteria to test against each item (see below). |
| `route` | Required. The section `id` to place matched items in, or `suppressed` to exclude them. |

### Match criteria

| Criterion | Description |
| :-------- | :---------- |
| `label` | A label name or list of label names. Matches if the item carries any of the listed labels. |
| `work-item-type` | A work-item type name or list of names (e.g., `Bug`, `Task`, `Epic`). |

Multiple criteria within a single `match` block are combined with AND logic — the item must satisfy
all specified criteria to match that rule.

A rule with no `match` key is a **catch-all** and matches every item that has not already been
routed by an earlier rule. Place the catch-all last to act as a default.

### The `suppressed` route

Setting `route: suppressed` excludes matched items from the report entirely. Use this to hide
internal tasks, dependency-update noise, or any other items that should not appear in the published
build notes.

## Report Options

The optional `report` section sets default report generation options. CLI arguments
override any values set here.

| Key | Description |
| :-- | :---------- |
| `file` | Default output file path for the generated report. |
| `depth` | Markdown heading depth (default: 1). |
| `include-known-issues` | Set to `true` to include known issues by default. |

Example:

```yaml
report:
  file: docs/build_notes.md
  depth: 2
  include-known-issues: true
```

## Built-in Defaults

When no `.buildmark.yaml` file is present, BuildMark applies built-in default sections
and routing rules:

**Default sections:**

| Section ID | Title |
| :--------- | :---- |
| `changes` | Changes |
| `bugs-fixed` | Bugs Fixed |
| `dependency-updates` | Dependency Updates |

**Default routing rules (evaluated in order):**

1. Labels `dependencies`, `renovate`, or `dependabot` → `dependency-updates`
2. Work-item type `Bug` → `bugs-fixed`
3. Labels `bug`, `defect`, or `regression` → `bugs-fixed`
4. Labels `internal` or `chore` → `suppressed`
5. Work-item types `Task` or `Epic` → `suppressed`
6. Catch-all → `changes`
