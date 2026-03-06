---
stepsCompleted: [step-01-validate-prerequisites, step-02-design-epics, step-03-create-stories, step-04-final-validation-skipped]
inputDocuments:
  - _bmad-output/planning-artifacts/prd.md
---

# hello-world-winui-main - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for the Device Management Application, decomposing requirements from the PRD into implementable stories. Source: PRD only (no Architecture or UX document available). Technical context derived from existing codebase structure and PROJECT_CONTEXT.md.

## Requirements Inventory

### Functional Requirements

FR1: User can view a paginated list of device models with Name, Manufacturer, Category, and SubCategory.
FR2: User can search the model list by model Name.
FR3: User can search the model list by Manufacturer.
FR4: User can filter the model list by Category.
FR5: User can filter the model list by SubCategory.
FR6: User can sort the model list by any visible column.
FR7: User can initiate a borrow action from any model item in the model list.
FR8: The system presents a modal dialog when a borrow action is initiated.
FR9: User can select the quantity of devices to borrow within the modal, constrained to available stock.
FR10: User can confirm or cancel the borrow operation from the modal.
FR11: The system updates device status to Borrowed upon borrow confirmation.
FR12: User can view a list of all devices currently borrowed by them.
FR13: User can search the borrowed device list by ModelId.
FR14: User can search the borrowed device list by device Name.
FR15: User can search the borrowed device list by IMEI.
FR16: User can search the borrowed device list by SerialLab.
FR17: User can search the borrowed device list by SerialNumber.
FR18: User can search the borrowed device list by CircuitSerialNumber.
FR19: User can search the borrowed device list by HWVersion.
FR20: User can filter the borrowed device list by HWVersion.
FR21: User can sort the borrowed device list by any visible column.
FR22: User can initiate a return action on any device in My Devices.
FR23: The system presents a confirmation dialog when a return action is initiated.
FR24: User can confirm or cancel the return from the confirmation dialog.
FR25: The system updates device status to Available upon return confirmation.
FR26: The system initializes the local SQLite database automatically at application startup.
FR27: If no database file exists, the system creates a new empty database.
FR28: If the database exists but contains no data, the system imports data from a JSON file.
FR29: If no JSON import file exists, the system generates and populates sample data.
FR30 [Post-MVP]: After a successful borrow, the system writes a change signal to signal.txt.
FR31 [Post-MVP]: After a successful return, the system writes a change signal to signal.txt.
FR32 [Post-MVP]: The system monitors signal.txt via FileWatcher and triggers a UI refresh on file change.
FR33 [Post-MVP]: A FileWatcher-triggered refresh updates both the model list (available count) and My Devices list.

### NonFunctional Requirements

NFR1: All database read operations complete within 1 second under ~3,000,000 records. Requires SQLite indexes on all queried columns.
NFR2: Search, sort, and filter operations return updated results within 1 second of user input at ~3,000,000 record volume.
NFR3: All SQLite operations execute asynchronously — UI thread never blocked.
NFR4: Application startup including full DB initialization completes within 3 seconds.
NFR5: App must not crash when SQLite is temporarily locked by a concurrent instance — graceful failure, stable process.
NFR6: Borrow and return operations are atomic — full commit or no change (transaction integrity, no partial updates).
NFR7 [Post-MVP]: FileWatcher-triggered UI updates must be dispatched to the UI thread — no cross-thread exceptions.

### Additional Requirements

- Brownfield project: 3-layer Clean Architecture (MVVM + Clean Architecture) already scaffolded.
- WinUI 3 (Windows App SDK) already configured. No new project setup required.
- SQLite via Microsoft.Data.Sqlite already integrated.
- DatabaseInitializer already exists in App7.Data/Db/ — called at OnLaunched in App.xaml.cs.
- UC1 (View Models) currently in progress — likely partial ViewModel/View scaffold exists.
- UC2–UC6: Not yet started — full View + ViewModel + UseCase + Repository implementation required.
- All UseCase interfaces must remain in Domain layer; implementations in Data layer.
- No migration system — schema changes are manual in this phase.
- Performance target of 1s at 3M records mandates SQLite indexes on search/filter columns.

