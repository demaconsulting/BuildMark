## xUnit Integration Design

### Purpose

xUnit is the .NET unit-testing framework used by the `DemaConsulting.BuildMark.Tests` project.
It discovers and runs the test methods and reports their outcomes as TRX files, which ReqStream
consumes as requirements evidence. xUnit v3 was chosen because it supports self-hosted test
executables on `net10.0` and `net11.0` and integrates with the Microsoft Testing Platform TRX
reporter.

### Features Used

- Test discovery and execution of methods marked with `[Fact]` and `[Theory]`.
- The xUnit assertion library (`Assert.Equal`, `Assert.True`, `Assert.Throws`, and others).
- `TestContext.Current.CancellationToken` to cancel asynchronous file operations in tests.
- TRX result generation through `Microsoft.Testing.Extensions.TrxReport`, enabled by
  `dotnet test --report-trx`.

### Integration Pattern

xUnit is referenced only by the test project
(`test/DemaConsulting.BuildMark.Tests/DemaConsulting.BuildMark.Tests.csproj`) through the
`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, and
`Microsoft.Testing.Extensions.TrxReport` packages, with `Xunit` added as an implicit using. The
project sets `OutputType` to `Exe` and `IsTestProject` to `true` and targets `net10.0` and
`net11.0`.

`build.ps1` runs `dotnet test --report-trx --results-directory artifacts/tests`. The CI `build`
job runs `dotnet test --report-trx` with coverlet coverage across the Windows, Ubuntu, and macOS
matrix and writes one TRX file per OS, assembly, framework, and architecture into `artifacts`.

The production project never references the test project, so xUnit cannot flow into the
published BuildMark package. No `xunit.runner.json` file is used; default settings apply.
