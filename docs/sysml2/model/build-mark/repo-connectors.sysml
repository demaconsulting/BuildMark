package BuildMark_RepoConnectorsSubsystem {
    public import BuildMark_AzureDevOpsSubsystem::*;
    public import BuildMark_GitHubSubsystem::*;
    public import BuildMark_IRepoConnector::*;
    public import BuildMark_ItemControlsInfo::*;
    public import BuildMark_ItemControlsParser::*;
    public import BuildMark_ItemRouter::*;
    public import BuildMark_MockSubsystem::*;
    public import BuildMark_RepoConnectorBase::*;
    public import BuildMark_RepoConnectorFactory::*;
    private import SoftwareItemMetadata::*;

    part def RepoConnectorsSubsystem {
        doc /* Repository connector abstraction, routing, and provider-specific integrations. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/RepoConnectors/RepoConnectorsTests.cs");
            designFiles = ("docs/design/build-mark/repo-connectors.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors.yaml");
        }

        part iRepoConnector : IRepoConnector;
        part repoConnectorBase : RepoConnectorBase;
        part repoConnectorFactory : RepoConnectorFactory;
        part itemRouter : ItemRouter;
        part itemControlsInfo : ItemControlsInfo;
        part itemControlsParser : ItemControlsParser;
        part github : GitHubSubsystem;
        part azureDevOps : AzureDevOpsSubsystem;
        part mock : MockSubsystem;
    }
}
