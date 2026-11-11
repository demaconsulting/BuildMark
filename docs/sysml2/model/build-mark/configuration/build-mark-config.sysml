package BuildMark {
    private import SoftwareItemMetadata::*;

    part def BuildMarkConfig {
        doc /* Top-level configuration data model (requirements documented in the subsystem requirements file). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/BuildMarkConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/build-mark-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/build-mark-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
