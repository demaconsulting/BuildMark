# Security Policy

## Supported Versions

BuildMark follows semantic versioning. Security updates are provided for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| Latest  | :white_check_mark: |
| Previous| :white_check_mark: |
| Older   | :x:                |

We recommend always using the latest version to ensure you have the most recent security updates.

## Reporting a Vulnerability

If you discover a security vulnerability in BuildMark, please help us by reporting it responsibly.

### How to Report

**Do not** report security vulnerabilities through public GitHub issues.

Instead, please report security issues by:

1. Creating a private security advisory on GitHub
2. Emailing the maintainers with details (contact information available in the repository)

### What to Include

When reporting a vulnerability, please include:

- Description of the vulnerability
- Steps to reproduce the issue
- Affected versions
- Potential impact assessment
- Any suggested fixes (optional)

### Response Timeline

- **Initial Response**: Within 48 hours of report submission
- **Status Update**: Within 7 days with assessment and planned actions
- **Resolution**: Security patches released as quickly as possible based on severity

### Disclosure Policy

- Security vulnerabilities will be addressed privately
- Fixes will be prepared and tested before public disclosure
- Credit will be given to reporters unless they prefer to remain anonymous
- Public disclosure will occur after a fix is available

## Security Best Practices

When using BuildMark:

- Keep your .NET SDK updated to the latest supported version
- Review generated reports before publishing to ensure no sensitive data is included
- Use environment variables for any tokens or credentials (never commit them to repositories)
- Run BuildMark in trusted environments only
- Keep BuildMark updated to the latest version

## Known Security Considerations

BuildMark interacts with:

- Local file system for reading Git repositories and writing reports
- GitHub API for fetching issue and pull request information (requires authentication token)
- Git repositories for analyzing commit history

Users should ensure appropriate access controls are in place for:

- GitHub personal access tokens (use tokens with minimal required scopes)
- File system permissions for report output directories
- Repository access permissions

## Security Updates

Security updates will be:

- Released as patch versions when possible
- Documented in release notes with severity classification
- Announced through GitHub security advisories
- Available through standard NuGet package updates

## Contact

For security concerns or questions about this policy, please use GitHub's security advisory feature or contact
the project maintainers through the repository.
