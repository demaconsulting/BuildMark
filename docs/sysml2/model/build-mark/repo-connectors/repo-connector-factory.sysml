package BuildMark {
    private import SoftwareItemMetadata::*;

    part def RepoConnectorFactory {
        doc /* Repository connector factory. */

        metadata info : SoftwareInfo {
            description = "Repository connector factory.";
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/RepoConnectorFactory.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/RepoConnectorFactoryTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/repo-connector-factory.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/repo-connector-factory.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/repo-connector-factory.yaml");
        }
    }
}
