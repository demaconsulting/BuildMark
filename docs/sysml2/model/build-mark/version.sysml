package BuildMark {
    private import SoftwareItemMetadata::*;

    part def VersionSubsystem {
        doc /* Version comparison, semantic versioning, tag parsing, and version intervals. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Version/VersionTests.cs");
            designFiles = ("docs/design/build-mark/version.md");
            verificationFiles = ("docs/verification/build-mark/version.md");
            requirementFiles = ("docs/reqstream/build-mark/version.yaml");
        }

        part versionComparable : VersionComparable;
        part versionSemantic : VersionSemantic;
        part versionTag : VersionTag;
        part versionInterval : VersionInterval;
        part versionIntervalSet : VersionIntervalSet;
        part versionCommitTag : VersionCommitTag;
    }
}
