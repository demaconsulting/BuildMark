package BuildMark {
    private import SoftwareItemMetadata::*;

    part def VersionInterval {
        doc /* Single version interval model and parser. */

        metadata info : SoftwareInfo {
            description = "Single version interval model and parser.";
            sourceFiles = ("src/DemaConsulting.BuildMark/Version/VersionInterval.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Version/VersionIntervalTests.cs");
            designFiles = ("docs/design/build-mark/version/version-interval.md");
            verificationFiles = ("docs/verification/build-mark/version/version-interval.md");
            requirementFiles = ("docs/reqstream/build-mark/version/version-interval.yaml");
        }
    }
}
