package BuildMark {
    private import SoftwareItemMetadata::*;

    part def AzureDevOpsApiTypes {
        doc /* REST API type definitions. */

        metadata info : SoftwareInfo {
            description = "REST API type definitions (requirements documented in the subsystem requirements file).";
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/AzureDevOps/AzureDevOpsApiTypes.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/azure-devops/azure-devops-api-types.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/azure-devops/azure-devops-api-types.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/azure-devops.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