### FR Coverage Map

| FR | Epic | Description |
|---|---|---|
| FR1–6 | Epic 2 | Model list view, search, filter, sort *(in-memory)* |
| FR7–11 | Epic 3 | Borrow modal workflow |
| FR12–21 | Epic 4 | My Devices view, multi-field search, filter, sort *(in-memory)* |
| FR22–25 | Epic 5 | Return confirmation workflow |
| FR26–29 | Epic 1 | DB bootstrap & initialization |
| FR30–33 | Epic 6 | InstanceSyncService — signal.txt append-only event log *(MVP)* |
| FR34–36 | Epic 1 | Background chunk loading (100k/chunk) |
| NFR1–4 | Epic 1, 2, 4 | Chunk load performance, in-memory ops, startup UX |
| NFR5–6 | Epic 3, 5 | SQLite lock safety, atomic transactions |
| NFR7 | Epic 6 | UI thread dispatch for FileWatcher |

## Epic List

### Epic 1: Database Foundation & App Startup
User can launch the app and have a working local database ready — with data loaded from import or generated samples — without any manual setup.
**FRs covered:** FR26, FR27, FR28, FR29 | **NFRs:** NFR1 (index strategy), NFR4 (3s startup)

### Epic 2: View & Discover Device Models
User can view the full paginated model catalog and quickly find models using search, sort, and filter.
**FRs covered:** FR1, FR2, FR3, FR4, FR5, FR6 | **NFRs:** NFR1, NFR2 (1s at 3M records), NFR3 (async)

### Epic 3: Borrow a Device
User can borrow one or more devices from a selected model through a modal confirmation flow, with device status updated immediately.
**FRs covered:** FR7, FR8, FR9, FR10, FR11 | **NFRs:** NFR3 (async), NFR5 (lock safety), NFR6 (atomic)

### Epic 4: View & Manage Borrowed Devices
User can view all currently borrowed devices and search, sort, or filter the list by any relevant field.
**FRs covered:** FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR21 | **NFRs:** NFR1, NFR2 (1s at 3M records), NFR3 (async)

### Epic 5: Return a Device
User can return any borrowed device through a confirmation dialog, with the device status immediately reset to Available and the model's available count updated.
**FRs covered:** FR22, FR23, FR24, FR25 | **NFRs:** NFR3 (async), NFR5 (lock safety), NFR6 (atomic)

### Epic 6: Multi-Instance Synchronization
When one app instance borrows or returns a device, all other running instances instantly update their in-memory dataset via `signal.txt` event log — no DB re-read required, no manual action needed.
**FRs covered:** FR30, FR31, FR32, FR33 | **NFRs:** NFR7 (UI thread dispatch)

---

## Epic 1: Database Foundation & App Startup

User can launch the app and have a working local database ready — with data loaded from import or generated samples — without any manual setup.

### Story 1.1: Database Initialization at App Startup

As a user,
I want the app to automatically initialize the local SQLite database when it launches,
So that I don't need to perform any manual setup before using the app.

**Acceptance Criteria:**

**Given** the app is launched on any machine
**When** `DatabaseInitializer` runs at `OnLaunched`
**Then** a SQLite database file is created at the configured path if it does not exist
**And** the database schema includes all required tables (Models, Devices) with correct columns and types
**And** SQLite indexes are created on all search/filter columns: Name, Manufacturer, Category, SubCategory, IMEI, SerialLab, SerialNumber, CircuitSerialNumber, HWVersion
**And** app startup completes within 3 seconds (NFR4)

### Story 1.2: Data Import from JSON on First Run

As a user,
I want the app to import device data from an existing JSON file if available,
So that I have real inventory data ready to use immediately without manual entry.

**Acceptance Criteria:**

**Given** the database was just created (empty) and a JSON import file exists at the configured path
**When** `DatabaseInitializer` runs after schema creation
**Then** all models and devices from the JSON file are imported into the database
**And** the import completes without data loss or corruption
**And** the app proceeds to launch normally after import completes

