// Views rendered into docs/design/generated/ for the Design document.
// Reopens the BuildMark package so each 'expose' statement can reference
// its target part def directly. Per SysML2Tools syntax, 'expose' is only valid
// inside a named 'view' usage (not a 'view def' definition), and it - not
// 'render' - is what scopes the rendered diagram content.
package BuildMark {
    view SoftwareStructureView {
        expose BuildMark;
    }

    view BuildMarkView {
        expose BuildMarkSystem;
    }

    view CliView {
        expose CliSubsystem;
    }

    view BuildNotesView {
        expose BuildNotesSubsystem;
    }

    view SelfTestView {
        expose SelfTestSubsystem;
    }

    view UtilitiesView {
        expose UtilitiesSubsystem;
    }

    view VersionView {
        expose VersionSubsystem;
    }

    view ConfigurationView {
        expose ConfigurationSubsystem;
    }

    view RepoConnectorsView {
        expose RepoConnectorsSubsystem;
    }

    view GitHubView {
        expose GitHubSubsystem;
    }

    view AzureDevOpsView {
        expose AzureDevOpsSubsystem;
    }

    view MockView {
        expose MockSubsystem;
    }
}
