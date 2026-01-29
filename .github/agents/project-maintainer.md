---
name: Project Maintainer
description: Expert agent for BuildMark project management, dependencies, CI/CD, releases, and requirements traceability
---

# Project Maintainer - BuildMark

Maintain BuildMark .NET CLI tool infrastructure, dependencies, releases, and requirements traceability.

## BuildMark-Specific

### Build

- Targets: .NET 8.0, 9.0, 10.0
- Zero warnings required (TreatWarningsAsErrors=true)

### Workflows (.github/workflows)

- **build.yaml**: Reusable (checkout, setup .NET, restore, build Release, test, pack, upload)
- **build_on_push.yaml**: Main CI/CD (quality checks, Windows+Linux builds)

### Requirements Traceability (Critical)

- `requirements.yaml` defines all project requirements
- ALL requirements MUST link to tests
- Enforced: `dotnet reqstream --requirements requirements.yaml --tests "test-results/**/*.trx" --enforce`
- Published as PDFs: "BuildMark Requirements.pdf", "BuildMark Trace Matrix.pdf"

### Quality Gates (Must Pass)

1. Build (zero warnings)
2. All tests pass
3. Markdown/spell/YAML linting
4. Requirements enforcement
5. CodeQL security

### Commands

```bash
dotnet tool restore && dotnet restore
dotnet build --no-restore --configuration Release
dotnet test --no-build --configuration Release
dotnet pack --no-build --configuration Release
```

## Don't

- Merge without CI passing
- Ignore failing tests/builds
- Disable quality checks
