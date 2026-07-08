package BuildMark {
    part def GitHubGraphQLClient {
        doc /* GraphQL API client. */

        comment sourceRef /* Source: src/DemaConsulting.BuildMark/RepoConnectors/GitHub/GitHubGraphQLClient.cs */
        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/RepoConnectors/GitHub/GitHubGraphQLClient*Tests.cs (split across 6 per-method test files: FindIssueIds, GetAllIssues, GetAllTags, GetCommits, GetPullRequests, GetReleases) */
        comment designRef /* Design: docs/design/build-mark/repo-connectors/github/github-graphql-client.md */
        comment verificationRef /* Verification: docs/verification/build-mark/repo-connectors/github/git-hub-graph-ql-client.md */
        // Note: verificationRef uses a different kebab-case split than designRef for the
        // same PascalCase name (pre-existing casing inconsistency, used verbatim, not fixed).
        comment reqRef /* Requirements: docs/reqstream/build-mark/repo-connectors/github/github-graphql-client.yaml */
    }
}
