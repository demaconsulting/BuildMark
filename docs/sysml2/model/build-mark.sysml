package BuildMark {
    public import BuildMark_BuildNotesSubsystem::*;
    public import BuildMark_CliSubsystem::*;
    public import BuildMark_ConfigurationSubsystem::*;
    public import BuildMark_Program::*;
    public import BuildMark_RepoConnectorsSubsystem::*;
    public import BuildMark_SelfTestSubsystem::*;
    public import BuildMark_UtilitiesSubsystem::*;
    public import BuildMark_VersionSubsystem::*;
    private import OtsDependencies::*;
    private import SoftwareItemMetadata::*;

    doc
    /*
     * BuildMark is a .NET command-line tool that generates markdown build notes from
     * Git repository metadata, including GitHub and Azure DevOps repositories.
     */

    part def BuildMarkSystem {
        doc /* The BuildMark system as a whole. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/IntegrationTests.cs");
            designFiles = ("docs/design/build-mark.md");
            verificationFiles = ("docs/verification/build-mark.md");
            requirementFiles = ("docs/reqstream/build-mark.yaml");
        }

        part program : Program;
        part cli : CliSubsystem;
        part buildNotes : BuildNotesSubsystem;
        part selfTest : SelfTestSubsystem;
        part utilities : UtilitiesSubsystem;
        part version : VersionSubsystem;
        part configuration : ConfigurationSubsystem;
        part repoConnectors : RepoConnectorsSubsystem;
        part yamlDotNet : YamlDotNet;
        part graphQLClient : GraphQLClient;
        part helpCards : HelpCards;
    }
}
