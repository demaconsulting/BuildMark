package BuildMark {
    private import SoftwareItemMetadata::*;

    part def AzureDevOpsRepoConnector {
        doc /* Azure DevOps API integration. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/AzureDevOps/AzureDevOpsRepoConnector.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/AzureDevOps/AzureDevOpsRepoConnectorTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/azure-devops/azure-devops-repo-connector.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/azure-devops/azure-devops-repo-connector.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/azure-devops/azure-devops-repo-connector.yaml");
        }
    }
}
