package BuildMark {
    private import SoftwareItemMetadata::*;

    part def Program {
        doc /* Entry point and execution orchestrator. */

        metadata info : SoftwareInfo {
            description = "Entry point and execution orchestrator.";
            sourceFiles = ("src/DemaConsulting.BuildMark/Program.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/ProgramTests.cs");
            designFiles = ("docs/design/build-mark/program.md");
            verificationFiles = ("docs/verification/build-mark/program.md");
            requirementFiles = ("docs/reqstream/build-mark/program.yaml");
        }
    }
}
