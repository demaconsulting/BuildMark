package BuildMark {
    part def GitHubSubsystem {
        doc /* GitHub repository connector, GraphQL client, and GraphQL type definitions. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubTests.cs */
        comment designRef /* Design: docs/design/build-mark/repo-connectors/github.md */
        comment verificationRef /* Verification: docs/verification/build-mark/repo-connectors/github.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/repo-connectors/github.yaml */

        part gitHubRepoConnector : GitHubRepoConnector;
        part gitHubGraphQLClient : GitHubGraphQLClient;
        part gitHubGraphQLTypes : GitHubGraphQLTypes;
    }
}
