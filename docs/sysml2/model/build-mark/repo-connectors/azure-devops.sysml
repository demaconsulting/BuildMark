package BuildMark_AzureDevOpsSubsystem {
    public import BuildMark_AzureDevOpsApiTypes::*;
    public import BuildMark_AzureDevOpsRepoConnector::*;
    public import BuildMark_AzureDevOpsRestClient::*;
    public import BuildMark_WorkItemMapper::*;
    private import SoftwareItemMetadata::*;

    part def AzureDevOpsSubsystem {
        doc /* Azure DevOps repository connector, REST client, and work item mapping. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/AzureDevOps/AzureDevOpsTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/azure-devops.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/azure-devops.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/azure-devops.yaml");
        }

        part azureDevOpsRepoConnector : AzureDevOpsRepoConnector;
        part azureDevOpsRestClient : AzureDevOpsRestClient;
        part azureDevOpsApiTypes : AzureDevOpsApiTypes;
        part workItemMapper : WorkItemMapper;
    }
}
