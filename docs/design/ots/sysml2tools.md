## SysML2Tools Integration Design

### Purpose

SysML2Tools checks the SysML2 architecture model under `docs/sysml2/` and renders its declared
views to the SVG diagrams that the design document embeds.

### Features Used

- `lint <globs>` to check the model for syntax and reference errors.
- `render --output <folder> --format svg <globs>` to render the declared views.
- `--validate --results <trx>` self-validation.

### Integration Pattern

SysML2Tools is the dotnet local tool `DemaConsulting.SysML2Tools.Tool`, restored from
`.config/dotnet-tools.json`.

- `lint.ps1` runs `dotnet sysml2tools lint 'docs/sysml2/**/*.sysml'`.
- The `build-docs` job runs `dotnet sysml2tools --validate --results
  artifacts/sysml2tools-self-validation.trx`, then renders the views in
  `docs/sysml2/views/design-views.sysml` to `docs/design/generated` before Pandoc compiles the
  design document.
