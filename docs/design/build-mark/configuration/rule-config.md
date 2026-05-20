### RuleConfig and RuleMatchConfig

#### Purpose

`RuleConfig` is a configuration data model representing a single item routing rule read from
the `rules:` list in `.buildmark.yaml`. Each rule carries match conditions and a destination
section ID. `RuleMatchConfig` holds the conditions that must be satisfied for a rule to fire.

Both types are defined in `Configuration/RuleConfig.cs`.

#### Data Model

##### RuleConfig

| Property | Type              | Description                                             |
|----------|-------------------|---------------------------------------------------------|
| `Match`  | `RuleMatchConfig` | Match conditions (labels, work-item types) for the rule |
| `Route`  | `string`          | Destination section `Id` for matched items              |

##### RuleMatchConfig

| Property       | Type            | Description                                                       |
|----------------|-----------------|-------------------------------------------------------------------|
| `Label`        | `IList<string>` | List of label values; rule matches when any label is present      |
| `WorkItemType` | `IList<string>` | List of work-item type values; rule matches when any type matches |

#### Error Handling

N/A — These are immutable configuration data records with no methods that detect or propagate errors.

#### Interactions

| Unit / Subsystem        | Role                                                                    |
|-------------------------|-------------------------------------------------------------------------|
| `BuildMarkConfig`       | Holds the list of `RuleConfig` objects in `Rules`                       |
| `BuildMarkConfigReader` | Parses the `rules:` YAML list and creates `RuleConfig` records          |
| `RepoConnectorBase`     | Receives the list via `Configure(rules, sections)`                      |
| `ItemRouter`            | Uses `RuleConfig` and `RuleMatchConfig` to route items to sections      |
