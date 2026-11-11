package BuildMark {
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
