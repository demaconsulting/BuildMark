package BuildMark {
    private import SoftwareItemMetadata::*;

    part def GitHubSubsystem {
        doc /* GitHub repository connector, GraphQL client, and GraphQL type definitions. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/github.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/github.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/github.yaml");
        }

        part gitHubRepoConnector : GitHubRepoConnector;
        part gitHubGraphQLClient : GitHubGraphQLClient;
        part gitHubGraphQLTypes : GitHubGraphQLTypes;
    }
}
