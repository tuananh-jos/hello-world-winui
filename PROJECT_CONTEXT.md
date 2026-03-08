1. Project Overview

Device Management Application is a desktop application (WinUI) used to manage devices on a computer.

The system allows:
- uc1: View model list
- uc2: Search / sort / filter devices
- uc3: Borrow devices
- uc4: View borrowed devices
- uc5: Search / sort / filter borrowed devices
- uc6: Return devices

The current goal is to build a solid 3-layer architecture (MVVM+clean architecture) foundation and complete basic features before implementing multi-instance synchronization.

2. Functional Use Cases
UC1 – View Models
- The system shall display a paginated list of device models.
- Requirements:
  + Show a list of models in a table/grid view.
  + Support pagination.
  + Display essential model information (e.g., Name, Manufacturer, Category, etc.).

UC2 – Search / Sort / Filter Models

- The system shall allow users to search, sort, and filter the model list.
- Search:
  + Search by Name
  + Search by Manufacturer
- Sort:
  + Sort by any visible column
- Filter:
  + Filter by Category
  + Filter by SubCategory
- UI Behavior:
  + Search and filter input controls are displayed directly below the table header.

UC3 – Borrow Device
- The system shall allow users to borrow devices from a selected model.
- Flow:
  + User clicks the Borrow button on a model item.
  + A modal dialog appears.
  + User selects the quantity of devices to borrow.
  + User confirms by clicking OK or cancels the operation.
- System Behavior:
  + Upon confirmation, the system updates device status accordingly.

UC4 – View My Devices
- The system shall display a list of devices currently borrowed by the user.
- Requirements:
  + Show all borrowed devices in a table/grid view.
  + Display relevant device-level information.

UC5 – Search / Sort / Filter My Devices
- The system shall allow users to search, sort, and filter the borrowed devices list.
- Search:
  + Search by:
    + ModelId
    + Name
    + IMEI
    + SerialLab
    + SerialNumber
    + CircuitSerialNumber
    + HWVersion
- Sort:
  + Sort by any visible column
- Filter:
  + Filter by HWVersion
- UI Behavior:
  + Search and filter input controls are displayed directly below the table header.

UC6 – Return Device
- The system shall allow users to return a borrowed device.
- Flow:
  + User clicks the Return button on a device item.
  + A confirmation popup/modal appears.
  + User clicks OK to confirm or Cancel to abort.
- System Behavior:
  + Upon confirmation, the system updates the device status to Available.

3. Scope Boundaries
- In Scope (Completed)
  + Device CRUD
  + Borrow / Return workflow
  + SQLite persistence
  + Local-only usage
  + Clean 3-layer architecture
  + Multi-instance synchronization (InstanceSyncService)
  + Concurrency protection between instances
  + UI polish (HandButton, WrapPanel, responsive sidebar)
  + Unit tests (NUnit + Moq, 22 tests)
- Out of Scope
  + Cloud sync
  + Authentication / multi-user system
  + Backend server
  + Cross-machine sync
  + Enterprise-scale optimization

4. Architecture Overview

The application is designed according to a 3-layer architecture:
- Presentation Layer (App7.Presentation)
  + View (Pages, Dialogs, Controls)
  + ViewModel
  + Custom Controls (HandButton, WrapPanel, PaginationControl, SmartDataTable, SmartDropdownPicker)
- Domain Layer (App7.Domain)
  + Entity (Model, Device)
  + UseCase (Borrow, Return, GetModelsPaged, GetBorrowedDevices, GetModelFilters)
  + IRepository (IUnitOfWork, IModelRepository, IDeviceRepository)
- Data Layer (App7.Data)
  + Repository (UnitOfWork, ModelRepository, DeviceRepository)
  + IDataSource
  + DataSource (ModelDataSource, DeviceDataSource)
  + DbContext (AppDbContext — SQLite + WAL mode)
- Test Layer (App7.Tests)
  + Unit tests for all UseCases
  + Performance tests (< 1s per UseCase)
  + Multi-instance concurrency tests

5. Current Implementation State

All features are implemented and tested:
- UseCase:
  + use cases 1 to 6: done
- InstanceSyncService: done
- Concurrency handling: done
- UI Polish: done
  + HandButton (hand cursor on hover for all buttons)
  + WrapPanel (pagination button wrapping)
  + Responsive sidebar (auto open/close based on window width)
  + Hamburger button hover effect removed
  + Disabled button opacity for unavailable models
- Unit Tests: done (22 tests, all passing)
  + UseCase logic tests (15)
  + Performance tests (5)
  + Multi-instance concurrency tests (2)
  + See docs/unit-tests.md for details

6. Database Initialization & Persistence

Database is initialized in .Data/Db/DatabaseInitializer.cs. This class is called in App.xaml.cs at the OnLaunched event.

Behavior:
- If database does not exist → create new
- If database exists but has no data → import from JSON file
- If JSON file does not exist → generate sample data

Database uses SQLite local file.
There is no migration system or versioning yet.

7. Multi-Instance Synchronization

Goal:
- Support multiple instances running on the same machine
- When one instance changes device status, the other instance must update UI

Design:
- InstanceSyncService uses file signal + polling + file watcher called after Borrow/Return success
- Implemented and functionally complete.

8. Non-Functional Goals (Target State)

- No double borrow between multiple instances
- UI is not blocked during polling
- No crash when SQLite is temporarily locked
- Keep clear separation between 3 layers

9. Technical Constraints

- Desktop WinUI
- SQLite
- No backend server
- Sync only on the same machine
- Small to medium scale
