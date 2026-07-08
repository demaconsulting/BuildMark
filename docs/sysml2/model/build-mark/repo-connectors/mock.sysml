package BuildMark {
    part def MockSubsystem {
        doc /* Mock repository connector used for self-test. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/RepoConnectors/Mock/MockTests.cs */
        comment designRef /* Design: docs/design/build-mark/repo-connectors/mock.md */
        comment verificationRef /* Verification: docs/verification/build-mark/repo-connectors/mock.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/repo-connectors/mock.yaml */

        part mockRepoConnector : MockRepoConnector;
    }
}
