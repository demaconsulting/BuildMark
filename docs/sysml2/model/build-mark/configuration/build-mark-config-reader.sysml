package BuildMark {
    private import SoftwareItemMetadata::*;

    part def BuildMarkConfigReader {
        doc /* Reads and parses .buildmark.yaml using YamlDotNet. */

        metadata info : SoftwareInfo {
            description = "Reads and parses .buildmark.yaml using YamlDotNet (requirements documented in the subsystem requirements file).";
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/BuildMarkConfigReader.cs");
            designFiles = ("docs/design/build-mark/configuration/build-mark-config-reader.md");
            verificationFiles = ("docs/verification/build-mark/configuration/build-mark-config-reader.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
