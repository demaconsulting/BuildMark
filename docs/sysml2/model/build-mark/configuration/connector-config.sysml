package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ConnectorConfig {
        doc /* Connector envelope data model. */

        metadata info : SoftwareInfo {
            description = "Connector envelope data model (requirements documented in the subsystem requirements file).";
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/ConnectorConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/connector-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/connector-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testRef - no dedicated test file exists (exercised only indirectly).
    }
}
