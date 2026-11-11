package BuildMark {
    private import SoftwareItemMetadata::*;

    part def Validation {
        doc /* Self-validation test runner. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/SelfTest/Validation.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/SelfTest/ValidationTests.cs");
            designFiles = ("docs/design/build-mark/self-test/validation.md");
            verificationFiles = ("docs/verification/build-mark/self-test/validation.md");
            requirementFiles = ("docs/reqstream/build-mark/self-test/validation.yaml");
        }
    }
}
