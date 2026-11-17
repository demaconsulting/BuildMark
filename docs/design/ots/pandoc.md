## Pandoc Integration Design

### Purpose

Pandoc, distributed as the `DemaConsulting.PandocTool` dotnet tool, assembles the Markdown files
of each document collection into a single HTML document. WeasyPrint then renders that HTML to
PDF. Pandoc was chosen because it assembles multi-file documents natively and supports a custom
HTML template, numbered sections, a table of contents, and Lua filters.

### Features Used

- `--defaults <definition.yaml>` to load the input files, resource paths, template, and options.
- `resource-path`, `input-files`, `template`, `table-of-contents`, `number-sections`, and
  `embed-resources` (Design and User Guide documents) settings.
- The `docs/filters/canvasnet.lua` Lua filter, which renders embedded diagrams.
- `--metadata version=<version>` and `--metadata date=<date>` for the title page.
- `--output <html>` to write the HTML document.

### Integration Pattern

Pandoc is a dotnet local tool restored from `.config/dotnet-tools.json` and invoked as
`dotnet pandoc` by the `build-docs` job once per document collection: Build Notes, Code Quality,
Review Plan, Review Report, Design, Verification, User Guide, Requirements, and Trace Matrix.
Each collection supplies `docs/<collection>/definition.yaml`, and all collections share the
`template.html` in `docs/template/`. For the Design document, the SysML2 structure diagrams are
rendered into `docs/design/generated` before Pandoc runs. Output HTML is written to
`docs/<collection>/generated/` and passed to WeasyPrint.
