package BuildMark {
    private import SoftwareItemMetadata::*;

    part def RuleConfig {
        doc /* Routing rule data model. */

        metadata info : SoftwareInfo {
            description = "Routing rule data model (requirements documented in the subsystem requirements file).";
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/RuleConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/rule-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/rule-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
