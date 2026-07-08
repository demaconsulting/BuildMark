package BuildMark {
    part def GitHubConnectorConfig {
        doc /* GitHub connector settings data model. */

        comment sourceRef /* Source: src/DemaConsulting.BuildMark/Configuration/GitHubConnectorConfig.cs */
        comment designRef /* Design: docs/design/build-mark/configuration/github-connector-config.md */
        comment verificationRef /* Verification: docs/verification/build-mark/configuration/git-hub-connector-config.md */
        // Note: verificationRef uses a different kebab-case split than designRef for the
        // same PascalCase name (pre-existing casing inconsistency, used verbatim, not fixed).
        comment reqRef /* Requirements: docs/reqstream/build-mark/configuration.yaml (documented exception: no dedicated per-unit reqstream file exists for Configuration units) */
        // Note: no testRef - no dedicated test file exists (exercised only indirectly).
    }
}
