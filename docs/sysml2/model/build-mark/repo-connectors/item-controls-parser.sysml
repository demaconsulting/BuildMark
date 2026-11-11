package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ItemControlsParser {
        doc /* BuildMark control-block parser. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/ItemControlsParser.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/ItemControls/ItemControlsParserTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/item-controls-parser.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/item-controls-parser.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/item-controls-parser.yaml");
        }
    }
}
