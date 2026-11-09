package BuildMark {
    private import SoftwareItemMetadata::*;

    part def SelfTestSubsystem {
        doc /* Self-validation test runner using the Mock repo connector. */

        metadata info : SoftwareInfo {
            description = "Self-validation test runner using the Mock repo connector.";
            testFiles = ("test/DemaConsulting.BuildMark.Tests/SelfTest/SelfTestTests.cs");
            designFiles = ("docs/design/build-mark/self-test.md");
            verificationFiles = ("docs/verification/build-mark/self-test.md");
            requirementFiles = ("docs/reqstream/build-mark/self-test.yaml");
        }

        part validation : Validation;
    }
}
