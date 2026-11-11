package BuildMark {
    private import SoftwareItemMetadata::*;

    part def VersionTag {
        doc /* Repository tag parsing and normalization. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Version/VersionTag.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Version/VersionTagTests.cs");
            designFiles = ("docs/design/build-mark/version/version-tag.md");
            verificationFiles = ("docs/verification/build-mark/version/version-tag.md");
            requirementFiles = ("docs/reqstream/build-mark/version/version-tag.yaml");
        }
    }
}
