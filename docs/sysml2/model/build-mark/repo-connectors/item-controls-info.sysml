package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ItemControlsInfo {
        doc /* Item controls data model (requirements documented in the subsystem requirements file). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/ItemControlsInfo.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/item-controls-info.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/item-controls-info.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors.yaml");
        }
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
