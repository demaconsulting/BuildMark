# SelfTest

## Verification Approach

The SelfTest subsystem is verified through the `Program_Run_ValidateFlag_OutputsValidationMessage`
test in `ProgramTests.cs` and through the CI pipeline integration test step
`Run self-validation`. Both confirm that the `Validation.Run` method can be invoked
without errors and produces output, providing evidence that all internal self-checks
pass in the test environment.

## Dependencies

| Mock / Stub | Reason                                                   |
| ----------- | -------------------------------------------------------- |
| `Context`   | Provides output capture for validation message assertion |

## Test Scenarios (Integration)

### Program_Run_ValidateFlag_OutputsValidationMessage

**Scenario**: `Program.Run` is called with `Validate = true`.

**Expected**: Validation framework runs; output is written; exit code is 0.

**Requirement coverage**: `BuildMark-SelfTest-Validation`, `BuildMark-Program-Validate`

## Requirements Coverage

- **BuildMark-SelfTest-Validation**: Program_Run_ValidateFlag_OutputsValidationMessage
- **BuildMark-Program-Validate**: Program_Run_ValidateFlag_OutputsValidationMessage
