package BuildMark {
    private import SoftwareItemMetadata::*;

    part def AzureDevOpsConnectorConfig {
        doc /* Azure DevOps connector settings data model (requirements documented in the subsystem requirements file). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/AzureDevOpsConnectorConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/azure-devops-connector-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/azure-devops-connector-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
