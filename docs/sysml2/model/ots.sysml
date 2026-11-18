package OtsDependencies {
    private import SoftwareItemMetadata::*;

    doc /* OTS dependencies used by BuildMark. */

    part def YamlDotNet {
        doc /* OTS: YAML parsing library used to read/write .buildmark.yaml configuration. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/yaml-dot-net.md");
            verificationFiles = ("docs/verification/ots/yaml-dot-net.md");
            requirementFiles = ("docs/reqstream/ots/yaml-dot-net.yaml");
        }
    }

    part def GraphQLClient {
        doc /* OTS: GraphQL client library used to query the GitHub GraphQL API. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/graphql-client.md");
            verificationFiles = ("docs/verification/ots/graphql-client.md");
            requirementFiles = ("docs/reqstream/ots/graphql-client.yaml");
        }
    }
}
