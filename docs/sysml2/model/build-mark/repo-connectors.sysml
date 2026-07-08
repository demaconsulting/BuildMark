package BuildMark {
    part def RepoConnectorsSubsystem {
        doc /* Repository connector abstraction, routing, and provider-specific integrations. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/RepoConnectors/RepoConnectorsTests.cs */
        comment designRef /* Design: docs/design/build-mark/repo-connectors.md */
        comment verificationRef /* Verification: docs/verification/build-mark/repo-connectors.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/repo-connectors.yaml */

        part iRepoConnector : IRepoConnector;
        part repoConnectorBase : RepoConnectorBase;
        part repoConnectorFactory : RepoConnectorFactory;
        part itemRouter : ItemRouter;
        part itemControlsInfo : ItemControlsInfo;
        part itemControlsParser : ItemControlsParser;
        part github : GitHubSubsystem;
        part azureDevOps : AzureDevOpsSubsystem;
        part mock : MockSubsystem;
    }
}
