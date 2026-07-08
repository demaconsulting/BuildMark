package BuildMark {
    part def VersionSubsystem {
        doc /* Version comparison, semantic versioning, tag parsing, and version intervals. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/Version/VersionTests.cs */
        comment designRef /* Design: docs/design/build-mark/version.md */
        comment verificationRef /* Verification: docs/verification/build-mark/version.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/version.yaml */

        part versionComparable : VersionComparable;
        part versionSemantic : VersionSemantic;
        part versionTag : VersionTag;
        part versionInterval : VersionInterval;
        part versionIntervalSet : VersionIntervalSet;
        part versionCommitTag : VersionCommitTag;
    }
}
