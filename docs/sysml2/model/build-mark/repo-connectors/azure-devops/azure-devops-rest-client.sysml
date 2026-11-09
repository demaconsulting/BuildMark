package BuildMark {
    private import SoftwareItemMetadata::*;

    part def AzureDevOpsRestClient {
        doc /* REST API client. */

        metadata info : SoftwareInfo {
            description = "REST API client.";
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/AzureDevOps/AzureDevOpsRestClient.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/AzureDevOps/AzureDevOpsRestClientTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/azure-devops/azure-devops-rest-client.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/azure-devops/azure-devops-rest-client.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/azure-devops/azure-devops-rest-client.yaml");
        }
    }
}
