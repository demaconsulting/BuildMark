package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ConnectorConfig {
        doc /* Connector envelope data model (requirements documented in the subsystem requirements file). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/ConnectorConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/connector-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/connector-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
