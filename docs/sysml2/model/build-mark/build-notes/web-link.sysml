package BuildMark {
    private import SoftwareItemMetadata::*;

    part def WebLink {
        doc /* Web link helper. */

        metadata info : SoftwareInfo {
            description = "Web link helper.";
            sourceFiles = ("src/DemaConsulting.BuildMark/BuildNotes/WebLink.cs");
            designFiles = ("docs/design/build-mark/build-notes/web-link.md");
            verificationFiles = ("docs/verification/build-mark/build-notes/web-link.md");
            requirementFiles = ("docs/reqstream/build-mark/build-notes/web-link.yaml");
        }
        // Note: no testRef - no dedicated test file exists (exercised only indirectly).
    }
}
