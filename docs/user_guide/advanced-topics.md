<!-- markdownlint-disable MD025 -->

# Common Use Cases

## CI/CD Integration

Integrate BuildMark into your CI/CD pipeline to automatically generate build notes:

```yaml
# GitHub Actions example
- name: Generate Build Notes
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
    buildmark \
      --build-version ${{ inputs.version }} \
      --report docs/build-notes.md

- name: Upload Build Notes
  uses: actions/upload-artifact@v4
  with:
    name: build-notes
    path: docs/build-notes.md
```

```yaml
# Azure Pipelines example
- task: CmdLine@2
  displayName: Generate Build Notes
  env:
    SYSTEM_ACCESSTOKEN: $(System.AccessToken)
  inputs:
    script: |
      buildmark \
        --build-version $(GitVersion.SemVer) \
        --report docs/build-notes.md

- task: PublishBuildArtifacts@1
  displayName: Upload Build Notes
  inputs:
    pathToPublish: docs/build-notes.md
    artifactName: build-notes
```

## Release Documentation

Generate build notes for a specific release:

```bash
# Generate build notes for version 2.0.0
buildmark --build-version v2.0.0 --report release-notes-2.0.0.md --include-known-issues
```

## Integration Testing

Run self-validation tests in your CI/CD pipeline:

```bash
buildmark --validate --results validation-results.trx
```

## Automated Reporting

Generate timestamped build notes for archival purposes:

```bash
#!/bin/bash
# Generate timestamped build notes
VERSION=$(git describe --tags --abbrev=0)
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
buildmark --build-version "$VERSION" \
  --report "build-notes-${TIMESTAMP}.md" \
  --log "analysis-${TIMESTAMP}.log"
```

# Report Format

The generated markdown report includes the following sections:

## Build Report Header

The report begins with a title showing the version:

```markdown
# Build Report
```

## Version Information

Shows the current version, baseline version (previous version), and commit information:

```markdown
## Version Information

| Field | Value |
|-------|-------|
| **Version** | 1.2.3 |
| **Commit Hash** | abc123def456 |
| **Previous Version** | 1.2.0 |
| **Previous Commit Hash** | 789012abc345 |
```

## Changes

Lists all non-bug changes implemented in this build, extracted from GitHub pull requests and issues:

```markdown
## Changes

- [#42](https://github.com/owner/repo/pull/42): Add new feature X
- [#43](https://github.com/owner/repo/pull/43): Improve performance of Y
- [#44](https://github.com/owner/repo/issues/44): Update documentation
```

Each change entry includes:

- **Issue/PR number**: Linked to the GitHub issue or pull request
- **Description**: Title of the issue or pull request

## Bugs Fixed

Lists all bugs resolved in this build, extracted from issues labeled as bugs:

```markdown
## Bugs Fixed

- [#40](https://github.com/owner/repo/issues/40): Fix crash when Z is null
- [#41](https://github.com/owner/repo/issues/41): Correct validation logic
```

## Dependency Updates

When a Dependency Updates section is configured (included in the built-in defaults), dependency
changes from tools like Dependabot and Renovate are grouped separately:

```markdown
## Dependency Updates

- [#45](https://github.com/owner/repo/pull/45): Bump lodash from 4.17.20 to 4.17.21
- [#46](https://github.com/owner/repo/pull/46): Update NuGet packages
```

## Known Issues

When `--include-known-issues` is specified, lists currently open bugs:

```markdown
## Known Issues

- [#50](https://github.com/owner/repo/issues/50): Performance degradation on large datasets
- [#51](https://github.com/owner/repo/issues/51): UI glitch in dark mode
```

## Complete Changelog

Provides a link to the full changelog on GitHub comparing the baseline and current versions:

```markdown
## Complete Changelog

[View Full Changelog](https://github.com/owner/repo/compare/v1.2.0...v1.2.3)
```

