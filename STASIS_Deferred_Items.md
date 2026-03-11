# STASIS Deferred & Pending Items

**Last Updated:** March 11, 2026
**Purpose:** Track all items from Phases 1–4 that were partially implemented or deferred, plus all items from Phases 5–7 that have not been started. Use this as a checklist when returning to unfinished work.

---

## Partially Implemented (Phases 1–4)

These items have some groundwork in place but need additional work to be fully functional.

### 1. Email Distribution of Availability Report (Phase 4, item 4.4)

- **What exists:** `EmailSender` service is registered; availability report renders on-screen after CSV import and in batch detail view.
- **What's missing:** No code connects `EmailSender` to the shipment workflow. After import, the system should email the availability report to Regulatory, the Requestor, and PIs.
- **Where to implement:** `ShipmentService.cs` (after `ImportBatchFromCsvAsync`) or a new handler in `Shipments/Create.cshtml.cs`.
- **Depends on:** A working SMTP configuration in `appsettings.json` or user secrets.

### 2. Whole-Box Shipping UI (Phase 4, item 4.8)

- **What exists:** `Shipment` model has `IsEntireBox` (bool) and `ShippedBoxID` (int?) fields. The database columns are ready.
- **What's missing:** No UI to select "ship entire box" during the Ship action. The Ship form only handles individual specimen shipping from matched requests.
- **Where to implement:** `Pages/Shipments/History.cshtml` (Ship form section) and `ShipmentService.ShipBatchAsync`.
- **Behavior:** When shipping an entire box, all specimens in that box should be marked Shipped, and `IsEntireBox = true` / `ShippedBoxID` should be set on the Shipment record.

### 3. Box Location Lookup from Sample Search Results (Phase 2, item 2.6)

- **What exists:** Sample search results show specimen location details (freezer/rack/box/position) in expandable rows.
- **What's missing:** No direct "jump to box view" link from specimen search results. Users cannot click a box label in search results to navigate to `Boxes/Search?label=BOX-001`.
- **Where to implement:** `Pages/Samples.cshtml` — add an `<a>` link on the box label in the expandable detail row.

### 4. Search Results Operational Lookup (Phase 2, exit criteria)

- **What exists:** Search works with filters for barcode, study, and sample type.
- **What's missing:** The exit criteria "Search results support real operational lookup" is unchecked. This likely means verifying the search works well with real data volumes (not just seeded demo data) and includes all fields operators need (status, collection date, etc.).
- **Where to verify:** Test with realistic data and confirm the expandable details show everything needed for daily operations.

### 5. Audit Trail Coverage Gaps

- **What exists:** `IAuditService`/`AuditService` is implemented and used by Freezers, Racks, Studies CRUD, and specimen import.
- **What's partially missing:** Not all write operations log to the audit trail. Specifically:
  - Specimen **Add** (individual) — audit entry written
  - Specimen **Import** (bulk) — audit entry written
  - Box **Move/Place/Rebox** — audit entries written via `StorageService`
  - Shipment **approval** — no audit entry (only updates `tbl_Approvals`)
  - Shipment **ship action** — no audit entry (only creates `tbl_Shipments`/`tbl_ShipmentContents`)
  - User management (create/edit/role change) — no audit entries
- **Where to implement:** `ShipmentService.cs` (ApproveBatchAsync, ShipBatchAsync), `Pages/Administration/CreateUser.cshtml.cs`, `EditUser.cshtml.cs`.

### 6. Staging Status for Specimens (Phase 4, item 4.6)

- **What exists:** Specimens go directly from their current status to "Shipped" when the Ship action is executed.
- **What's missing:** No intermediate "Staged" status. The plan mentions specimens should be set to "Staged" before the actual shipment is recorded.
- **Where to implement:** `ShipmentService.cs` — add a `StageBatchAsync` method that sets matched specimens to Status="Staged" after approval; the Ship action then transitions from Staged to Shipped.

---

## Not Started (Phase 5: Discard and Approval Workflows)

