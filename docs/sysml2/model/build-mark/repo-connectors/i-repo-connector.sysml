package BuildMark {
    private import SoftwareItemMetadata::*;

    part def IRepoConnector {
        doc /* Repository connector interface (the connector contract). */

        metadata info : SoftwareInfo {
            description = "Repository connector interface (the connector contract; documented in the subsystem design doc).";
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/IRepoConnector.cs");
            designFiles = ("docs/design/build-mark/repo-connectors.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/i-repo-connector.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
