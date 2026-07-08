package BuildMark {
    part def AzureDevOpsSubsystem {
        doc /* Azure DevOps repository connector, REST client, and work item mapping. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/RepoConnectors/AzureDevOps/AzureDevOpsTests.cs */
        comment designRef /* Design: docs/design/build-mark/repo-connectors/azure-devops.md */
        comment verificationRef /* Verification: docs/verification/build-mark/repo-connectors/azure-devops.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/repo-connectors/azure-devops.yaml */

        part azureDevOpsRepoConnector : AzureDevOpsRepoConnector;
        part azureDevOpsRestClient : AzureDevOpsRestClient;
        part azureDevOpsApiTypes : AzureDevOpsApiTypes;
        part workItemMapper : WorkItemMapper;
    }
}
