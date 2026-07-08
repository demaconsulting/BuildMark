package BuildMark {
    part def BuildNotesSubsystem {
        doc /* Build notes data models: build information, item info, and web links. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/BuildNotes/BuildNotesTests.cs */
        comment designRef /* Design: docs/design/build-mark/build-notes.md */
        comment verificationRef /* Verification: docs/verification/build-mark/build-notes.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/build-notes.yaml */

        part buildInformation : BuildInformation;
        part itemInfo : ItemInfo;
        part webLink : WebLink;
    }
}
