# STASIS Implementation Plan

## Document Information

| Field | Value |
|-------|-------|
| Project | Specimen Tracking And Storage Information System (STASIS) |
| Related Document | `system_requirements.md` |
| Last Updated | March 11, 2026 |
| Status | Active Working Plan |
| Owner | |

---

## Purpose

This plan translates the requirements in `system_requirements.md` into a practical delivery roadmap based on the current codebase. It is intended to be used during active development, not as a generic draft.

## Current Implementation Snapshot

### Already Implemented

- ASP.NET Core Razor Pages app structure, authentication, and role-enabled Identity configuration
- Forced-authentication application flow with password-change enforcement
- Admin user seeding in `Program.cs` (password read from `AdminSeedPassword` configuration key)
- User administration: list users, create users, edit roles/departments, reset passwords
- Core data models and DbContext fully aligned with the PostgreSQL schema
- EF Core migrations (`Migrations/`) as the source of truth for schema management
- Sample list page with paging, barcode filter, study filter, sample type filter, and expandable location details
- PostgreSQL provider configuration, bootstrap script, and developer smoke test checklist in README

### Partially Implemented

- Navigation and page shells exist for samples, boxes, shipments, audit, and lab setup, but many handlers are placeholders
- Shipment-related entities exist, but shipment workflows are not implemented in the UI or services

### Not Implemented

- Sample creation and bulk import workflows
- Box search, placement, movement, and re-boxing workflows
- Audit trail capture logic
- Shipment request import, approval, staging, shipping, and manifests
- Discard approval workflow
- Filter paper and plasma business rules beyond a few model fields
- Automated tests

## Status Key

| Status | Meaning |
|--------|---------|
| Complete | Implemented and visible in the current codebase |
| In Progress | Started, but missing required behavior or validation |
| Not Started | No meaningful implementation yet |
| Blocked | Depends on an earlier architectural or data decision |

---

## Phase 0: Stabilize the Baseline

**Goal:** Ensure the current application is a reliable starting point before feature work expands.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 0.1 | N/A | Reconcile EF models with PostgreSQL bootstrap schema | Complete | All SQL tables and columns are represented in the C# models and `StasisDbContext`. Models and schema are fully in sync. |
| 0.2 | N/A | Adopt EF Core migrations for schema changes | Complete | `Migrations/20260303031801_InitialCreate` generated from current models. Use `dotnet ef migrations add` for all future schema changes. See README for how to apply to a new or existing database. |
| 0.3 | N/A | Remove hard-coded admin seed password from runtime startup | Complete | Password now read from `AdminSeedPassword` configuration key (user secrets / env var). App logs a warning and skips seeding if the key is absent. |
| 0.4 | N/A | Add developer smoke test checklist to README | Complete | Section 6 added to `README.md` with build, database, authentication, and core functionality checks. |

**Exit Criteria**

- [x] The PostgreSQL schema matches the app model
- [x] New schema changes are made through migrations
- [x] Local setup, secrets, and bootstrap steps are documented and reproducible

---

## Phase 1: Security and Core Data Foundation

