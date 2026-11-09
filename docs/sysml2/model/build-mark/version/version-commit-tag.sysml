package BuildMark {
    private import SoftwareItemMetadata::*;

    part def VersionCommitTag {
        doc /* Version commit tag representation. */

        metadata info : SoftwareInfo {
            description = "Version commit tag representation.";
            sourceFiles = ("src/DemaConsulting.BuildMark/Version/VersionCommitTag.cs");
            designFiles = ("docs/design/build-mark/version/version-commit-tag.md");
            verificationFiles = ("docs/verification/build-mark/version/version-commit-tag.md");
            requirementFiles = ("docs/reqstream/build-mark/version/version-commit-tag.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
