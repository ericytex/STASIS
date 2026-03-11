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
| 3.1 | REQ-INV-02 | Manage freezers, racks, box types, and box assignments | In Progress | Freezers and Racks CRUD complete (Phase 1). Box assignment UI not yet implemented. |
| 3.2 | REQ-RPT-02 | Search boxes and display contents/location | Not Started | `Pages/Boxes/Search.cshtml.cs` is a placeholder. |
| 3.3 | REQ-MOV-01 | Move an individual sample between boxes | Not Started | `Pages/Boxes/Move.cshtml.cs` is a placeholder. |
| 3.4 | REQ-MOV-03 | Move an entire box to another rack/freezer | Not Started | Likely belongs in box search/details workflow. |
| 3.5 | REQ-MOV-02 | Re-box a full set of specimens by scan/update | Not Started | `Pages/Boxes/Rebox.cshtml.cs` is a placeholder. |
| 3.6 | REQ-MOV-05 | Add specimens into partially populated boxes | Not Started | Should reuse the same slot-allocation rules as intake. |
| 3.7 | REQ-MOV-06 | Move specimens to Temp | Not Started | PostgreSQL seed data includes `SYSTEM-TEMP`; behavior is not implemented. |
| 3.8 | REQ-MOV-04 | Empty box auto-unassign logic | Not Started | Requires movement workflows first. |

**Exit Criteria**

- [ ] Lab setup screens can create and maintain storage locations
- [ ] Users can view a box and its contents
- [ ] Users can move one specimen or a whole box safely
- [ ] Temp holding and re-boxing workflows work end to end

---

## Phase 4: Shipment Workflow

**Goal:** Build the end-to-end outbound shipment process defined in the requirements.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 4.1 | REQ-SHP-02 | Import shipment requests | Not Started | Start with CSV import into `ShipmentRequest` records. |
| 4.2 | REQ-SHP-03 | Match requests against inventory | Not Started | Reuse specimen search logic and add shipment-specific status resolution. |
| 4.3 | REQ-SHP-04 | Generate availability report | Not Started | First deliver as on-screen/exportable table. |
| 4.4 | REQ-SHP-05 | Distribute availability report | Not Started | Email integration exists in principle, but workflow wiring does not. |
| 4.5 | REQ-SHP-01 | Approval workflow for shipment requests | Not Started | Current code has no approval engine despite schema references. |
| 4.6 | REQ-SHP-06 | Track staged/shipped/request statuses | Not Started | Shipment pages are placeholders. |
| 4.7 | REQ-SHP-07 | Record shipping container positions | Not Started | Needs shipment content UI and model alignment. |
| 4.8 | REQ-SHP-08 | Ship an entire box | Not Started | Depends on box location and shipment workflows. |
| 4.9 | REQ-RPT-04 | Generate shipment manifest | Not Started | Start with CSV export before PDF. |

**Exit Criteria**

- [ ] A shipment request can be imported and matched
- [ ] Availability can be reviewed before shipping
- [ ] Shipment approval and status tracking work
- [ ] Users can create and export a shipment manifest

---

## Phase 5: Discard and Approval Workflows

**Goal:** Implement governance-heavy operations separately from basic movement so they are testable and auditable.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 5.1 | REQ-MOV-07 | Sample discard workflow | Not Started | Current codebase has no discard UI or approval logic. |
| 5.2 | REQ-MOV-07 | ED, Regulatory, and PI approval capture | Not Started | The existing SQL script references approval tables not present in EF models. Resolve schema first. |
| 5.3 | REQ-RPT-03 | Audit logging for discard and shipment approvals | Not Started | This should not ship without real audit capture. |

**Exit Criteria**

- [ ] Samples can be routed into a discard workflow without immediate deletion
- [ ] Required approvers can record decisions
- [ ] Approval history is auditable

---

## Phase 6: Special Sample Rules

**Goal:** Implement the domain rules that distinguish STASIS from a generic inventory system.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 6.1 | REQ-SPL-01 | Track remaining filter paper spots | In Progress | `RemainingSpots` exists, but usage events do not. |
| 6.2 | REQ-SPL-02 | Record up to 4 filter paper usages over time | Not Started | Needs a usage history table, not just a counter. |
| 6.3 | REQ-SPL-03 | Deplete filter paper correctly when fully used or shipped | Not Started | Must integrate with shipments. |
| 6.4 | REQ-SPL-04 | Enforce international/local filter paper limits | Not Started | Business rule layer required. |
| 6.5 | REQ-SPL-05 | Enforce Plasma-1 shipping restriction | In Progress | `AliquotNumber` exists in SQL script, but not in current `Specimen` model. Resolve schema/model mismatch first. |
| 6.6 | REQ-SPL-06 | Support single-aliquot exception approval | Not Started | Depends on approval engine in Phase 5. |

**Exit Criteria**

- [ ] Filter paper usage is tracked historically
- [ ] Filter paper and plasma shipping restrictions are enforced in code
- [ ] Exceptions require approval and are auditable

---

## Phase 7: Non-Functional Work and Production Readiness

**Goal:** Make the system usable in production, not just functionally complete.

| Order | Req ID | Description | Status | Notes |
|-------|--------|-------------|--------|-------|
| 7.1 | NFR-USE-01 | Barcode-scanner-friendly forms and focus handling | Not Started | Add to all operational entry pages. |
| 7.2 | NFR-USE-02 | Color-coded box occupancy view | Not Started | Best added once box search/details exist. |
| 7.3 | NFR-PER-01 | Bulk import performance validation | Not Started | Measure with realistic files after import exists. |
| 7.4 | NFR-PER-02 | Search performance tuning | In Progress | Sample search exists, but not benchmarked or indexed deliberately. |
| 7.5 | NFR-SYS-02 | Backup and restore procedures | Not Started | Required before production cutover. |
| 7.6 | N/A | Automated test project for services and page models | Not Started | No tests currently exist. |
| 7.7 | N/A | Deployment checklist for IIS + PostgreSQL | Not Started | Needed for production consistency. |

**Exit Criteria**

- [ ] Daily operational flows are fast enough for real users
- [ ] Backup/restore is documented and tested
- [ ] Critical workflows have automated test coverage
- [ ] Production deployment is repeatable

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
