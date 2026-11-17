package BuildMark {
    private import SoftwareItemMetadata::*;

    part def RuleMatchConfig {
        doc /* Rule-match condition data model (companion record defined alongside RuleConfig in RuleConfig.cs). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/RuleConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/rule-match-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/rule-match-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
