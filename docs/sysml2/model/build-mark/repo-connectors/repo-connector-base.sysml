package BuildMark {
    private import SoftwareItemMetadata::*;

    part def RepoConnectorBase {
        doc /* Repository connector base class. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/RepoConnectorBase.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/RepoConnectorBaseTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/repo-connector-base.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/repo-connector-base.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/repo-connector-base.yaml");
        }
    }
}
