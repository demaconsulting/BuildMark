package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ConfigurationLoadResult {
        doc /* Holds config and any load issues (requirements documented in the subsystem requirements file). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/ConfigurationLoadResult.cs");
            designFiles = ("docs/design/build-mark/configuration/configuration-load-result.md");
            verificationFiles = ("docs/verification/build-mark/configuration/configuration-load-result.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
