# Project State Summary — Device Management Application
> **Last updated:** 2026-03-05 (Session 3 checkpoint)

---

## 1. Project Identity

| Field | Value |
|---|---|
| **App Name** | Device Management Application |
| **Tech Stack** | WinUI 3 (Windows App SDK), C#, MVVM + Clean Architecture |
| **UI Controls** | CommunityToolkit.WinUI.UI.Controls.DataGrid v7.1.2 |
| **Persistence** | SQLite via Entity Framework Core (`Microsoft.Data.Sqlite`) |
| **Solution File** | `App7.Presentation.sln` |
| **Project Root** | `c:\Users\Tai Khoan\Documents\hello-world-winui\` |

---

## 2. Architecture

```
Presentation Layer  →  App7.Presentation
  ├── Views/          (XAML pages + code-behind)
  ├── Views/Dialogs/  (BorrowDialog, ReturnDialog)
  └── ViewModels/     (MVVM via CommunityToolkit.Mvvm)
      └── PagedListViewModelBase (shared pagination base)

Domain Layer        →  App7.Domain
  ├── Entities/       (Model, Device)
  ├── UseCases/
  └── IRepository/    (interfaces only)

Data Layer          →  App7.Data
  ├── Db/             (AppDbContext, DatabaseInitializer, SampleDataGenerator)
  ├── DataSource/     (ModelDataSource, DeviceDataSource)
  └── Repository/     (ModelRepository, DeviceRepository)
```

**Key rule:** Domain has zero dependencies on Data or Presentation.

---

## 3. Domain Model

### Entity: `Model`
| Field | Type | Note |
|---|---|---|
| Id | Guid | PK |
| Name | string | |
| Manufacturer | string | |
| Category | string | |
| SubCategory | string | |
| Available | int | count of available devices |
| IsAvailable | bool (computed) | `Available > 0` — used for Borrow button disable |

### Entity: `Device`
| Field | Type | Note |
|---|---|---|
| Id | Guid | PK |
| ModelId | Guid | FK to Model |
| ModelName | string | `[NotMapped]` — populated by JOIN |
| IMEI | string | |
| SerialLab | string | |
| SerialNumber | string | |
| CircuitSerialNumber | string | |
| HWVersion | string | |
| Status | string | `"Available"` or `"Borrowed"` |

---

## 4. Feature Scope

| UC | Description | Status |
|---|---|---|
| UC1 | View paginated list of device models | ✅ **Done** |
| UC2 | Search / sort / filter models | ✅ **Done** |
| UC3 | Borrow device (modal + quantity + confirm) | ✅ **Done** |
| UC4 | View My Devices (borrowed devices list) | ✅ **Done** |
| UC5 | Search / sort / filter / paginate My Devices | ✅ **Done** |
| UC6 | Return device (custom ReturnDialog) | ✅ **Done** |
| — | Unit Tests (App7.Tests) | ⏳ Todo |
| — | InstanceSyncService (multi-instance signal) | ⏳ Post-MVP |

---

## 5. Key Files & Current State

### ViewModels
| File | Status | Notes |
|---|---|---|
| `ViewModels/PagedListViewModelBase.cs` | ✅ Complete | Shared base: pagination, search, sort, column visibility. `ShowingTextVisibility` always Visible. `EmptyVisibility` for empty state. Nav buttons disabled via CanExecute (FirstPage/PreviousPage/NextPage/LastPage — ALL four notified on CurrentPage change). |
| `ViewModels/ModelListViewModel.cs` | ✅ Complete | UC1+UC2. `ObservableCollection<Model> Models`. |
| `ViewModels/MyDevicesViewModel.cs` | ✅ Complete | UC4+UC5. `ObservableCollection<Device> Devices`. 6 search fields. |

### Views
| File | Status | Notes |
|---|---|---|
| `Views/ModelListPage.xaml` | ✅ Complete | Custom header + filter rows. DataGrid `Width="*"` star columns. `LayoutUpdated` syncs header/filter widths. `SelectionChanged` clears selection. Hover via `LoadingRow`. `ShowingText` footer always visible. Nav buttons always shown, disabled by CanExecute. Empty-state TextBlock overlay inside Grid.Row=2. PageSize popup Width=56. |
| `Views/ModelListPage.xaml.cs` | ✅ Complete | `OnSearchNameChanged`, `OnSearchKeyDown`, `OnSearchLostFocus` (search fires on Enter OR LostFocus). `OnGridLayoutUpdated` (All-zero bail, not Any). `GetNaturalWidth` returns Star. Hover: `LoadingRow → PointerEntered/Exited`. |
| `Views/MyDevicesPage.xaml` | ✅ Complete | Same pattern as ModelListPage. 6 search TextBoxes with Tag + TextChanged + KeyDown + LostFocus. Star columns. LayoutUpdated sync. Footer always visible. |
| `Views/MyDevicesPage.xaml.cs` | ✅ Complete | Same pattern. `OnSearchTextChanged` routes via Tag. `OnSearchLostFocus`. LayoutUpdated with 7-column sync. Hover. |
| `Views/Dialogs/BorrowDialog.xaml/.cs` | ✅ Complete | Custom ContentDialog: navy header, body with model info + quantity, footer buttons. |
| `Views/Dialogs/ReturnDialog.xaml/.cs` | ✅ Complete | Clone of BorrowDialog. Shows Model, IMEI, SerialLab, SerialNumber, CircuitSerial, HWVersion. Cancel + Confirm Return. Calls ReturnDeviceUseCase. |

### Data Layer
| File | Status | Notes |
|---|---|---|
| `DataSource/ModelDataSource.cs` | ✅ Complete | `GetPagedAsync` with case-insensitive search (`.ToLower().Contains(...ToLower())`), filter by Manufacturer/Category/SubCategory, sort, pagination. |
| `DataSource/DeviceDataSource.cs` | ✅ Complete | `GetBorrowedPagedAsync` with case-insensitive search on all 6 fields. Atomic `BorrowAsync` + `ReturnAsync` (transactions). |
| `Domain/Entities/Model.cs` | ✅ Complete | Added `IsAvailable => Available > 0` computed property. |

---

## 6. UI Patterns & Decisions

### DataGrid Setup (both pages)
- `HeadersVisibility="None"` — custom header row above grid
- `Width="*"` on all data columns + `MinWidth`
- `LayoutUpdated` event → reads `Columns[i].ActualWidth` → pushes to header/filter `ColumnDefinition.Width`
  - Bail condition: `colWidths.All(w => w <= 0)` (only skip if nothing rendered, not when some cols are hidden=0)
- `SelectionChanged` handler → `SelectedItem = null` (no row selection)
- `LoadingRow` → `PointerEntered/Exited` → `row.Background` for hover `#ecf3f8`
- Column visibility toggle: `SyncColumnVisibility(item)` sets `DataGridColumn.Visibility` + header/filter col `Width=0 or Width=Star`

