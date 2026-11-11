package BuildMark {
    private import SoftwareItemMetadata::*;

    part def MockRepoConnector {
        doc /* Mock repository connector for self-test. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/Mock/MockRepoConnector.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/Mock/MockRepoConnectorTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/mock/mock-repo-connector.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/mock/mock-repo-connector.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/mock/mock-repo-connector.yaml");
        }
    }
}