| Item | Description | Key Files |
|------|-------------|-----------|
| 5.1 | **Discard request UI** — initiate discard from specimen detail or list; creates an Approval record with `ApprovalType = "Discard"` | New `Pages/Samples/Discard.cshtml(.cs)` or action on existing pages |
| 5.2 | **Discard approval capture** — ED, Regulatory, and PI can approve/reject discard requests with comments; all three required | New `Pages/Administration/Approvals.cshtml(.cs)` |
| 5.3 | **Execute discard** — once all approved, set `Specimen.Status = "Discarded"`, link `DiscardApprovalID`, write audit log | `ISampleService` / `SampleService` — add `DiscardSpecimen` method |
| 5.4 | **Single aliquot exception** — if Plasma specimen is the last aliquot, require `ApprovalType = "SingleAliquotException"` before shipping | `ShipmentService.cs` — validation in ship/stage logic |

---

## Not Started (Phase 6: Special Sample Rules)

| Item | Description | Key Files |
|------|-------------|-----------|
| 6.1 | **Filter paper spot tracking** — track `RemainingSpots` decrement on each partial shipment; `RemainingSpots` field exists but is never decremented | `ShipmentService.cs`, `SampleService.cs` |
| 6.2 | **Filter paper usage history** — record each usage event in `tbl_FilterPaperUsage` (table and model exist) | `SampleService.cs`, new specimen detail page |
| 6.3 | **Filter paper depletion** — set `Status = "Depleted"` when `RemainingSpots = 0` | `ShipmentService.cs` |
| 6.4 | **International/local spot limits** — enforce max 2 spots international, 2 reserved local (REQ-SPL-04) | `ShipmentService.cs` — validation during staging |
| 6.5 | **Plasma-1 shipping restriction** — only `AliquotNumber = 1` can be shipped outbound; Plasma-2 blocked (REQ-SPL-05). Note: `AliquotNumber` exists in the model. | `ShipmentService.cs` — validation during staging |
| 6.6 | **Single-aliquot exception approval** — depends on approval engine from Phase 5 | Approval service + `ShipmentService.cs` |

---

## Not Started (Phase 7: Non-Functional & Production Readiness)

| Item | Description | Key Files |
|------|-------------|-----------|
| 7.1 | **Barcode scanner optimization** — auto-focus barcode input fields, submit on Enter, no mouse required (NFR-USE-01) | All operational pages (`Add`, `Move`, `Rebox`, `Search`) |
| 7.2 | **Color-coded box occupancy** — CSS classes for occupied/empty/staged/temp cells (NFR-USE-02). Note: `Boxes/Search` already has a color-coded grid; verify it meets requirements. | `Pages/Boxes/Search.cshtml` |
| 7.3 | **Bulk import performance** — test with 1000+ row CSV files, measure time (NFR-PER-01) | `SampleService.ImportSpecimensFromCsv` |
| 7.4 | **Search performance tuning** — benchmark with 1000+ specimens, add DB indexes if needed (NFR-PER-02) | `SampleService.cs`, migration for indexes |
| 7.5 | **Backup/restore procedures** — document PostgreSQL `pg_dump` schedule and restore steps (NFR-SYS-02) | `README.md` or new ops doc |
| 7.6 | **Automated tests** — xUnit integration tests for `SampleService`, `StorageService`, `ShipmentService`; at least one test per major workflow | New `STASIS.Tests` project |
| 7.7 | **Deployment checklist** — environment variables, migration steps, initial seed for production | `README.md` or new deployment doc |

---

## Quick Reference: Priority Order

If returning to this list, the recommended order is:

1. **Audit trail gaps** (item 5 above) — important for compliance, low effort
2. **Box lookup link** (item 3) — quick UX win
3. **Phase 5 discard workflow** — governance requirement
4. **Phase 6 special sample rules** — domain-critical business logic
5. **Email distribution** (item 1) — depends on SMTP setup
6. **Whole-box shipping** (item 2) — nice-to-have refinement
7. **Staging status** (item 6) — workflow refinement
8. **Phase 7 production readiness** — before go-live