### Pagination Footer
- `ShowingTextVisibility` = always `Visibility.Visible`
- `ShowingText` when TotalCount=0 → `"Showing 0 results"`
- Nav buttons (First/Previous/Next/Last) always shown, disabled via CanExecute
- `EmptyVisibility` → overlay TextBlock "No matching records found" inside grid area

### Search
- Trigger: Enter key (`OnSearchKeyDown`) OR LostFocus (`OnSearchLostFocus`)
- Case-insensitive: `.ToLower().Contains(term.ToLower())` at DB query level

### PageSize Popup
- Button `Width="56"` (both pages)
- Popup `Width="56"` (matches button)
- Items: "10", "20", "50", "100" (no "/page" suffix)

---

## 7. Non-Functional Status

| NFR | Target | Status |
|---|---|---|
| NFR1 | DB reads < 1s at 3M records | ✅ Indexes + pagination |
| NFR2 | Search/filter/sort < 1s | ✅ EF Core query |
| NFR3 | All DB ops async | ✅ |
| NFR4 | App startup < 3s | ✅ (assumed OK) |
| NFR5 | No crash on SQLite lock | ⏳ Not hardened |
| NFR6 | Atomic borrow/return | ✅ Transactions in DeviceDataSource |
| NFR7 | FileWatcher UI thread dispatch | ⏳ InstanceSyncService not started |

---

## 8. Remaining Work

### Option A — Unit Tests (`App7.Tests`)
- New project: xUnit + NSubstitute
- UseCases to test:
  - `GetModelsPagedUseCase` — search, filter, sort, pagination
  - `GetModelFiltersUseCase` — returns distinct Manufacturer/Category/SubCategory lists
  - `BorrowDeviceUseCase` — happy path, insufficient stock, concurrency
  - `GetBorrowedDevicesUseCase` — search, filter, pagination
  - `ReturnDeviceUseCase` — happy path, device not found
- Mock the repository interfaces with NSubstitute

### Option B — InstanceSyncService
- Purpose: when one instance borrows/returns, other open instances refresh automatically
- Mechanism: watch a `signal.txt` file (FileSystemWatcher)
- On change → raise event → UI thread dispatch → reload ViewModel data
- Files to create:
  - `App7.Domain/Services/IInstanceSyncService.cs`
  - `App7.Data/Services/InstanceSyncService.cs` (FileSystemWatcher impl)
  - Wire into `ModelListViewModel` + `MyDevicesViewModel` (call `ReloadAsync` on signal)
  - Register in DI container

### Low Priority
- BorrowDialog visual review (spacing, fonts) — probably fine as-is
- NFR5: SQLite lock graceful handling (retry on `DbUpdateException`)
- InfoBar layout/style polish

---

## 9. Commit History (Key)
| Commit | Feature |
|---|---|
| `7e86151` | UC1+UC2 — Model list: pagination, search, filter, sort |
| `46d2387` | UC3 — Borrow device dialog |
| `758848c` | UC4+UC5+UC6 — My Devices: list, search, filter, sort + Return |
| *(recent)* | DataGrid polish: hover, star columns, pagination fix, search case-insensitive, ReturnDialog |
