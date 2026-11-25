---
title: Item controls
summary: Control visibility, type, and affected versions from issue and PR descriptions.
aliases:
  - controls
  - buildmark-block
  - visibility
  - affected-versions
  - known-issues
tags:
  - reference
order: 6
see-also:
  - build-notes
  - configuration
---
A `buildmark` block in the description of an issue, pull request, or work
item changes how {{tool}} reports that item. The block is a fenced code
block of three or more backticks whose info string is `buildmark` (any
case), for example:

````markdown
```buildmark
visibility: public
type: bug
affected-versions: (,1.0.1],[1.1.0,1.2.0)
```
````

The block may sit inside an HTML comment (`<!--` and `-->`) to hide it on
the web page. Only the first block counts. Each line is `key: value`; keys
ignore case, values are exact, and unknown keys or values are ignored.

**visibility.** `internal` leaves the item out of the report. `public`
keeps it, which is also the default.

**type.** `bug` or `feature` replaces the type that {{tool}} derives from
the labels or the work-item type, so the routing rules see the new type;
see [[configuration]].

**affected-versions.** A comma-separated list of intervals of the versions
that a bug affects:

- `[` and `]` include the bound, `(` and `)` exclude it
- an empty bound is open: `(,1.0.1]` is every version up to 1.0.1, and
  `[3.0.0,)` is 3.0.0 and later

**Known issues.** With `--include-known-issues` (see [[build-notes]]), the
report lists the bugs that this build does not fix. A bug with
`affected-versions` is listed when the range contains the build version,
open or closed. A bug without it is listed only while it is open.

**Azure DevOps fields.** The work-item fields `Custom.Visibility` and
`Custom.AffectedVersions` set the same controls, and win over the block.
