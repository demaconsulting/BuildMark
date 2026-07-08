package BuildMark {
    part def CliSubsystem {
        doc /* Command-line argument parsing and I/O ownership. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/Cli/CliTests.cs */
        comment designRef /* Design: docs/design/build-mark/cli.md */
        comment verificationRef /* Verification: docs/verification/build-mark/cli.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/cli.yaml */

        part context : Context;
    }
}
