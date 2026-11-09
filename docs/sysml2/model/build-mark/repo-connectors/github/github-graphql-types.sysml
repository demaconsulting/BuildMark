package BuildMark {
    private import SoftwareItemMetadata::*;

    part def GitHubGraphQLTypes {
        doc /* GraphQL type definitions. */

        metadata info : SoftwareInfo {
            description = "GraphQL type definitions (requirements documented in the subsystem requirements file).";
            sourceFiles = ("src/DemaConsulting.BuildMark/RepoConnectors/GitHub/GitHubGraphQLTypes.cs");
            designFiles = ("docs/design/build-mark/repo-connectors/github/github-graphql-types.md");
            verificationFiles = ("docs/verification/build-mark/repo-connectors/github/git-hub-graph-ql-types.md");
            requirementFiles = ("docs/reqstream/build-mark/repo-connectors/github.yaml");
        }
        // Note: verificationFiles uses a different kebab-case split than designFiles for the
        // same PascalCase name (pre-existing casing inconsistency, used verbatim, not fixed).
        // Note: no testFiles - no dedicated test file exists (exercised only indirectly).
    }
}
