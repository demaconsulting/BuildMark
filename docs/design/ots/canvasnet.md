## CanvasNet Integration Design

### Purpose

CanvasNet draws the pictures of the Pandoc documents. BuildMark uses it to draw the Mermaid
diagrams in the design document, so diagrams are written as text in Markdown and drawn when the
document is compiled.

### Features Used

- `pandoc-filter`, run through the Lua shim `docs/filters/canvasnet.lua`, which draws each
  fenced code block of class `mermaid` as a picture.
- `--validate --results <trx>` self-validation.

### Integration Pattern

CanvasNet is the dotnet local tool `DemaConsulting.CanvasNet.Tool`, restored from
`.config/dotnet-tools.json`.

- Every document definition (`docs/*/definition.yaml`) lists `docs/filters/canvasnet.lua` under
  `filters`. The shim is written by `dotnet canvasnet pandoc-filter --write-shim` and is not
  edited by hand.
- The shim sends each document to `canvasnet pandoc-filter` once; a diagram that cannot be drawn
  stops the build.
- The `build-docs` job runs `dotnet canvasnet --validate --results
  artifacts/canvasnet-self-validation.trx` before the documents are compiled.
