package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ReportConfig {
        doc /* Report output settings data model (requirements documented in the subsystem requirements file). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/ReportConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/report-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/report-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
