package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ProcessRunner {
        doc /* Process runner for Git commands. */

        metadata info : SoftwareInfo {
            description = "Process runner for Git commands.";
            sourceFiles = ("src/DemaConsulting.BuildMark/Utilities/ProcessRunner.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Utilities/ProcessRunnerTests.cs");
            designFiles = ("docs/design/build-mark/utilities/process-runner.md");
            verificationFiles = ("docs/verification/build-mark/utilities/process-runner.md");
            requirementFiles = ("docs/reqstream/build-mark/utilities/process-runner.yaml");
        }
    }
}
