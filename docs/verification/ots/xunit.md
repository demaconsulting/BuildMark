# xUnit

## Verification Approach

xUnit is an OTS unit testing framework. Verification is achieved through the framework's
execution of all project unit tests in the CI pipeline. xUnit discovers and runs all test
methods in `DemaConsulting.BuildMark.Tests`, writes TRX result files, and reports test
outcomes. Successful test execution across all supported operating systems and .NET
runtime versions constitutes evidence that xUnit is functioning correctly.

## Evidence

The CI pipeline step `Test` executes:

```bash
dotnet test --no-build --configuration Release \
  --collect "XPlat Code Coverage;Format=opencover" \
  --logger "trx;LogFilePrefix=<os>" \
  --results-directory artifacts
```

The resulting TRX files are consumed by ReqStream to satisfy unit test requirements.
The matrix of operating systems (Windows, Ubuntu, macOS) and .NET versions (8, 9, 10)
provides broad platform coverage evidence.

## Requirements Coverage

- **BuildMark-OTS-xUnit**: CI pipeline test execution TRX evidence confirming that
  xUnit discovers and runs tests on all supported platforms
