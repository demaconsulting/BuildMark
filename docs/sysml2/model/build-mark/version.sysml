package BuildMark_VersionSubsystem {
    public import BuildMark_VersionCommitTag::*;
    public import BuildMark_VersionComparable::*;
    public import BuildMark_VersionIntervalSet::*;
    public import BuildMark_VersionInterval::*;
    public import BuildMark_VersionSemantic::*;
    public import BuildMark_VersionTag::*;
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
