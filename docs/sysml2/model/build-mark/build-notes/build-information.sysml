package BuildMark {
    private import SoftwareItemMetadata::*;

    part def BuildInformation {
        doc /* Build information data model. */

        metadata info : SoftwareInfo {
            description = "Build information data model.";
            sourceFiles = ("src/DemaConsulting.BuildMark/BuildNotes/BuildInformation.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/BuildNotes/BuildInformationTests.cs");
            designFiles = ("docs/design/build-mark/build-notes/build-information.md");
            verificationFiles = ("docs/verification/build-mark/build-notes/build-information.md");
            requirementFiles = ("docs/reqstream/build-mark/build-notes/build-information.yaml");
        }
    }
}
