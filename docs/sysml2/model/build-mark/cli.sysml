package BuildMark {
    private import SoftwareItemMetadata::*;

    part def CliSubsystem {
        doc /* Command-line argument parsing and I/O ownership. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Cli/CliTests.cs");
            designFiles = ("docs/design/build-mark/cli.md");
            verificationFiles = ("docs/verification/build-mark/cli.md");
            requirementFiles = ("docs/reqstream/build-mark/cli.yaml");
        }

        part context : Context;
    }
}
