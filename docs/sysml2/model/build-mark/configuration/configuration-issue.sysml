package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ConfigurationIssue {
        doc /* Single issue with location and severity. */

        metadata info : SoftwareInfo {
            description = "Single issue with location and severity (requirements documented in the subsystem requirements file).";
            sourceFiles = ("src/DemaConsulting.BuildMark/Configuration/ConfigurationIssue.cs");
            designFiles = ("docs/design/build-mark/configuration/configuration-issue.md");
            verificationFiles = ("docs/verification/build-mark/configuration/configuration-issue.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
