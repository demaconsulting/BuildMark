# ProcessRunner

## Overview

`ProcessRunner` is a static helper class in the Utilities subsystem that executes
external shell commands and captures their standard output. It provides two public
methods: `RunAsync`, which throws on failure, and `TryRunAsync`, which returns
`null` on failure.

## Methods

### `RunAsync(command, params arguments) → string`

Starts the specified process, captures stdout and stderr asynchronously, waits for
exit, and returns the trimmed stdout string. Throws `InvalidOperationException` if
the process exits with a non-zero exit code or if the command is not found (wraps
`Win32Exception` into `InvalidOperationException` with a descriptive message).

The `arguments` parameter is a `params string[]` array. Each argument is added to
the `ProcessStartInfo.ArgumentList` collection so that the Process class handles
argument quoting correctly, rather than concatenating into a single string.

### `TryRunAsync(command, params arguments) → string?`

Executes the process and returns the stdout string if the exit code is zero, or
`null` if the process fails or throws any exception. This method never propagates
exceptions to its callers.

The `arguments` parameter is a `params string[]` array, matching the signature of
`RunAsync`.

### `CreateStartInfo(command, arguments) → ProcessStartInfo`

Private helper method that creates a `ProcessStartInfo` for the given command.

On Windows, non-empty commands are routed through `cmd /c` so that `.cmd` and
`.bat` scripts (such as the Azure CLI `az.cmd`) are resolved correctly by the
Windows command interpreter. The command and all arguments are added to
`ProcessStartInfo.ArgumentList` for correct quoting. On non-Windows platforms,
the command is invoked directly without a shell wrapper.

Empty or whitespace-only commands are not routed through `cmd /c`, preserving the
exception behavior for invalid commands.

## Interactions

| Unit / Subsystem              | Role                                                                   |
|-------------------------------|------------------------------------------------------------------------|
| `RepoConnectorBase`           | Delegates `RunCommandAsync` to `ProcessRunner.RunAsync`                |
| `RepoConnectorFactory`        | Uses `TryRunAsync` to inspect the git remote URL                       |
| `GitHubRepoConnector`         | Calls `RunCommandAsync` (via base) for git and gh CLI commands         |
| `AzureDevOpsRepoConnector`    | Calls `RunCommandAsync` (via base) for git and az CLI commands         |
