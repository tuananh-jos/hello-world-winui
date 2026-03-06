---
stepsCompleted: [step-01-init, step-02-discovery, step-02b-vision, step-02c-executive-summary, step-03-success, step-04-journeys, step-05-domain-skipped, step-06-innovation-skipped, step-07-project-type, step-08-scoping, step-09-functional, step-10-nonfunctional, step-11-polish, step-12-complete]
completedAt: '2026-03-03T00:00:26+07:00'
inputDocuments:
  - PROJECT_CONTEXT.md
workflowType: 'prd'
classification:
  projectType: desktop_app
  domain: general
  complexity: low
  projectContext: brownfield
  platform: Windows only
  autoUpdate: false
  offlineMode: side-effect (SQLite local)
---

# Product Requirements Document — Device Management Application

**Project:** hello-world-winui-main
**Author:** mothy
**Date:** 2026-03-02
**Deadline:** 3 days

---

## Executive Summary

The Device Management Application is a Windows desktop tool (WinUI 3) for a single user to track the borrow and return of physical devices from a local inventory. It replaces error-prone manual tracking (Excel) with a reliable, offline-first application backed by SQLite — no network, no backend, no authentication required. Correctness of device state and speed of delivery are the primary values.

The application manages a catalog of device models, supports search/sort/filter on both models and borrowed devices, and enforces borrow/return workflows via modal confirmation dialogs. A planned InstanceSyncService will enable safe concurrent use across multiple app instances on the same machine.

### What Makes This Special

A pragmatic internal tool with one job: maintain accurate device inventory state with zero infrastructure overhead. Clean 3-layer architecture (MVVM + Clean Architecture) keeps data consistent in SQLite, runs fully offline, and is safely extensible with multi-instance synchronization when needed.

## Project Classification

| Field           | Value                        |
|-----------------|------------------------------|
| Project Type    | Desktop App (WinUI 3, Windows) |
| Domain          | General / Internal Tooling   |
| Complexity      | Low                          |
| Project Context | Brownfield (in progress)     |
| Platform        | Windows only                 |
| Persistence     | SQLite (local file)          |
| Network         | None                         |
| Authentication  | None                         |
| Delivery        | 3-day deadline               |

## Success Criteria

### User Success

- Model list loads paginated on app launch with no additional user action.
- Search, sort, and filter on both model list and my devices list respond within 1 second.
- Borrow and Return complete in ≤ 2 steps: trigger → confirm dialog.
- My Devices list reflects accurate device state immediately after every borrow/return — no stale data.

### Business Success

- UC1–UC6 fully operational before the 3-day deadline.
- Zero data corruption: no double-borrow, no phantom devices, no incorrect status transitions.

### Technical Success

- App does not crash when SQLite is temporarily locked by a concurrent instance.
- UI thread never blocked during any data operation.
- Domain layer has zero dependencies on Data or Presentation layers.
- Layer communication exclusively through defined interfaces (IRepository, IDataSource).

### Measurable Outcomes

- UC1–UC6: 100% functional, manually verified end-to-end.
- Borrow/Return cycle produces correct device status with no data loss.
- App initializes SQLite (create / import JSON / generate sample data) without errors on every launch.

## Product Scope

### MVP — Phase 1 (3-day delivery)

| Use Case | Description |
|---|---|
| UC1 | View paginated model list |
| UC2 | Search / sort / filter models (Name, Manufacturer, Category, SubCategory) |
| UC3 | Borrow device — modal + quantity selector + confirm |
| UC4 | View My Devices list |
| UC5 | Search / sort / filter my devices (multi-field; HWVersion filter) |
| UC6 | Return device — confirmation modal + status reset to Available |
| Bootstrap | DB init: create new / import from JSON / generate sample data |

### Phase 2 — Growth (Post-MVP)

- **InstanceSyncService**: Write `signal.txt` on borrow/return; FileWatcher triggers UI refresh across instances.
- **Concurrency protection**: Prevent double-borrow under multi-instance race conditions.

### Phase 3 — Vision

- SQLite schema versioning and migration system.
- In-app notification when device state changes from another running instance.

### Risk Mitigation

