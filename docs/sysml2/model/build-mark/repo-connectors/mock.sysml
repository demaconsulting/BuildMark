package BuildMark {
    private import SoftwareItemMetadata::*;

    part def MockSubsystem {
        doc /* Mock repository connector used for self-test. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/Mock/MockTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/mock.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/mock.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/mock.yaml");
        }

        part mockRepoConnector : MockRepoConnector;
    }
}
