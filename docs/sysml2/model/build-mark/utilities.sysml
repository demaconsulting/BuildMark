package BuildMark {
    part def UtilitiesSubsystem {
        doc /* Shared utility helpers: path handling, process execution, and temp directories. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/Utilities/UtilitiesTests.cs */
        comment designRef /* Design: docs/design/build-mark/utilities.md */
        comment verificationRef /* Verification: docs/verification/build-mark/utilities.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/utilities.yaml */

        part pathHelpers : PathHelpers;
        part processRunner : ProcessRunner;
        part temporaryDirectory : TemporaryDirectory;
    }
}
