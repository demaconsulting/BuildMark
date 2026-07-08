package BuildMark {
    import OtsDependencies::*;

    doc
    /*
     * BuildMark is a .NET command-line tool that generates markdown build notes from
     * Git repository metadata, including GitHub and Azure DevOps repositories.
     */

    part def BuildMarkSystem {
        doc /* The BuildMark system as a whole. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/IntegrationTests.cs */
        comment designRef /* Design: docs/design/build-mark.md */
        comment verificationRef /* Verification: docs/verification/build-mark.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark.yaml */

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