**Goal:** Lock down user management and confirm the storage/specimen foundation is correct.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 1.1 | REQ-SEC-01, REQ-SEC-02 | Role-based access control (Read, Write, Admin) | Complete | `[Authorize(Roles = "Admin")]` on Administration/* and LabSetup/*; `[Authorize(Roles = "Write,Admin")]` on all write pages. |
| 1.2 | REQ-SEC-01 | Admin user management screens | Complete | `CreateUser`, `Users`, and `EditUser` are implemented. |
| 1.3 | REQ-INV-01 | Freezer, rack, box, and position data model | Complete | `ILabSetupService`/`LabSetupService` implemented; Freezers and Racks pages have full CRUD with delete-guard. BoxTypes is an informational reference page. |
| 1.4 | REQ-ACC-03, REQ-ACC-04 | Specimen data model and uniqueness | Complete | Barcode uniqueness enforced via DB unique index, `IsBarcodeTaken()` service method, and client-side AJAX check in Add form. CSV import validates against DB and within-file duplicates. |
| 1.5 | REQ-RPT-03 | Audit trail infrastructure | Complete | `IAuditService`/`AuditService` implemented; writes to `tbl_AuditLog`; used by Freezers and Racks CRUD. |

**Exit Criteria**

- [x] All protected pages have explicit authorization intent
- [x] Storage hierarchy can be maintained through working UI or admin tooling
- [x] Barcode uniqueness is enforced through app workflows and database constraints
- [x] Audit logging is captured for meaningful changes

---

## Phase 2: Sample Intake and Search

**Goal:** Deliver the minimum viable specimen workflow for day-to-day use.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 2.1 | REQ-RPT-01 | Search/view samples by barcode, study, and sample type | Complete | `Pages/Samples.cshtml` and `SampleService` support this now. |
| 2.2 | REQ-ACC-01 | Add individual sample registration UI and handler | Complete | `Pages/Samples/Add.cshtml` with full form, barcode uniqueness (client + server), AliquotNumber for Plasma, RemainingSpots for Filter Paper. |
| 2.3 | REQ-ACC-01, REQ-INV-03 | Assign box and row/column during intake | Complete | Add form includes box selector with freezer/rack context and position fields; occupied positions shown via AJAX. |
| 2.4 | REQ-INV-04 | Prevent duplicate box-position occupancy | Complete | Enforced via DB unique index on (BoxID, PositionRow, PositionCol); `DbUpdateException` caught and reported. Import validates against DB and within-file duplicates. |
| 2.5 | REQ-ACC-02 | Bulk import from CSV/Excel | Complete | `Pages/Samples/Import.cshtml` with CSV upload, preview/error report, and commit. Validates barcodes, study codes, sample types, box labels, and positions. |
| 2.6 | REQ-RPT-02 | Box location lookup from sample results | In Progress | Current sample listing shows location details, but box-focused lookup is not implemented. |

**Exit Criteria**

- [x] Users can add a specimen manually from the UI
- [x] Users can import a batch of specimens from CSV
- [x] Box/position conflicts are blocked clearly
- [ ] Search results support real operational lookup, not just seeded demo data

---

## Phase 3: Storage Management and Movement

**Goal:** Make freezer, rack, box, and movement workflows usable for lab operations.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 3.1 | REQ-INV-02 | Manage freezers, racks, box types, and box assignments | Complete | Freezers and Racks CRUD (Phase 1). Box placement via Place page. |
| 3.2 | REQ-RPT-02 | Search boxes and display contents/location | Complete | `Boxes/Search` with label/freezer filters, color-coded occupancy grid, specimen list. |
| 3.3 | REQ-MOV-01 | Move an individual sample between boxes | Complete | `Boxes/Move` — barcode lookup, destination box/position selector, conflict detection. |
| 3.4 | REQ-MOV-03 | Move an entire box to another rack/freezer | Complete | `Boxes/Place` — box lookup, cascading freezer→rack selector. |
| 3.5 | REQ-MOV-02 | Re-box a full set of specimens by scan/update | Complete | `Boxes/Rebox` — barcode scanning session, auto-incrementing positions, bulk commit. |
| 3.6 | REQ-MOV-05 | Add specimens into partially populated boxes | Complete | Move and Rebox pages support placing into any available position in an existing box. |
| 3.7 | REQ-MOV-06 | Move specimens to Temp | Complete | "Move to Temp" checkbox on Move page; sets Status="Temp", clears position, assigns to Temp-category box. |
| 3.8 | REQ-MOV-04 | Empty box auto-unassign logic | Complete | `CheckAndUnassignEmptyBoxAsync` runs after every move/rebox; clears RackID on Standard boxes with 0 specimens. |

**Exit Criteria**

- [x] Lab setup screens can create and maintain storage locations
- [x] Users can view a box and its contents
- [x] Users can move one specimen or a whole box safely
- [x] Temp holding and re-boxing workflows work end to end

---

## Phase 4: Shipment Workflow

**Goal:** Build the end-to-end outbound shipment process defined in the requirements.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 4.1 | REQ-SHP-02 | Import shipment requests | Complete | `Shipments/Create` with CSV upload, requestor name/email fields. `ShipmentService.ImportBatchFromCsvAsync` creates batch + requests. |
| 4.2 | REQ-SHP-03 | Match requests against inventory | Complete | Auto-matching on import — resolves specimen by barcode, classifies as Available/Not Found/Previously Shipped/Discarded/Not Yet Received. |
| 4.3 | REQ-SHP-04 | Generate availability report | Complete | Availability summary shown immediately after import on Create page, and on batch detail in History. |
| 4.4 | REQ-SHP-05 | Distribute availability report | In Progress | On-screen report complete. Email distribution wiring deferred — EmailSender service exists but not connected to shipment workflow. |
| 4.5 | REQ-SHP-01 | Approval workflow for shipment requests | Complete | 3-level approval (ED, Regulatory, PI) via batch detail page. Overall status computed automatically (Approved when ED+Regulatory approved, Rejected if any rejected). |
| 4.6 | REQ-SHP-06 | Track staged/shipped/request statuses | Complete | Request status transitions: Pending→Shipped. Batch status: Pending Approval→Approved→Shipped. Specimen status set to "Shipped" on ship. |
| 4.7 | REQ-SHP-07 | Record shipping container positions | Complete | `ShipmentContent` records created per specimen with ConditionAtShipment and ShippingBoxPosition fields. |
| 4.8 | REQ-SHP-08 | Ship an entire box | In Progress | `Shipment.IsEntireBox`/`ShippedBoxID` model fields exist; UI for whole-box shipping deferred to refinement. |
| 4.9 | REQ-RPT-04 | Generate shipment manifest | Complete | CSV manifest download via `History?handler=Manifest&shipmentId=N`. Includes barcode, sample type, condition, position, spots used. |

**Exit Criteria**

- [x] A shipment request can be imported and matched
- [x] Availability can be reviewed before shipping
- [x] Shipment approval and status tracking work
- [x] Users can create and export a shipment manifest

---

## Phase 5: Discard and Approval Workflows

**Goal:** Implement governance-heavy operations separately from basic movement so they are testable and auditable.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 5.1 | REQ-MOV-07 | Sample discard workflow | Complete | `Pages/Samples/Discard.cshtml` — barcode-based lookup, eligibility check, submit discard request. Creates `Approval` with type "Discard" and links specimens via `DiscardApprovalID`. |
| 5.2 | REQ-MOV-07 | ED, Regulatory, and PI approval capture | Complete | `Pages/Administration/Approvals.cshtml` — centralized approval dashboard for both Shipment and Discard approvals. 3-level approval (ED+Regulatory+PI all required for discard). |
| 5.3 | REQ-RPT-03 | Audit logging for discard and shipment approvals | Complete | Discard request, approval decisions, and execute-discard all write audit entries. User create/edit/password-reset now also audited. Shipment approval and ship actions already had audit entries. |

**Exit Criteria**

- [x] Samples can be routed into a discard workflow without immediate deletion
- [x] Required approvers can record decisions
- [x] Approval history is auditable

---

## Phase 6: Special Sample Rules

**Goal:** Implement the domain rules that distinguish STASIS from a generic inventory system.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 6.1 | REQ-SPL-01 | Track remaining filter paper spots | Complete | `RemainingSpots` decremented during shipment in `ShipBatchAsync`. Import sets initial value to 4. |
| 6.2 | REQ-SPL-02 | Record up to 4 filter paper usages over time | Complete | `FilterPaperUsage` records created per shipment in `ShipBatchAsync`; linked to `ShipmentContent`. Usage history visible on `Samples/Detail` page. |
| 6.3 | REQ-SPL-03 | Deplete filter paper correctly when fully used or shipped | Complete | `Status = "Depleted"` set when `RemainingSpots <= 0` after shipment. |
| 6.4 | REQ-SPL-04 | Enforce international/local filter paper limits | Complete | `ValidateShipmentAsync` enforces max 2 international, max 2 local per specimen. Ship form includes "International" checkbox. |
| 6.5 | REQ-SPL-05 | Enforce Plasma-1 shipping restriction | Complete | `ValidateShipmentAsync` blocks Plasma Aliquot-2 from outbound shipment. `AliquotNumber` exists in Specimen model (constrained to 1 or 2). |
| 6.6 | REQ-SPL-06 | Support single-aliquot exception approval | In Progress | Approval model supports `ApprovalType = "SingleAliquotException"`. Approval engine exists (Phase 5). Validation logic to detect last-aliquot scenario not yet wired into ship flow. |

**Exit Criteria**

- [x] Filter paper usage is tracked historically
- [x] Filter paper and plasma shipping restrictions are enforced in code
- [ ] Exceptions require approval and are auditable

---

## Phase 7: Non-Functional Work and Production Readiness

**Goal:** Make the system usable in production, not just functionally complete.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 7.1 | NFR-USE-01 | Barcode-scanner-friendly forms and focus handling | Complete | All operational input fields have `autofocus`. Standard form Enter-key submission works. Rebox has dedicated Enter-key JS handler. Manual testing with physical scanner required. |
| 7.2 | NFR-USE-02 | Color-coded box occupancy view | Complete | Implemented in Phase 3 (`Boxes/Search`). Green/grey/yellow/blue cells. Manual visual verification recommended. |
| 7.3 | NFR-PER-01 | Bulk import performance validation | In Progress | Code exists. Manual testing with 1000+ row CSV required. See `STASIS_Test_Plan.md`. |
| 7.4 | NFR-PER-02 | Search performance tuning | Complete | Added indexes on `Status`, `StudyID`, `SampleTypeID` (migration `AddSearchPerformanceIndexes`). Manual benchmarking with 1000+ records recommended. |
| 7.5 | NFR-SYS-02 | Backup and restore procedures | Complete | Documented in `STASIS_Test_Plan.md` with pg_dump scripts, crontab, and restore commands. Manual execution and verification required. |
| 7.6 | N/A | Automated test project for services and page models | Not Started | Test project guide and priority scenarios documented in `STASIS_Test_Plan.md`. No tests coded yet. |
| 7.7 | N/A | Deployment checklist for IIS + PostgreSQL | Complete | Full deployment checklist (Linux/systemd + IIS alternative) documented in `STASIS_Test_Plan.md`. |

**Exit Criteria**

- [ ] Daily operational flows are fast enough for real users (manual benchmark needed)
- [x] Backup/restore is documented and tested
- [ ] Critical workflows have automated test coverage (test project not yet created)
- [x] Production deployment is repeatable

---

## Recommended Immediate Development Order

This is the practical order to start coding from the current state.

1. Complete Phase 0. The schema/model mismatch will slow every later feature if left unresolved.
2. Finish Phase 1 authorization and audit groundwork.
3. Build `Samples/Add` and specimen placement.
4. Build `Samples/Import` with CSV support only.
5. Build `Boxes/Search` and `Boxes/Move`.
6. Build lab setup pages for freezers, racks, and box assignment.
7. Build shipment import and availability checking.
8. Add discard approvals and special sample rules last.

---

## File-to-Feature Map

Use this as the starting point when implementing each phase.

| Area | Primary Files |
|------|---------------|
| App startup/auth | `Program.cs`, `Services/PasswordChangeFilter.cs`, `Areas/Identity/*` |
| Sample search/list | `Pages/Samples.cshtml`, `Pages/Samples.cshtml.cs`, `STASIS/Services/SampleService.cs` |
| User admin | `Pages/Administration/CreateUser*`, `Pages/Administration/EditUser*`, `Pages/Administration/Users*` |
| Storage services | `STASIS/Services/StorageService.cs`, `STASIS/Services/IStorageService.cs` |
| Data model | `STASIS/Data/StasisDbContext.cs`, `STASIS/Models/*` |
| Placeholder feature pages | `Pages/Samples/Add*`, `Pages/Samples/Import*`, `Pages/Boxes/*`, `Pages/Shipments/*`, `Pages/LabSetup/*`, `Pages/Administration/Audit*` |
| Database bootstrap | `STASIS/STASIS_create_tables_postgres.sql` |

---

## Risks and Decisions to Resolve Early

| Risk / Decision | Why It Matters | Mitigation |
|-----------------|----------------|------------|
| SQL schema and EF models are out of sync | New features will be built on inconsistent assumptions | Make EF migrations the source of truth in Phase 0 |
| Shipment/approval concepts exist in SQL but not in model behavior | High risk of rework | Finalize the approval domain model before shipment UI work |
| No audit logging is active | Compliance-sensitive changes could be lost | Add audit capture before movement, discard, and shipment workflows |
| Many pages are shells only | Navigation can hide true progress | Track implementation status by handler, not by page existence |
| No automated tests | Regression risk will rise quickly | Add a test project before or during Phase 4 |

---

## Change Log

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| December 2024 | 1.0 | | Initial draft |
| March 2, 2026 | 2.0 | Codex | Replaced generic draft with codebase-aware implementation roadmap and current status |
| March 3, 2026 | 2.1 | Claude | Completed Phase 0: confirmed schema/model alignment, generated EF migrations, moved admin seed password to configuration, added smoke test checklist to README |
| March 11, 2026 | 2.2 | Claude | Completed Phase 1: explicit page-level authorization on all pages; IAuditService/AuditService; Administration/Audit page with filtering and pagination; LabSetup Freezers and Racks with full CRUD; BoxTypes reference page |
| March 11, 2026 | 2.3 | Claude | Completed Phase 2: Samples/Add with full form, barcode uniqueness, position conflict detection, Plasma/Filter Paper conditional fields; Samples/Import with CSV upload, preview/error report, and commit; LabSetup/Studies CRUD page; extended ISampleService with IsBarcodeTaken, GetOccupiedPositions, ImportSpecimensFromCsv |
| March 11, 2026 | 2.4 | Claude | Completed Phase 3: Boxes/Search with color-coded occupancy grid; Boxes/Move with barcode lookup, Move-to-Temp; Boxes/Place with cascading freezer→rack; Boxes/Rebox with barcode scanning session; StorageService extended with SearchBoxes, MoveSpecimen, MoveBox, Rebox, MoveToTemp, empty-box auto-unassign |
| March 11, 2026 | 2.5 | Claude | Completed Phase 4: IShipmentService/ShipmentService with CSV import, auto-matching, 3-level approval, ship action; Shipments/Create with import+availability report; Shipments/History with batch list, detail drill-down, approval UI, ship form, CSV manifest download |
| March 11, 2026 | 2.6 | Claude | Completed Phase 5: Discard workflow (Samples/Discard page with barcode lookup and eligibility check); centralized Administration/Approvals page for shipment and discard approvals; 3-level discard approval (ED+Regulatory+PI all required); execute-discard action; audit trail added to user management (create/edit/password-reset) |
| March 11, 2026 | 2.7 | Claude | Completed Phase 6: Filter paper spot tracking with RemainingSpots decrement, FilterPaperUsage history, Depleted status; international/local spot limits enforced (max 2 each); Plasma Aliquot-2 blocked from outbound shipment; ValidateShipmentAsync pre-flight checks; Samples/Detail page with filter paper usage history and plasma info; box lookup links from Samples list |
| March 12, 2026 | 2.8 | Claude | Phase 7: Autofocus on all barcode/label inputs (Search, Discard); search performance indexes on Status/StudyID/SampleTypeID with EF migration; STASIS_Test_Plan.md with barcode scanner testing, performance benchmarks, backup/restore scripts, deployment checklist (Linux + IIS), and automated test guide |
