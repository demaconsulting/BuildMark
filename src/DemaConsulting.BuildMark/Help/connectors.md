---
title: Connectors and tokens
summary: How BuildMark reaches GitHub or Azure DevOps, and where it gets its token.
aliases:
  - github
  - azure-devops
  - azure
  - tokens
  - token
  - authentication
tags:
  - reference
order: 5
see-also:
  - configuration
  - troubleshooting
examples:
  - name: github-step
    kind: snippet
    language: yaml
    save-as: buildmark-github-step.yaml
    description: A GitHub Actions step that writes the build notes with the token of the job.
    content: |
      - name: Write build notes
        env:
          GH_TOKEN: ${{ github.token }}
        run: dotnet buildmark --build-version ${{ github.ref_name }} --report build-notes.md
  - name: azure-step
    kind: snippet
    language: yaml
    save-as: buildmark-azure-step.yaml
    description: An Azure Pipelines step that writes the build notes with the token of the pipeline.
    content: |
      - script: dotnet buildmark --build-version $(Build.SourceBranchName) --report build-notes.md
        displayName: Write build notes
        env:
          SYSTEM_ACCESSTOKEN: $(System.AccessToken)
---
A connector reads the history of the repository from GitHub or Azure
DevOps. Both connectors run `git` to read the `origin` remote URL and the
current commit, so run {{tool}} in a clone that has an `origin` remote.

**Choice.** The first of these that applies picks the connector:

1. `connector.type: azure-devops` in `.buildmark.yaml` picks Azure DevOps
2. the `TF_BUILD` environment variable (set in Azure Pipelines) picks Azure
   DevOps
3. the `GITHUB_ACTIONS` or `GITHUB_WORKSPACE` environment variable picks
   GitHub
4. an `origin` URL that contains `dev.azure.com` or `visualstudio.com`
   picks Azure DevOps
5. otherwise GitHub

**GitHub keys** go below `connector.github` in `.buildmark.yaml`; every key
is optional:

- `repository: owner/repo`, or `owner` and `repo`; by default taken from
  the `origin` URL
- `url`: the API base URL for GitHub Enterprise; GraphQL is then at
  `<url>/api/graphql` (`<url>/graphql` for `api.github.com`)
- `token-variable`: the environment variable that holds the token

**GitHub token.** With `token-variable`, only that variable is read, and a
variable that is not set or empty is an error. Otherwise the token comes
from `GH_TOKEN`, then `GITHUB_TOKEN`, then `gh auth token`. The step that
[[#github-step]] prints passes the token of the job.

**Azure DevOps keys** go below `connector.azure-devops`; every key is
optional:

- `url` (the organization URL), `project`, and `repository`; by default
  taken from the `origin` URL
- `area-path`: the area path that known issues are taken from; by default
  the project, and `""` for the whole project without a filter
- `token-variable`: the environment variable that holds the token

**Azure DevOps token.** With `token-variable`, only that variable is read,
and a variable that is not set or empty is an error. Otherwise the token
comes from `AZURE_DEVOPS_PAT`, `AZURE_DEVOPS_TOKEN`,
`AZURE_DEVOPS_EXT_PAT`, or `SYSTEM_ACCESSTOKEN`, in that order, then from
`az account get-access-token`. The step that [[#azure-step]] prints passes
the token of the pipeline.
