### BuildMarkConfig

#### Purpose

`BuildMarkConfig` is the top-level configuration data model for BuildMark. It holds all
settings read from the `.buildmark.yaml` file, including optional connector configuration,
optional report settings, ordered section definitions, and item routing rules. When no
`.buildmark.yaml` file is present, `Program` calls `BuildMarkConfig.CreateDefault()` to obtain
a configuration populated with built-in sections and routing rules.

#### Data Model

**Connector**: `ConnectorConfig?` — Optional connector configuration; `null` when the
`connector:` key is absent from `.buildmark.yaml`.

**Report**: `ReportConfig?` — Optional report output settings; `null` when the `report:` key
is absent from `.buildmark.yaml`.

**Sections**: `List<SectionConfig>` — Ordered list of report section definitions built from the
`sections:` YAML sequence; empty when the key is absent.

**Rules**: `List<RuleConfig>` — List of item routing rules built from the `rules:` YAML
sequence; empty when the key is absent.

#### Key Methods

**CreateDefault()**: Static factory that returns a `BuildMarkConfig` populated with built-in
section and routing rule definitions, used when no `.buildmark.yaml` file is present.

- *Parameters*: None.
- *Returns*: `BuildMarkConfig` — a new instance whose `Sections` list contains three entries
  (`changes`, `bugs-fixed`, `dependency-updates`) and whose `Rules` list contains six routing
  rules (dependency labels → `dependency-updates`; Bug work-item type → `bugs-fixed`; bug
  labels → `bugs-fixed`; internal/chore labels → `suppressed`; Task/Epic work-item types →
  `suppressed`; catch-all → `changes`). `Connector` and `Report` are `null`.
- *Preconditions*: None.
- *Postconditions*: The returned config has non-empty `Sections` and `Rules` lists; `Connector`
  and `Report` are `null`.

#### Error Handling

N/A — `BuildMarkConfig` is a configuration data record. No methods on this type detect or
propagate errors; all parsing and validation occur in `BuildMarkConfigReader`.

#### Dependencies

- **ConnectorConfig** — held by the `Connector` property when connector settings are present.
- **ReportConfig** — held by the `Report` property when report settings are present.
- **SectionConfig** — elements of the `Sections` list.
- **RuleConfig** — elements of the `Rules` list.

#### Callers

- **BuildMarkConfigReader** — creates instances by parsing `.buildmark.yaml` and returns them
  inside a `ConfigurationLoadResult`.
- **Program** — calls `CreateDefault()` when no config file is present; reads `Connector`,
  `Report`, `Sections`, and `Rules` to drive connector selection and report generation.
- **RepoConnectorBase** — receives `Rules` and `Sections` via `Configure(rules, sections)`.
- **RepoConnectorFactory** — receives `Connector` to select the appropriate connector
  implementation.
