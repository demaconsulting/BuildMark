package BuildMark {
    private import SoftwareItemMetadata::*;

    part def TemporaryDirectory {
        doc /* Temporary directory lifecycle management. */

        metadata info : SoftwareInfo {
            description = "Temporary directory lifecycle management.";
            sourceFiles = ("src/DemaConsulting.BuildMark/Utilities/TemporaryDirectory.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Utilities/TemporaryDirectoryTests.cs");
            designFiles = ("docs/design/build-mark/utilities/temporary-directory.md");
            verificationFiles = ("docs/verification/build-mark/utilities/temporary-directory.md");
            requirementFiles = ("docs/reqstream/build-mark/utilities/temporary-directory.yaml");
        }
    }
}
