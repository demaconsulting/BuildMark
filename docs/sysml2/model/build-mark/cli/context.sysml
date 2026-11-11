package BuildMark {
    private import SoftwareItemMetadata::*;

    part def Context {
        doc /* Command-line argument parser and I/O owner. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/Cli/Context.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Cli/ContextTests.cs");
            designFiles = ("docs/design/build-mark/cli/context.md");
            verificationFiles = ("docs/verification/build-mark/cli/context.md");
            requirementFiles = ("docs/reqstream/build-mark/cli/context.yaml");
        }
    }
}