| Risk | Mitigation |
|---|---|
| SQLite lock under multi-instance | App must not crash on transient lock — graceful failure, stable process (MVP requirement) |
| UI freeze during DB operations | All SQLite calls are `async`/`await` — UI thread never blocked |
| 3-day deadline | Scope fixed at UC1–UC6. Zero scope creep permitted in MVP. |

## User Journeys

### Journey 1 — Browse & Borrow (Happy Path)

The user opens the app to pick up a test device. The app launches, SQLite initializes, and the model list renders paginated. The user types a model name, filters by Category, sorts by Manufacturer — the list narrows in real time. They click **Borrow** on the target model, select quantity `1` in the modal, and confirm. My Devices shows the borrowed device with full details.

**Capabilities revealed:** UC1 (paginated model list), UC2 (search/sort/filter models), UC3 (borrow modal + quantity), UC4 (My Devices view).

### Journey 2 — Return Device (Happy Path)

The user opens My Devices, searches by IMEI to locate a specific device, and clicks **Return**. A confirmation popup appears. They click OK. The device disappears from My Devices; its status resets to `Available` in SQLite.

**Capabilities revealed:** UC5 (My Devices search/sort/filter), UC6 (return confirmation modal), real-time list refresh.

### Journey 3 — Multi-Instance Sync on Borrow (Post-MVP)

Two app windows are open. The user borrows a device from Instance A. Upon confirmation, Instance A writes to `signal.txt`. Instance B's FileWatcher detects the change and automatically refreshes — the borrowed device no longer appears as available. No manual refresh needed.

**Capabilities revealed:** `signal.txt` write on borrow (FR30), FileWatcher-triggered UI refresh (FR32, FR33).

### Journey 4 — Fresh Install / Empty Database

First launch on a new machine. No SQLite file exists. `DatabaseInitializer` creates the database, checks for a JSON import file — imports if found, generates sample data if not. UC1 loads immediately with no manual setup.

**Capabilities revealed:** DB bootstrap logic (FR26–FR29), seamless first-run experience.

### Journey 5 — Multi-Instance Sync on Return (Post-MVP)

Instance B shows model X with `Available = 0`. From Instance A, the user returns a device from model X. Instance A writes to `signal.txt`. Instance B's FileWatcher detects the change and refreshes — model X now shows `Available = 1` (++). No user action required on Instance B.

**Capabilities revealed:** `signal.txt` write on return (FR31), FileWatcher updates model list available count (FR33).

### Journey Requirements Summary

| Capability | Journeys |
|---|---|
| Paginated model list | J1 |
| Search / sort / filter — models | J1 |
| Borrow modal + quantity selector | J1 |
| My Devices list | J1, J2 |
| Search / sort / filter — my devices | J2 |
| Return confirmation modal | J2 |
| `signal.txt` write on borrow/return | J3, J5 |
| FileWatcher → auto UI refresh | J3, J5 |
| DB bootstrap / sample data generation | J4 |

## Platform Requirements

- **Platform**: Windows only. No cross-platform support planned.
- **Framework**: WinUI 3 (Windows App SDK), MVVM pattern throughout.
- **Persistence**: SQLite via `Microsoft.Data.Sqlite`. `DatabaseInitializer` runs at `OnLaunched`.
- **Update**: No auto-update. Internal tool, manual distribution. No DB migration system in MVP.
- **Offline**: Fully offline by design. No network calls at any point.
- **UI Binding**: `ObservableCollection<T>` and `INotifyPropertyChanged` for reactive data binding.
- **Threading**: All SQLite operations are `async`/`await`. FileWatcher dispatches UI updates to the UI thread (Post-MVP).

## Functional Requirements

> **Capability Contract**: UX designs only what is listed here. Architecture supports only what is listed here. Development implements only what is listed here.

### Model Management

- **FR1**: User can view a paginated list of device models with Name, Manufacturer, Category, and SubCategory.
- **FR2**: User can search the model list by model Name *(filter applied in-memory — no DB query)*.
- **FR3**: User can search the model list by Manufacturer *(filter applied in-memory — no DB query)*.
- **FR4**: User can filter the model list by Category *(filter applied in-memory — no DB query)*.
- **FR5**: User can filter the model list by SubCategory *(filter applied in-memory — no DB query)*.
- **FR6**: User can sort the model list by any visible column *(sort applied in-memory — no DB query)*.

### Borrow Workflow

