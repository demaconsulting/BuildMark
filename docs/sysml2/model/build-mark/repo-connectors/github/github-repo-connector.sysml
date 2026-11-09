package BuildMark {
    private import SoftwareItemMetadata::*;

    part def GitHubRepoConnector {
        doc /* GitHub API integration. */

        metadata info : SoftwareInfo {
            description = "GitHub API integration.";
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/GitHub/GitHubRepoConnector.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubRepoConnectorTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/github/github-repo-connector.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/github/git-hub-repo-connector.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/github/github-repo-connector.yaml");
        }
        // Note: verificationRef uses a different kebab-case split than designRef for the
        // same PascalCase name (pre-existing casing inconsistency, used verbatim, not fixed).
    }
}
