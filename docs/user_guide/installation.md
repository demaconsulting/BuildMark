# Installation

## Prerequisites

BuildMark requires a supported .NET SDK version:

- .NET 8.0
- .NET 9.0
- .NET 10.0

Install the SDK from <https://dotnet.microsoft.com/download> if it is not already available on the
build machine.

## Global Installation

Install BuildMark as a global .NET tool for system-wide access:

```bash
dotnet tool install --global DemaConsulting.BuildMark
```

Verify the installation:

```bash
buildmark --version
```

## Local Installation

For team projects, install BuildMark as a local tool to keep the tool version aligned with the
repository:

```bash
# Create tool manifest if it does not exist
dotnet new tool-manifest

# Install the tool
dotnet tool install DemaConsulting.BuildMark
```

Run the locally installed tool:

```bash
dotnet buildmark --version
```

## Update

Update BuildMark to the latest version when needed:

```bash
# Global installation
dotnet tool update --global DemaConsulting.BuildMark

# Local installation
dotnet tool update DemaConsulting.BuildMark
```
