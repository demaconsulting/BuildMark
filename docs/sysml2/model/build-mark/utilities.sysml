package BuildMark {
    private import SoftwareItemMetadata::*;

    part def UtilitiesSubsystem {
        doc /* Shared utility helpers: path handling, process execution, and temp directories. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Utilities/UtilitiesTests.cs");
            designFiles = ("docs/design/build-mark/utilities.md");
            verificationFiles = ("docs/verification/build-mark/utilities.md");
            requirementFiles = ("docs/reqstream/build-mark/utilities.yaml");
        }

        part pathHelpers : PathHelpers;
        part processRunner : ProcessRunner;
        part temporaryDirectory : TemporaryDirectory;
    }
}