### Story 1.3: Sample Data Generation as Fallback

As a user,
I want the app to generate sample data if no JSON import file is found,
So that the app is immediately usable on a fresh install with no data file.

**Acceptance Criteria:**

**Given** the database was just created (empty) and no JSON import file exists at the configured path
**When** `DatabaseInitializer` runs after schema creation
**Then** a set of sample device models and devices is generated and inserted into the database
**And** the sample data is sufficient to exercise all app features (borrow, return, search, filter)
**And** the app proceeds to launch normally after sample data generation

### Story 1.4: Background Chunk Loading of Full Dataset

As a user,
I want the app to load all device and model data into memory in the background at startup,
So that search, sort, and filter operations are instant without hitting the database.

**Acceptance Criteria:**

**Given** the app has launched and the database is initialized
**When** the app starts up
**Then** all Models are loaded into memory in chunks of 100k records per DB read (FR34)
**And** all Devices are loaded into memory in chunks of 100k records per DB read (FR35)
**And** loading executes asynchronously in the background — UI is displayed and responsive immediately (FR36)
**And** each individual 100k-chunk DB read completes within 1 second (NFR1)
**And** a loading indicator is shown while background loading is in progress
**And** search, sort, and filter become fully responsive once all chunks are loaded

---

## Epic 2: View & Discover Device Models

User can view the full paginated model catalog and quickly find models using search, sort, and filter.

### Story 2.1: Paginated Model List

As a user,
I want to view a paginated list of device models when I open the app,
So that I can browse available devices without being overwhelmed by the full dataset.

**Acceptance Criteria:**

**Given** the app has launched and the database is initialized
**When** the main screen loads
**Then** a paginated list of device models is displayed showing Name, Manufacturer, Category, SubCategory
**And** the list loads within 1 second even with ~3,000,000 records in the database (NFR1)
**And** pagination controls allow navigating between pages
**And** the data load executes asynchronously — UI remains responsive during load (NFR3)

### Story 2.2: Search Models by Name and Manufacturer

As a user,
I want to search the model list by Name or Manufacturer,
So that I can quickly find a specific model without scrolling through all pages.

**Acceptance Criteria:**

**Given** the model list is displayed and the full dataset is loaded in memory
**When** the user types text into the search field
**Then** the list filters to show only models where Name or Manufacturer contains the search text
**And** filtering is performed entirely in-memory — no DB query is triggered (FR2, FR3)
**And** filtered results appear within 200ms of input (NFR2)
**And** clearing the search field restores the full paginated model list

### Story 2.3: Filter Models by Category and SubCategory

As a user,
I want to filter the model list by Category and SubCategory,
So that I can narrow down models by device type without typing.

**Acceptance Criteria:**

**Given** the model list is displayed and the full dataset is loaded in memory
**When** the user selects a Category from the filter control
**Then** the list shows only models matching that Category
**And** when a SubCategory is additionally selected, the list narrows further to matching SubCategory
**And** filtering is performed entirely in-memory — no DB query is triggered (FR4, FR5)
**And** filter results appear within 200ms (NFR2)
**And** clearing filters restores the full paginated list

### Story 2.4: Sort Models by Column

As a user,
I want to sort the model list by any visible column,
So that I can organize the list by the field most relevant to my current task.

**Acceptance Criteria:**

**Given** the model list is displayed and the full dataset is loaded in memory
**When** the user clicks a column header
**Then** the list is sorted by that column in ascending order
**And** clicking the same column header again toggles to descending order
**And** sort can be applied simultaneously with active search or filter
**And** sorting is performed entirely in-memory — no DB query is triggered (FR6)
**And** sort results appear within 200ms (NFR2)

---

## Epic 3: Borrow a Device

User can borrow one or more devices from a selected model through a modal confirmation flow, with device status updated immediately.

### Story 3.1: Borrow Modal Trigger

As a user,
I want to click a Borrow button on a model item to open a borrow dialog,
So that I can initiate the borrow process for that model.

