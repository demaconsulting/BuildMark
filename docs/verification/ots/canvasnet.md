## CanvasNet

### Verification Approach

CanvasNet is an OTS build tool. Verification is achieved through the tool's built-in
self-validation (`--validate`) executed in the CI pipeline, which writes its results to a TRX
file. The self-tests run the Pandoc filter protocol and draw Mermaid diagrams. CanvasNet is also
exercised by every document build, which stops if a diagram cannot be drawn.

### Evidence

The CI pipeline step `Run CanvasNet self-validation` in the `build-docs` job executes:

```bash
dotnet canvasnet --validate --results artifacts/canvasnet-self-validation.trx
```

The resulting TRX file is consumed by ReqStream to satisfy the CanvasNet OTS requirements.

### Test Scenarios

- *CanvasNet self-validation*: CI pipeline executes
  `dotnet canvasnet --validate --results artifacts/canvasnet-self-validation.trx`;
  expects exit code 0 and a TRX file in which every self-test passes.
