package BuildMark {
    private import SoftwareItemMetadata::*;

    part def VersionSemantic {
        doc /* Semantic version with build metadata. */

        metadata info : SoftwareInfo {
            description = "Semantic version with build metadata.";
            sourceFiles = ("src/DemaConsulting.BuildMark/Version/VersionSemantic.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Version/VersionSemanticTests.cs");
            designFiles = ("docs/design/build-mark/version/version-semantic.md");
            verificationFiles = ("docs/verification/build-mark/version/version-semantic.md");
            requirementFiles = ("docs/reqstream/build-mark/version/version-semantic.yaml");
        }
    }
}