**Acceptance Criteria:**

**Given** the model list is displayed and a model has available stock (`Available > 0`)
**When** the user clicks the **Borrow** button on a model item
**Then** a modal dialog opens showing the model name and a quantity input field
**And** the quantity input is pre-filled with `1` and constrained between `1` and the current available count
**And** the modal contains a **Confirm** button and a **Cancel** button

### Story 3.2: Borrow Confirmation and Device Status Update

As a user,
I want to confirm a borrow action and have the device status updated immediately,
So that the inventory accurately reflects which devices are now borrowed.

**Acceptance Criteria:**

**Given** the borrow modal is open and a valid quantity is entered
**When** the user clicks **Confirm**
**Then** the selected quantity of devices belonging to that model are marked as `Borrowed` in the database
**And** the operation completes atomically — either all selected devices are updated or none are (NFR6)
**And** the operation executes asynchronously — UI does not freeze during the database write (NFR3)
**And** if SQLite is temporarily locked by another instance, the app does not crash — it fails gracefully (NFR5)
**And** the in-memory dataset is updated directly: affected devices marked as `Borrowed`, model `availableCount` decremented
**And** a JSON event is appended to `signal.txt`: `{"action":"borrow","modelId":X,"deviceIds":[...],"availableCount":N}` (FR30)
**And** the modal closes and the UI reflects the updated state immediately from in-memory

### Story 3.3: Cancel Borrow

As a user,
I want to cancel a borrow operation from the modal,
So that I can dismiss the dialog without making any changes.

**Acceptance Criteria:**

**Given** the borrow modal is open
**When** the user clicks **Cancel** or dismisses the modal
**Then** no changes are made to the database
**And** the modal closes and the model list remains unchanged

---

## Epic 4: View & Manage Borrowed Devices

User can view all currently borrowed devices and search, sort, or filter the list by any relevant field.

### Story 4.1: My Devices List

As a user,
I want to view a list of all devices I am currently borrowing,
So that I can track what devices are in my possession at any time.

**Acceptance Criteria:**

**Given** the user navigates to the My Devices screen
**When** the screen loads
**Then** a list of all devices with status `Borrowed` is displayed with fields: ModelId, Name, IMEI, SerialLab, SerialNumber, CircuitSerialNumber, HWVersion
**And** the list is served from the in-memory dataset — no DB query on tab switch (NFR2)
**And** the screen renders within 200ms (NFR2)

### Story 4.2: Multi-Field Search on My Devices

As a user,
I want to search my borrowed devices by any identifier field,
So that I can locate a specific device quickly regardless of which field I remember.

**Acceptance Criteria:**

**Given** the My Devices list is displayed and the full dataset is loaded in memory
**When** the user types text into the search field
**Then** the list filters to show only devices where ModelId, Name, IMEI, SerialLab, SerialNumber, CircuitSerialNumber, or HWVersion contains the search text
**And** filtering is performed entirely in-memory — no DB query is triggered (FR13–FR19)
**And** filtered results appear within 200ms of input (NFR2)
**And** clearing the search field restores the full My Devices list

### Story 4.3: Filter My Devices by HWVersion

As a user,
I want to filter my borrowed devices by HWVersion,
So that I can group and identify devices by hardware revision.

**Acceptance Criteria:**

**Given** the My Devices list is displayed and the full dataset is loaded in memory
**When** the user selects a HWVersion value from the filter control
**Then** the list shows only devices matching that HWVersion
**And** filtering is performed entirely in-memory — no DB query is triggered (FR20)
**And** filter results appear within 200ms (NFR2)
**And** clearing the filter restores the full My Devices list

### Story 4.4: Sort My Devices by Column

As a user,
I want to sort my borrowed devices by any visible column,
So that I can organize the list to find what I need faster.

**Acceptance Criteria:**

**Given** the My Devices list is displayed and the full dataset is loaded in memory
**When** the user clicks a column header
**Then** the list is sorted by that column in ascending order
**And** clicking the same column again toggles to descending order
**And** sort can be applied simultaneously with active search or filter
**And** sorting is performed entirely in-memory — no DB query is triggered (FR21)
**And** sort results appear within 200ms (NFR2)

