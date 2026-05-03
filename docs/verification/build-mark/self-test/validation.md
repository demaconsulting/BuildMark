# Validation

## Verification Approach

`Validation` is tested indirectly through `ProgramTests.cs`. The test
`Program_Run_ValidateFlag_OutputsValidationMessage` invokes `Program.Run` with
`Validate = true`, which delegates to `Validation.Run`. Successful completion without
exception or non-zero exit code constitutes evidence that the validation framework
is operational.

The CI pipeline integration test `Run self-validation` also runs
`buildmark --validate --results artifacts/validation-*.trx` on each operating
system and .NET runtime combination, providing platform-level evidence.

## Dependencies

| Mock / Stub | Reason                                                |
| ----------- | ----------------------------------------------------- |
| `Context`   | Provides output stream for validation result messages |

## Test Scenarios

### Program_Run_ValidateFlag_OutputsValidationMessage

**Scenario**: `Program.Run` is called with `Validate = true`; control reaches
`Validation.Run`.

**Expected**: Validation output is written to the context output; exit code is 0.

**Requirement coverage**: `BuildMark-SelfTest-Validation`

## Requirements Coverage

- **BuildMark-SelfTest-Validation**: Program_Run_ValidateFlag_OutputsValidationMessage
