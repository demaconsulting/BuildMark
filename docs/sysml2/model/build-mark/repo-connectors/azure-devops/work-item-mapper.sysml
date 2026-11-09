package BuildMark {
    private import SoftwareItemMetadata::*;

    part def WorkItemMapper {
        doc /* Work item to ItemInfo mapper. */

        metadata info : SoftwareInfo {
            description = "Work item to ItemInfo mapper.";
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/AzureDevOps/WorkItemMapper.cs");
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/AzureDevOps/WorkItemMapperTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/azure-devops/work-item-mapper.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/azure-devops/work-item-mapper.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/azure-devops/work-item-mapper.yaml");
        }
    }
}