# Version Selection Rules

BuildMark automatically determines which previous version to use as the baseline when generating build notes. This
section explains how BuildMark selects the baseline version for different scenarios.

## Pre-Release Versions

For pre-release versions (e.g., `1.2.3-beta.1`, `1.2.3-rc.1`), BuildMark picks the **previous tag (release or
pre-release) that has a different commit hash**.

This behavior handles cases where multiple pre-release tags point to the same commit (re-tagging scenarios), ensuring
the generated changelog shows actual code changes rather than an empty diff.

### Example: Pre-Release with Re-Tagged Commits

Consider the following tags:

- `1.1.2-rc.1` (commit hash: `a1b2c3d4`)
- `1.1.2-beta.2` (commit hash: `a1b2c3d4`)
- `1.1.2-beta.1` (commit hash: `734713bc`)

When generating build notes for `1.1.2-rc.1`:

1. BuildMark identifies that `1.1.2-beta.2` has the same commit hash (`a1b2c3d4`)
2. BuildMark skips `1.1.2-beta.2` since it would result in an empty changelog
3. BuildMark selects `1.1.2-beta.1` as the baseline (different commit hash: `734713bc`)

The generated build notes will show changes between `1.1.2-beta.1` and `1.1.2-rc.1`.

## Release Versions

For release versions (e.g., `1.2.3`), BuildMark picks the **previous release tag**, skipping all pre-release versions.

This ensures release notes compare against the previous stable release, showing the complete set of changes since the
last production release.

### Example: Release Skipping Pre-Releases

Consider the following tags:

- `1.1.2` (release)
- `1.1.2-rc.1` (pre-release)
- `1.1.2-beta.2` (pre-release)
- `1.1.2-beta.1` (pre-release)
- `1.1.1` (release)

When generating build notes for `1.1.2`:

1. BuildMark identifies `1.1.2` as a release version (no pre-release suffix)
2. BuildMark skips all pre-release tags (`1.1.2-rc.1`, `1.1.2-beta.2`, `1.1.2-beta.1`)
3. BuildMark selects `1.1.1` as the baseline (the previous release)

The generated build notes will show all changes between `1.1.1` and `1.1.2`, including changes from all the
pre-release versions.

## No Previous Version

If no previous version is found (e.g., generating build notes for the first release), BuildMark will build the
history from the beginning of the repository, showing all commits up to the specified version.

## Version Tag Format

BuildMark recognizes version tags with various formats:

- Simple format: `1.2.3`
- V-prefix: `v1.2.3`
- Custom prefixes: `ver-1.2.3`, `release_1.2.3`
- Path-based prefixes: `release/1.2.3`, `builds/release/1.2.3`
- Pre-release suffixes: `-alpha.1`, `-beta.2`, `-rc.1`, `.pre.1`
- Build metadata: `+build.123`, `+linux.x64`

Examples of recognized version tags:

- `1.0.0`, `v1.0.0`, `ver-1.0.0`
- `release/1.2.3`, `builds/release/1.2.3-beta.1+build.99`
- `2.0.0-beta.1`, `v2.0.0-rc.2`
- `1.2.3+build.456`, `v2.0.0-rc.1+linux`

# Best Practices

## Version Tagging

- **Use semantic versioning**: Follow the `vX.Y.Z` format for version tags
- **Tag releases consistently**: Ensure all releases are tagged in Git
- **Use annotated tags**: Create annotated tags with `git tag -a` for better metadata

## GitHub Integration

- **Store tokens securely**: Use environment variables or secret management systems
- **Use read-only tokens**: BuildMark only needs read access to the GitHub API
- **Don't commit tokens**: Never commit tokens to version control
- **Set appropriate rate limits**: Be aware of GitHub API rate limits

## Azure DevOps Integration

- **Use Personal Access Tokens**: Set `AZURE_DEVOPS_PAT` with **Code (Read)** and
  **Work Items (Read)** scopes
