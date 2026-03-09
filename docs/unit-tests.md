# Unit Test Documentation

## Overview

| Property | Value |
|----------|-------|
| **Project** | `App7.Tests` |
| **Framework** | NUnit 3.14 |
| **Mocking** | Moq 4.20 |
| **Target** | `net8.0` |
| **Total Tests** | 22 |
| **Run Command** | `dotnet test App7.Tests` |

---

## Test Structure

```
App7.Tests/
└── Domain/
    └── UseCases/
        ├── BorrowDeviceUseCaseTests.cs     (4 tests)
        ├── ReturnDeviceUseCaseTests.cs      (4 tests)
        ├── GetModelsPagedUseCaseTests.cs    (2 tests)
        ├── GetBorrowedDevicesUseCaseTests.cs(2 tests)
        ├── GetModelFiltersUseCaseTests.cs   (3 tests)
        ├── PerformanceTests.cs             (5 tests)
        └── MultiInstanceTests.cs           (2 tests)
```

---

## Test Categories

### 1. UseCase Logic Tests (15 tests)

Tests that verify orchestration logic of each UseCase using Moq mocks.

#### BorrowDeviceUseCaseTests
| Test | Verifies |
|------|----------|
| `ExecuteAsync_Success_CallsBorrowAndDecrement` | Correct methods called on success |
| `ExecuteAsync_Success_CallsInCorrectOrder` | Begin → Borrow → Decrement → Commit |
| `ExecuteAsync_BorrowThrows_RollsBackAndRethrows` | Rollback on BorrowAsync failure |
| `ExecuteAsync_DecrementThrows_RollsBackAndRethrows` | Rollback on DecrementAvailable failure |

#### ReturnDeviceUseCaseTests
| Test | Verifies |
|------|----------|
| `ExecuteAsync_Success_CallsReturnAndIncrement` | Correct methods called on success |
| `ExecuteAsync_Success_CallsInCorrectOrder` | Begin → Return → Increment → Commit |
| `ExecuteAsync_ReturnThrows_RollsBack` | Rollback on ReturnAsync failure |
| `ExecuteAsync_IncrementThrows_RollsBack` | Rollback on IncrementAvailable failure |

#### GetModelsPagedUseCaseTests
| Test | Verifies |
|------|----------|
| `ExecuteAsync_DelegatesCorrectRequest` | Request passed to repository unchanged |
| `ExecuteAsync_ReturnsRepositoryResult` | Result from repository returned as-is |

#### GetBorrowedDevicesUseCaseTests
| Test | Verifies |
|------|----------|
| `ExecuteAsync_DelegatesCorrectRequest` | Request passed to repository unchanged |
| `ExecuteAsync_ReturnsRepositoryResult` | Result from repository returned as-is |

#### GetModelFiltersUseCaseTests
| Test | Verifies |
|------|----------|
| `ExecuteAsync_CombinesAllFilterResults` | All 3 filter lists combined into response |
| `ExecuteAsync_CallsAllThreeMethods` | All 3 filter methods called exactly once |
| `ExecuteAsync_OneThrows_PropagatesException` | Exception from any filter method propagates |

### 2. Performance Tests (5 tests)

Verifies each UseCase completes within 1 second (NFR requirement).

| Test | Requirement |
|------|------------|
| `BorrowDeviceUseCase_CompletesUnder1Second` | < 1000ms |
| `ReturnDeviceUseCase_CompletesUnder1Second` | < 1000ms |
| `GetModelsPagedUseCase_CompletesUnder1Second` | < 1000ms |
| `GetBorrowedDevicesUseCase_CompletesUnder1Second` | < 1000ms |
| `GetModelFiltersUseCase_CompletesUnder1Second` | < 1000ms |

### 3. Multi-Instance / Concurrency Tests (2 tests)

Simulates concurrent access from multiple application instances.

| Test | Scenario |
|------|----------|
| `ConcurrentBorrow_BothSucceed_WhenEnoughStock` | 2 concurrent borrows with sufficient stock → both succeed |
| `ConcurrentBorrow_OneFailsGracefully_WhenInsufficientStock` | 2 concurrent borrows with only 1 available → exactly 1 fails with InvalidOperationException |

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| NUnit | 3.14.0 | Test framework |
| NUnit3TestAdapter | 4.5.0 | VS/dotnet test integration |
| Microsoft.NET.Test.Sdk | 17.9.0 | MSBuild test infrastructure |
| Moq | 4.20.70 | Mocking framework |

---

## Running Tests

```bash
# Run all tests
dotnet test App7.Tests

# Run with detailed output
dotnet test App7.Tests --logger "console;verbosity=detailed"

# Run specific test class
dotnet test App7.Tests --filter "FullyQualifiedName~BorrowDeviceUseCaseTests"
```

---

## Code Review Findings (2026-03-09)

Status: **All 22 tests passing**. No critical issues. Below are known limitations to address in a future iteration.

### Finding 1: Performance tests only measure UseCase logic overhead
- **File**: `PerformanceTests.cs`
- **Severity**: Low
- **Detail**: Mock repos return instantly (0ms), so tests only measure Moq + UseCase orchestration overhead (~1ms). They do NOT test real DB performance.
- **Action**: If end-to-end performance testing is needed, create integration tests using real SQLite DB.

### Finding 2: Multi-instance tests are deterministic, not true race conditions
- **File**: `MultiInstanceTests.cs`
- **Severity**: Low
- **Detail**: `Task.Run` + `lock` on shared state → threads serialize through the lock → always deterministic. Tests verify correct **logic** but never produce a real race condition.
- **Action**: Acceptable for unit tests. For true concurrency stress testing, use integration tests with real SQLite + multiple threads.

### Finding 3: BorrowDeviceUseCaseTests B-01 doesn't verify BeginTransactionAsync
- **File**: `BorrowDeviceUseCaseTests.cs`, test `ExecuteAsync_Success_CallsBorrowAndDecrement`
- **Severity**: Low
- **Detail**: Verifies `BorrowAsync`, `DecrementAvailableAsync`, `CommitAsync` — but missing `BeginTransactionAsync` verification. Already covered by B-02 (order test), but B-01 should have it for completeness.
- **Action**: Add `_unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);` to B-01.

