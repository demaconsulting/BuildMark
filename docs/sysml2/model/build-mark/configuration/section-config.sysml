package BuildMark {
    private import SoftwareItemMetadata::*;

    part def SectionConfig {
        doc /* Report section definition data model. */

        metadata info : SoftwareInfo {
            description = "Report section definition data model (requirements documented in the subsystem requirements file).";
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/SectionConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/section-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/section-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