- **FR7**: User can initiate a borrow action from any model item in the model list.
- **FR8**: The system presents a modal dialog when a borrow action is initiated.
- **FR9**: User can select the quantity of devices to borrow within the modal, constrained to available stock.
- **FR10**: User can confirm or cancel the borrow operation from the modal.
- **FR11**: The system updates device status to Borrowed upon borrow confirmation.

### My Devices

- **FR12**: User can view a list of all devices currently borrowed by them.
- **FR13**: User can search the borrowed device list by ModelId *(in-memory — no DB query)*.
- **FR14**: User can search the borrowed device list by device Name *(in-memory — no DB query)*.
- **FR15**: User can search the borrowed device list by IMEI *(in-memory — no DB query)*.
- **FR16**: User can search the borrowed device list by SerialLab *(in-memory — no DB query)*.
- **FR17**: User can search the borrowed device list by SerialNumber *(in-memory — no DB query)*.
- **FR18**: User can search the borrowed device list by CircuitSerialNumber *(in-memory — no DB query)*.
- **FR19**: User can search the borrowed device list by HWVersion *(in-memory — no DB query)*.
- **FR20**: User can filter the borrowed device list by HWVersion *(in-memory — no DB query)*.
- **FR21**: User can sort the borrowed device list by any visible column *(sort applied in-memory — no DB query)*.

### Return Workflow

- **FR22**: User can initiate a return action on any device in My Devices.
- **FR23**: The system presents a confirmation dialog when a return action is initiated.
- **FR24**: User can confirm or cancel the return from the confirmation dialog.
- **FR25**: The system updates device status to Available upon return confirmation.

### Database & Initialization

- **FR26**: The system initializes the local SQLite database automatically at application startup.
- **FR27**: If no database file exists, the system creates a new empty database.
- **FR28**: If the database exists but contains no data, the system imports data from a JSON file.
- **FR29**: If no JSON import file exists, the system generates and populates sample data.

### Data Loading

- **FR34**: The app loads all Models into memory at startup in background chunks of 100k records per DB read.
- **FR35**: The app loads all Devices into memory at startup in background chunks of 100k records per DB read.
- **FR36**: Loading executes asynchronously in the background — the UI is displayed and responsive while data loads progressively into memory.

### Multi-Instance Synchronization *(InstanceSyncService)*

- **FR30**: After a successful borrow, the system appends a JSON event to `signal.txt` containing: `action`, `modelId`, affected `deviceIds`, and updated `availableCount`.
- **FR31**: After a successful return, the system appends a JSON event to `signal.txt` containing: `action`, `modelId`, affected `deviceIds`, and updated `availableCount`.
- **FR32**: The system monitors `signal.txt` via FileWatcher; on file change, it reads only new events since the last processed position (`lastReadPosition`).
- **FR33**: New events from `signal.txt` are applied directly to the in-memory dataset — affected model and device records are updated without a DB read. If event parsing fails, the system falls back to a targeted DB query for the affected records only.

## Non-Functional Requirements

### Performance

- **NFR1**: Each individual DB chunk read (100k records) completes within 1 second. Requires SQLite indexes on all loaded columns. Full dataset (~3M records) loads progressively in the background.
- **NFR2**: Search, sort, and filter operations on the in-memory dataset return updated results within 200ms of user input — no DB query involved.
- **NFR3**: All SQLite operations execute asynchronously — UI thread never blocked.
- **NFR4**: Application UI is displayed and responsive within 3 seconds of launch. Background chunk loading continues after UI is shown.

> **Note for Architect**: In-memory strategy mandates loading the full dataset at startup in 100k-record chunks. Index all DB load columns: Name, Manufacturer, Category, SubCategory, IMEI, SerialLab, SerialNumber, CircuitSerialNumber, HWVersion. All pagination, search, sort, and filter are client-side (slice/LINQ on in-memory list) — no DB queries after initial load.

### Reliability

- **NFR5**: The app must not crash when SQLite is temporarily locked by a concurrent instance — operations fail gracefully, process remains stable.
- **NFR6**: Borrow and return operations are atomic — full commit or no change (transaction integrity, no partial updates).
- **NFR7**: FileWatcher-triggered UI updates must be dispatched to the UI thread — no cross-thread exceptions.
