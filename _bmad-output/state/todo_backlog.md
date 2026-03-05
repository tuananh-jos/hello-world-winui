# 📋 Device Management App — Backlog
> **Last updated:** 2026-03-05

---

## ✅ Core Use Cases (UC1–UC6) — ALL DONE

| UC | Feature | Status | Commit |
|---|---|---|---|
| UC1 | View paginated model list | ✅ Done | `7e86151` |
| UC2 | Search / filter / sort / paginate models | ✅ Done | `7e86151` |
| UC3 | Borrow device dialog (atomic) | ✅ Done | `46d2387` |
| UC4 | My Devices view (borrowed list) | ✅ Done | `758848c` |
| UC5 | Search / filter / sort / paginate My Devices | ✅ Done | `758848c` |
| UC6 | Return device (ReturnDialog, atomic) | ✅ Done | `758848c` + recent |

---

## ✅ UI Polish — ALL DONE (session 2026-03-05)

| Item | Status | Notes |
|---|---|---|
| DataGrid hover row color `#ecf3f8` | ✅ Done | `LoadingRow → PointerEntered/Exited` |
| No row / cell selection | ✅ Done | `SelectionChanged → SelectedItem = null` |
| Star-width columns (auto-expand) | ✅ Done | `Width="*"` on DataGrid columns |
| Header + filter rows sync with DataGrid | ✅ Done | `LayoutUpdated → Columns[i].ActualWidth` |
| Header/filter ẩn khi hide column | ✅ Done | Fixed bail: `All(w<=0)` not `Any` |
| Borrow button disabled khi Available=0 | ✅ Done | `Model.IsAvailable` computed property |
| ReturnDialog (custom, matches BorrowDialog) | ✅ Done | Full device info display |
| Pagination footer luôn visible | ✅ Done | `ShowingTextVisibility = Visible always` |
| Next/Last buttons disable ở trang cuối | ✅ Done | `_currentPage` notifies 4 commands |
| "No matching records" inside grid area | ✅ Done | Overlay TextBlock, EmptyVisibility |
| Search case-insensitive | ✅ Done | `.ToLower()` at EF Core query level |
| Search trigger: Enter + LostFocus | ✅ Done | Both pages |
| PageSize popup width = button width (56px) | ✅ Done | Both pages |

---

## 🔜 Remaining Work (2 options)

### Option A — Unit Tests
- **Project:** `App7.Tests` (xUnit + NSubstitute)
- **UseCases to cover:**
  - `GetModelsPagedUseCase`
  - `GetModelFiltersUseCase`
  - `BorrowDeviceUseCase` (happy path + insufficient stock + concurrency)
  - `GetBorrowedDevicesUseCase`
  - `ReturnDeviceUseCase`
- **Pattern:** Mock IRepository via NSubstitute, test business logic only

### Option B — InstanceSyncService
- **Purpose:** Other open app instances auto-refresh when one borrows/returns
- **Mechanism:** `signal.txt` file + `FileSystemWatcher`
- **Files to create:**
  - `App7.Domain/Services/IInstanceSyncService.cs`
  - `App7.Data/Services/InstanceSyncService.cs`
- **Wire into:** `ModelListViewModel` + `MyDevicesViewModel` → call `ReloadAsync()` on signal
- **DI:** Register in `App.xaml.cs` DI container

---

## 🐛 Known Low-Priority Issues

| Issue | Location | Priority |
|---|---|---|
| NFR5: SQLite lock graceful handling | DeviceDataSource | Low |
| InfoBar layout/style | Both pages | Low |
| BorrowDialog visual review (spacing) | BorrowDialog | Low |
