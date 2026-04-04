# ProcessRunner

## Overview

`ProcessRunner` is a static helper class that executes external shell commands and
captures their standard output. It provides two public methods: `RunAsync`, which
throws on failure, and `TryRunAsync`, which returns `null` on failure.

## Methods

### `RunAsync(command, arguments) → string`

Starts the specified process, captures stdout and stderr asynchronously, waits for
exit, and returns the trimmed stdout string. Throws `InvalidOperationException` if
the process exits with a non-zero exit code.

### `TryRunAsync(command, arguments) → string?`

Executes the process and returns the stdout string if the exit code is zero, or
`null` if the process fails or throws any exception. This method never propagates
exceptions to its callers.

## Interactions

| Unit / Subsystem       | Role                                                    |
|------------------------|---------------------------------------------------------|
| `RepoConnectorBase`    | Delegates `RunCommandAsync` to `ProcessRunner.RunAsync` |
| `RepoConnectorFactory` | Uses `TryRunAsync` to inspect the git remote URL        |
| `GitHubRepoConnector`  | Calls `RunCommandAsync` (via base) for git and gh CLI   |
