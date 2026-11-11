package BuildMark {
    private import SoftwareItemMetadata::*;

    import OtsDependencies::*;

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
    }
}
