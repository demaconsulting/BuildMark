package BuildMark {
    private import SoftwareItemMetadata::*;

    part def ItemRouter {
        doc /* Shared item routing logic. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/ItemRouter.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/ItemRouterTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/item-router.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/item-router.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/item-router.yaml");
        }
    }
}
