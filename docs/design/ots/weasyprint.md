## WeasyPrint Integration Design

### Purpose

WeasyPrint, distributed as the `DemaConsulting.WeasyPrintTool` dotnet tool, renders the HTML
documents produced by Pandoc to PDF. The PDFs are the archived release documents. WeasyPrint was
chosen because it renders standard HTML and CSS, including paged-media rules in the shared
template, and can emit PDF/A for long-term archiving.

### Features Used

- Conversion of an input HTML file to an output PDF file.
- `--pdf-variant pdf/a-3u` to produce archival PDF/A-3u output.

### Integration Pattern

WeasyPrint is a dotnet local tool restored from `.config/dotnet-tools.json` and invoked by the
`build-docs` job as `dotnet weasyprint --pdf-variant pdf/a-3u <html> <pdf>` once per document
collection. The input is `docs/<collection>/generated/<name>.html` and the output is
`docs/generated/BuildMark <Document Name>.pdf`, which is uploaded as the `documents` artifact.
FileAssert then checks each PDF. No configuration file is used; styling comes from the shared
HTML template.
