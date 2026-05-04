<!-- markdownlint-disable MD025 -->

# Extended Item Controls

BuildMark supports an optional `buildmark` code block embedded in GitHub and Azure DevOps issue and
pull request descriptions. This block gives developers fine-grained control over visibility, type
classification, and affected-version ranges without requiring custom labels or special fields.

Azure DevOps work items additionally support native custom fields as an alternative mechanism for
the same controls.

## BuildMark Code Block Format

Add a fenced `buildmark` block to the issue or PR description:

````markdown
```buildmark
visibility: public
type: feature
affected-versions: (,1.0.1],[1.1.0,1.2.0)
```
````

All fields are optional. Unrecognized fields are ignored.

The block can be concealed inside an HTML comment if you do not want it rendered in the
GitHub UI:

````markdown
<!--
```buildmark
visibility: internal
```
-->
````

## Visibility Field

The `visibility` field explicitly overrides the default visibility rules for an item.

| Value | Behavior |
| :---- | :-------- |
| `public` | Force-include the item in build notes, overriding any default exclusion. |
| `internal` | Force-exclude the item from build notes, overriding any default inclusion. |

When the `visibility` field is absent, BuildMark applies its default rules: merged pull
requests and linked issues are included in generated build notes by default. Standard GitHub
issue labels (`bug`, `defect`, `feature`, and similar) are used to classify entries such as
bug fixes versus changes, but unlabeled or non-standard-labeled items may still be included.

### Visibility Examples

Include an item that would otherwise be excluded by default:

```yaml
visibility: public
```

Suppress an item even though it carries a `bug` label:

```yaml
visibility: internal
```

## Type Field

The `type` field overrides the item classification that BuildMark uses when placing the item
into the report.

| Value | Report Section |
| :---- | :------------- |
| `bug` | **Bugs Fixed** section |
| `feature` | **Changes** section |

The `type` field sets the item classification. Routing rules (configured in `.buildmark.yaml`) govern which
report section each classified item is placed in. The section names in the table above reflect the default
configuration only.

When `type` is absent, BuildMark infers the type from the GitHub issue or PR labels or from the Azure DevOps work-item type.

### Type Example

Override a pull request so it appears in the **Bugs Fixed** section regardless of its labels:

```yaml
type: bug
```

## Affected Versions Field

The `affected-versions` field records which software versions are affected by the change
using mathematical interval notation. Multiple intervals are separated by commas.

```text
affected-versions: (,1.0.1],[1.1.0,1.2.0),(1.2.5,2.0.0],[3.0.0,)
```

### Interval Syntax

| Symbol | Meaning |
| :----- | :------ |
| `[` | Inclusive lower bound |
| `(` | Exclusive lower bound |
| `]` | Inclusive upper bound |
| `)` | Exclusive upper bound |
| _(empty)_ lower bound | No minimum - all versions from the beginning |
| _(empty)_ upper bound | No maximum - all versions from the lower bound onward |

### Affected Version Examples

| Expression | Meaning |
| :--------- | :------ |
| `(,1.0.1]` | All versions up to and including `1.0.1` |
| `[1.1.0,1.2.0)` | From `1.1.0` up to (but not including) `1.2.0` |
| `(1.2.5,2.0.0]` | After `1.2.5` up to and including `2.0.0` |
| `[3.0.0,)` | `3.0.0` and all later versions |

### Multiple Ranges

Combine ranges with commas to express disjoint sets of affected versions:

```text
affected-versions: (,1.0.1],[1.1.0,1.2.0)
```

This matches all versions up to and including `1.0.1`, and also versions from `1.1.0` up to
(but not including) `1.2.0`.

### Known-Issue Inclusion Rules

When `--include-known-issues` is used to generate a report, BuildMark applies the following
four rules to determine whether each bug qualifies as a known issue for the current build
version:

1. A **closed** bug with **no declared `affected-versions`** is **not** a known issue.
2. An **open** bug with **no declared `affected-versions`** **is** a known issue.
3. A bug in **any state** (open or closed) whose `affected-versions` **contains** the build
   version **is** a known issue.
4. A bug in **any state** (open or closed) whose `affected-versions` **does not contain** the
   build version is **not** a known issue.

Rules 3 and 4 take precedence over the open/closed status whenever `affected-versions` is
declared. This matters for LTS branches: a bug may be closed in a later release but still
affects an older LTS branch. Setting `affected-versions` on that bug ensures it is correctly
reported as a known issue in LTS build notes even after it has been closed.

**Example:** A bug is fixed in v2.0.0 but affects all v1.x releases. Setting
`affected-versions: (,2.0.0)` ensures it appears in build notes for any v1.x version
regardless of its open/closed state.

Azure DevOps work items support the same visibility and version controls through native custom
fields, as an alternative to embedding `buildmark` code blocks in descriptions. When both a custom
field and a `buildmark` block are present, the custom field takes precedence.

| Custom Field | Equivalent | Description |
| :----------- | :--------- | :---------- |
| `Custom.Visibility` | `visibility` | Set to `public` or `internal` to override default visibility. |
| `Custom.AffectedVersions` | `affected-versions` | Version interval set using the same interval notation. |
