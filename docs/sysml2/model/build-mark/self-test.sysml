package BuildMark {
    part def SelfTestSubsystem {
        doc /* Self-validation test runner using the Mock repo connector. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/SelfTest/SelfTestTests.cs */
        comment designRef /* Design: docs/design/build-mark/self-test.md */
        comment verificationRef /* Verification: docs/verification/build-mark/self-test.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/self-test.yaml */

        part validation : Validation;
    }
}
