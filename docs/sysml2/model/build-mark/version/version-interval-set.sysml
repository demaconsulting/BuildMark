package BuildMark {
    private import SoftwareItemMetadata::*;

    part def VersionIntervalSet {
        doc /* Ordered set of version intervals. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Version/VersionIntervalSet.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Version/VersionIntervalSetTests.cs");
            designFiles = ("docs/design/build-mark/version/version-interval-set.md");
            verificationFiles = ("docs/verification/build-mark/version/version-interval-set.md");
            requirementFiles = ("docs/reqstream/build-mark/version/version-interval-set.yaml");
        }
    }
}
