package BuildMark_CliSubsystem {
    public import BuildMark_Context::*;
    public import BuildMark_HelpCommand::*;
    private import SoftwareItemMetadata::*;

    part def CliSubsystem {
        doc /* Command-line argument parsing, I/O ownership, and the built-in help command. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Cli/CliTests.cs");
            designFiles = ("docs/design/build-mark/cli.md");
            verificationFiles = ("docs/verification/build-mark/cli.md");
            requirementFiles = ("docs/reqstream/build-mark/cli.yaml");
        }

        part context : Context;
        part helpCommand : HelpCommand;
    }
}
