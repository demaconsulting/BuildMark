### ProcessRunner

![Utilities Structure](../../generated/UtilitiesView.svg)

#### Purpose

`ProcessRunner` is a static helper class in the Utilities subsystem that executes external shell commands
and captures their standard output. It provides two public methods: `RunAsync`, which throws on failure,
and `TryRunAsync`, which returns `null` on failure. A private helper `CreateStartInfo` encapsulates
platform-specific process configuration.

#### Data Model

N/A — `ProcessRunner` is a static utility class with no instance state.

#### Key Methods

**`RunAsync`**: Starts the specified process, captures stdout and stderr asynchronously, waits for exit,
and returns the trimmed stdout string.

- *Parameters*: `string command` — the executable or script name; `params string[] arguments` — arguments
  added individually to `ProcessStartInfo.ArgumentList` for correct quoting.
- *Returns*: `Task<string>` — the trimmed stdout string on success.
- *Preconditions*: `command` is a valid executable name or path.
- *Postconditions*: Returns trimmed stdout if the process exits with code zero.

Throws `InvalidOperationException` if the process exits with a non-zero exit code or if the command is
not found (wraps `Win32Exception` into `InvalidOperationException` with a descriptive message).

**`TryRunAsync`**: Executes the process and returns the stdout string if the exit code is zero, or `null`
if the process fails or throws any exception.

- *Parameters*: `string command` — the executable or script name; `params string[] arguments` — arguments
  added individually to `ProcessStartInfo.ArgumentList`.
- *Returns*: `Task<string?>` — trimmed stdout on success, or `null` on any failure.
- *Preconditions*: None — any command string is accepted.
- *Postconditions*: Never throws; all exceptions are caught internally.

**`CreateStartInfo`** (private): Creates a `ProcessStartInfo` for the given command.

- *Parameters*: `string command` — the executable or script name; `string[] arguments` — the arguments
  array.
- *Returns*: `ProcessStartInfo` — configured with redirected stdout and stderr, no shell execution.

On Windows, non-empty commands are routed through `cmd /c` so that `.cmd` and `.bat` scripts (such as the
Azure CLI `az.cmd`) are resolved correctly by the Windows command interpreter. The command and all
arguments are added to `ProcessStartInfo.ArgumentList` for correct quoting. On non-Windows platforms, the
command is invoked directly without a shell wrapper. Empty or whitespace-only commands are not routed
through `cmd /c`, preserving the exception behavior for invalid commands.

#### Error Handling

`RunAsync` throws `InvalidOperationException` when the process exits with a non-zero exit code or when
the command is not found (wrapping `Win32Exception` with a descriptive message). `TryRunAsync` catches all
exceptions and returns `null` on any failure, never propagating exceptions to callers.

#### Dependencies

N/A — `ProcessRunner` depends only on the .NET BCL (`System.Diagnostics.Process`,
`System.Runtime.InteropServices.RuntimeInformation`, `System.ComponentModel.Win32Exception`).

#### Callers

- **`RepoConnectorBase`** — delegates `RunCommandAsync` to `ProcessRunner.RunAsync`
- **`RepoConnectorFactory`** — uses `TryRunAsync` to inspect the git remote URL
- **`GitHubRepoConnector`** — calls `RunCommandAsync` (via base) for git and gh CLI commands
- **`AzureDevOpsRepoConnector`** — calls `RunCommandAsync` (via base) for git and az CLI commands
