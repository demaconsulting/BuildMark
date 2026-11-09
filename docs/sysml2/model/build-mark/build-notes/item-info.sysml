package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ItemInfo {
        doc /* Item information data model. */

        metadata info : SoftwareInfo {
            description = "Item information data model.";
            sourceFiles = ("src/DemaConsulting.BuildMark/BuildNotes/ItemInfo.cs");
            designFiles = ("docs/design/build-mark/build-notes/item-info.md");
            verificationFiles = ("docs/verification/build-mark/build-notes/item-info.md");
            requirementFiles = ("docs/reqstream/build-mark/build-notes/item-info.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
