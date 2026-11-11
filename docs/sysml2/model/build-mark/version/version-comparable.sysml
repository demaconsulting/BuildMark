package BuildMark {
    private import SoftwareItemMetadata::*;

    part def VersionComparable {
        doc /* Core integer-based version comparison. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Version/VersionComparable.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Version/VersionComparableTests.cs");
            designFiles = ("docs/design/build-mark/version/version-comparable.md");
            verificationFiles = ("docs/verification/build-mark/version/version-comparable.md");
            requirementFiles = ("docs/reqstream/build-mark/version/version-comparable.yaml");
        }
    }
}
