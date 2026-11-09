package BuildMark {
    private import SoftwareItemMetadata::*;

    part def GitHubConnectorConfig {
        doc /* GitHub connector settings data model. */

        metadata info : SoftwareInfo {
            description = "GitHub connector settings data model (requirements documented in the subsystem requirements file).";
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/GitHubConnectorConfig.cs");
            designFiles = ("docs/design/build-mark/configuration/github-connector-config.md");
            verificationFiles = ("docs/verification/build-mark/configuration/git-hub-connector-config.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: verificationRef uses a different kebab-case split than designRef for the
        // same PascalCase name (pre-existing casing inconsistency, used verbatim, not fixed).
        // Note: no testRef - no dedicated test file exists (exercised only indirectly).
    }
}
