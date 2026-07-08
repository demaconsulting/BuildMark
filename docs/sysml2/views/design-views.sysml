// Views rendered into docs/design/generated/ for the Design document.
// Reopens the BuildMark package so each 'expose' statement can reference
// its target part def directly. Per SysML2Tools syntax, 'expose' is only valid
// inside a named 'view' usage (not a 'view def' definition), and it - not
// 'render' - is what scopes the rendered diagram content.
package BuildMark {
    view SoftwareStructureView {
        expose BuildMark;
        render asTreeDiagram;
    }

    view BuildMarkView {
        expose BuildMarkSystem;
        render asTreeDiagram;
    }

    view CliView {
        expose CliSubsystem;
        render asTreeDiagram;
    }

    view BuildNotesView {
        expose BuildNotesSubsystem;
        render asTreeDiagram;
    }

    view SelfTestView {
        expose SelfTestSubsystem;
        render asTreeDiagram;
    }

    view UtilitiesView {
        expose UtilitiesSubsystem;
        render asTreeDiagram;
    }

    view VersionView {
        expose VersionSubsystem;
        render asTreeDiagram;
    }

    view ConfigurationView {
        expose ConfigurationSubsystem;
        render asTreeDiagram;
    }

    view RepoConnectorsView {
        expose RepoConnectorsSubsystem;
        render asTreeDiagram;
    }

    view GitHubView {
        expose GitHubSubsystem;
        render asTreeDiagram;
    }

    view AzureDevOpsView {
        expose AzureDevOpsSubsystem;
        render asTreeDiagram;
    }

    view MockView {
        expose MockSubsystem;
        render asTreeDiagram;
    }
}
