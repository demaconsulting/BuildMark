package BuildMark {
    private import SoftwareItemMetadata::*;

    part def PathHelpers {
        doc /* Safe path combination utilities. */

        metadata info : SoftwareInfo {
            description = "Safe path combination utilities.";
            sourceFiles = ("src/DemaConsulting.BuildMark/Utilities/PathHelpers.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Utilities/PathHelpersTests.cs");
            designFiles = ("docs/design/build-mark/utilities/path-helpers.md");
            verificationFiles = ("docs/verification/build-mark/utilities/path-helpers.md");
            requirementFiles = ("docs/reqstream/build-mark/utilities/path-helpers.yaml");
        }
    }
}
