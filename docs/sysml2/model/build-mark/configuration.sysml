package BuildMark_ConfigurationSubsystem {
    public import BuildMark_AzureDevOpsConnectorConfig::*;
    public import BuildMark_BuildMarkConfigReader::*;
    public import BuildMark_BuildMarkConfig::*;
    public import BuildMark_ConfigurationIssue::*;
    public import BuildMark_ConfigurationLoadResult::*;
    public import BuildMark_ConnectorConfig::*;
    public import BuildMark_GitHubConnectorConfig::*;
    public import BuildMark_ReportConfig::*;
    public import BuildMark_RuleConfig::*;
    public import BuildMark_RuleMatchConfig::*;
    public import BuildMark_SectionConfig::*;
    private import SoftwareItemMetadata::*;

    part def ConfigurationSubsystem {
        doc /* Configuration data models and .buildmark.yaml loading/parsing. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.BuildMark.Tests/Configuration/ConfigurationSubsystemTests.cs");
            designFiles = ("docs/design/build-mark/configuration.md");
            verificationFiles = ("docs/verification/build-mark/configuration.md");
            requirementFiles = ("docs/reqstream/build-mark/configuration.yaml");
        }

        part buildMarkConfig : BuildMarkConfig;
        part buildMarkConfigReader : BuildMarkConfigReader;
        part configurationLoadResult : ConfigurationLoadResult;
        part configurationIssue : ConfigurationIssue;
        part connectorConfig : ConnectorConfig;
        part gitHubConnectorConfig : GitHubConnectorConfig;
        part azureDevOpsConnectorConfig : AzureDevOpsConnectorConfig;
        part reportConfig : ReportConfig;
        part sectionConfig : SectionConfig;
        part ruleConfig : RuleConfig;
        part ruleMatchConfig : RuleMatchConfig;
    }
}