- **Azure Pipelines**: Pass `SYSTEM_ACCESSTOKEN` to the task and grant the pipeline
  permission to access the repository
- **Entra ID tokens**: Be aware that tokens expire after one hour; prefer PATs for
  long-running pipelines
- **On-premises**: Set the `url` in `.buildmark.yaml` to your Azure DevOps Server URL

## CI/CD Best Practices

- **Generate on every release**: Automate build notes generation for every release
- **Archive reports**: Save build notes as build artifacts for historical tracking
- **Use silent mode**: Suppress unnecessary output in automated scripts with `--silent`
- **Handle failures gracefully**: Use appropriate error handling in your CI/CD scripts

## Report Best Practices

- **Use meaningful filenames**: Include version numbers or timestamps in report filenames
- **Adjust header depth**: Use `--depth` when embedding reports in larger documents
- **Include known issues**: Use `--include-known-issues` for comprehensive release documentation
- **Combine with logging**: Use `--log` to capture detailed execution information

# Troubleshooting

## Git Repository Issues

**Problem**: Cannot find Git repository

**Solutions**:

- Verify you're running BuildMark from within a Git repository
- Ensure the `.git` directory exists in the current or parent directories
- Check file permissions on the Git repository

## Version Tag Issues

**Problem**: Cannot find version tags

**Solutions**:

- Verify version tags exist in the Git repository with `git tag`
- Ensure tags follow a recognizable version format (e.g., `v1.2.3` or `1.2.3`)
- Check if the specified build version exists as a tag

## GitHub API Issues

**Problem**: GitHub API rate limit exceeded or authentication failures

**Solutions**:

- Set the `GH_TOKEN` or `GITHUB_TOKEN` environment variable with a valid GitHub personal access token
- Verify the token has appropriate permissions (read access to repositories)
- Wait for the rate limit to reset (typically one hour)
- Use a GitHub token to increase rate limits from 60 to 5000 requests per hour

## Azure DevOps API Issues

**Problem**: Azure DevOps API authentication failures or access denied errors

**Solutions**:

- Set the `AZURE_DEVOPS_PAT` environment variable with a valid Personal Access Token
- Verify the PAT has **Code (Read)** and **Work Items (Read)** permissions
- In Azure Pipelines, ensure `SYSTEM_ACCESSTOKEN` is passed to the task and the pipeline
  has been granted permission to access the repository
- For Entra ID tokens, verify the token is not expired (they are valid for one hour)

## Report Generation Issues

**Problem**: Report file is not generated or is empty

**Solutions**:

- Check file permissions in the output directory
- Verify the output path is valid and accessible
- Ensure there's enough disk space
- Check the log output for specific error messages

## Validation Failures

**Problem**: Self-validation tests fail

**Solutions**:

- Update to the latest version of BuildMark
- Check if there are any known issues in the GitHub repository
- Report the issue with full validation output if problem persists

## Exit Codes

BuildMark uses the following exit codes:

- `0`: Success
- `1`: Error occurred

Use these exit codes in scripts for error handling:

```bash
#!/bin/bash
if buildmark --build-version v1.2.3 --report build-notes.md; then
  echo "Build notes generated successfully!"
else
  echo "Build notes generation failed!"
  exit 1
fi
```

# Additional Resources

- [GitHub Repository][github]
- [Issue Tracker][issues]
- [Security Policy][security]
- [Contributing Guide][contributing]
- [NuGet Package][nuget]

[github]: https://github.com/demaconsulting/BuildMark
[issues]: https://github.com/demaconsulting/BuildMark/issues
[security]: https://github.com/demaconsulting/BuildMark/blob/main/SECURITY.md
[contributing]: https://github.com/demaconsulting/BuildMark/blob/main/CONTRIBUTING.md
[nuget]: https://www.nuget.org/packages/DemaConsulting.BuildMark
