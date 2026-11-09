package BuildMark {
    private import SoftwareItemMetadata::*;

    part def GitHubGraphQLClient {
        doc /* GraphQL API client. */

        metadata info : SoftwareInfo {
            description = "GraphQL API client (tests split across 6 per-method test files).";
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/GitHub/GitHubGraphQLClient.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubGraphQLClientFindIssueIdsTests.cs",
                "test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubGraphQLClientGetAllIssuesTests.cs",
                "test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubGraphQLClientGetAllTagsTests.cs",
                "test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubGraphQLClientGetCommitsTests.cs",
                "test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubGraphQLClientGetPullRequestsTests.cs",
                "test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubGraphQLClientGetReleasesTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/github/github-graphql-client.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/github/git-hub-graph-ql-client.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/github/github-graphql-client.yaml");
        }
        // Note: verificationFiles uses a different kebab-case split than designFiles for the
        // same PascalCase name (pre-existing casing inconsistency, used verbatim, not fixed).
    }
}
