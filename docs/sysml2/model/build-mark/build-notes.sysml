package BuildMark {
    private import SoftwareItemMetadata::*;

    part def BuildNotesSubsystem {
        doc /* Build notes data models: build information, item info, and web links. */

        metadata info : SoftwareInfo {
            description = "Build notes data models: build information, item info, and web links.";
            testFiles = ("test/DemaConsulting.BuildMark.Tests/BuildNotes/BuildNotesTests.cs");
            designFiles = ("docs/design/build-mark/build-notes.md");
            verificationFiles = ("docs/verification/build-mark/build-notes.md");
            requirementFiles = ("docs/reqstream/build-mark/build-notes.yaml");
        }

        part buildInformation : BuildInformation;
        part itemInfo : ItemInfo;
        part webLink : WebLink;
    }
}