---

## Epic 5: Return a Device

User can return any borrowed device through a confirmation dialog, with the device status immediately reset to Available and the model's available count updated.

### Story 5.1: Return Confirmation Dialog

As a user,
I want to click a Return button on a borrowed device to open a confirmation dialog,
So that I can confirm my intent to return before the action is committed.

**Acceptance Criteria:**

**Given** the My Devices list is displayed and contains at least one device
**When** the user clicks the **Return** button on a device item
**Then** a confirmation dialog opens identifying the device (Name and IMEI or SerialNumber)
**And** the dialog contains a **Confirm** button and a **Cancel** button

### Story 5.2: Return Confirmation and Device Status Reset

As a user,
I want to confirm a return action and have the device status reset immediately,
So that the device becomes available for others to borrow again.

**Acceptance Criteria:**

**Given** the return confirmation dialog is open
**When** the user clicks **Confirm**
**Then** the device status is updated to `Available` in the database
**And** the operation completes atomically — full commit or no change (NFR6)
**And** the operation executes asynchronously — UI does not freeze (NFR3)
**And** if SQLite is temporarily locked by another instance, the app does not crash — it fails gracefully (NFR5)
**And** the in-memory dataset is updated directly: device marked as `Available`, model `availableCount` incremented
**And** a JSON event is appended to `signal.txt`: `{"action":"return","modelId":X,"deviceIds":[...],"availableCount":N}` (FR31)
**And** the dialog closes and the device disappears from My Devices list immediately from in-memory

### Story 5.3: Cancel Return

As a user,
I want to cancel a return operation from the confirmation dialog,
So that I can dismiss without making any changes.

**Acceptance Criteria:**

**Given** the return confirmation dialog is open
**When** the user clicks **Cancel** or dismisses the dialog
**Then** no changes are made to the database
**And** the dialog closes and the My Devices list remains unchanged

---

## Epic 6: Multi-Instance Synchronization

When one app instance borrows or returns a device, all other running instances instantly update their in-memory dataset by reading new events from `signal.txt` — no DB re-read, no manual action required.

### Story 6.1: Append Event to signal.txt on Borrow or Return

As a user running multiple app instances,
I want any borrow or return action to broadcast a structured event to other instances via signal.txt,
So that all open windows can sync their in-memory data without reloading from the database.

**Acceptance Criteria:**

**Given** a borrow or return operation completes successfully (DB write confirmed)
**When** the database transaction commits
**Then** the app appends a JSON event as a new line to `signal.txt` (FR30 / FR31)
**And** the event contains: `seq` (monotonic integer), `action` ("borrow" or "return"), `modelId`, `deviceIds` (array), `availableCount`
**And** the write uses append mode — existing events are never overwritten
**And** the append happens after the DB commit — never before
**And** example format: `{"seq":1,"action":"borrow","modelId":42,"deviceIds":[1001,1002],"availableCount":8}`

### Story 6.2: FileWatcher Reads and Applies Events to In-Memory Dataset

As a user,
I want other open app instances to automatically apply changes to their in-memory data when signal.txt is updated,
So that the model list and My Devices list always reflect the latest inventory state across all instances.

**Acceptance Criteria:**

**Given** two or more instances of the app are running with full datasets loaded in memory
**When** Instance A appends a new event to `signal.txt`
**Then** all other instances' FileWatcher detects the file change (FR32)
**And** each instance reads only the new lines since its `lastReadPosition` (FR32)
**And** each event is parsed and applied to the in-memory dataset: affected device records updated, model `availableCount` updated (FR33)
**And** no DB query is performed during this sync — only `signal.txt` is read (FR33)
**And** if a line fails to parse, the instance falls back to a targeted DB query for the affected `modelId` only
**And** the UI update (model list + My Devices list refresh) is dispatched to the UI thread — no cross-thread exceptions (NFR7)
**And** no user action is required on any instance
