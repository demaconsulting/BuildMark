package BuildMark {
    part def ConfigurationSubsystem {
        doc /* Configuration data models and .buildmark.yaml loading/parsing. */

        comment testRef /* Test: test/DemaConsulting.BuildMark.Tests/Configuration/ConfigurationSubsystemTests.cs */
        comment designRef /* Design: docs/design/build-mark/configuration.md */
        comment verificationRef /* Verification: docs/verification/build-mark/configuration.md */
        comment reqRef /* Requirements: docs/reqstream/build-mark/configuration.yaml */

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
