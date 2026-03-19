# STASIS Deferred & Pending Items

**Last Updated:** March 12, 2026
**Purpose:** Track all items across all phases that were partially implemented or deferred. Use this as a checklist when returning to unfinished work.

---

## Partially Implemented

These items have groundwork in place but need additional work to be fully functional.

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

### 3. Staging Status for Specimens (Phase 4, item 4.6)

- **What exists:** Specimens go directly from their current status to "Shipped" when the Ship action is executed.
- **What's missing:** No intermediate "Staged" status.
- **Where to implement:** `ShipmentService.cs` — add a `StageBatchAsync` method that sets matched specimens to Status="Staged" after approval; the Ship action then transitions from Staged to Shipped.

### 4. Single Aliquot Exception (Phase 6, item 6.6)

- **What exists:** Approval model supports `ApprovalType = "SingleAliquotException"` (DB check constraint allows it). Approval engine is implemented (Phase 5). Plasma Aliquot-2 is blocked from outbound shipment (Phase 6).
- **What's missing:** No validation to detect when a Plasma specimen is the **last remaining aliquot** and require a SingleAliquotException approval before shipping.
- **Where to implement:** `ShipmentService.ValidateShipmentAsync` — query for sibling aliquots; if no other aliquot is In-Stock, require exception approval.

### 5. Automated Tests (Phase 7, item 7.6)

- **What exists:** Test guide with priority scenarios and sample code in `STASIS_Test_Plan.md`.
- **What's missing:** No xUnit test project created yet. No tests coded.
- **Where to implement:** Create `STASIS.Tests` project per guide in manual testing doc.

---

## Resolved Items

| Item | Phase | Resolution |
|------|-------|------------|
| Box location lookup from Samples list | Phase 6 | Box labels and barcodes now link to Search and Detail pages |
| Audit trail coverage gaps | Phase 5 | All services and user management now log audit entries |
| Phase 6 special sample rules | Phase 6 | Filter paper tracking, Plasma-2 block, spot limits all implemented |
| Barcode scanner autofocus | Phase 7 | All input fields have `autofocus` |
| Color-coded box grid | Phase 3/7 | Implemented and verified |
| Search performance indexes | Phase 7 | Migration `AddSearchPerformanceIndexes` adds indexes on Status, StudyID, SampleTypeID |
| Backup/restore procedures | Phase 7 | Documented in `STASIS_Test_Plan.md` |
| Deployment checklist | Phase 7 | Documented in `STASIS_Test_Plan.md` |

---

## Manual Testing Required

These items require manual verification and are documented in `STASIS_Test_Plan.md`:

- [ ] Barcode scanner testing with physical scanner hardware
- [ ] Box occupancy grid visual verification
- [ ] Bulk import performance with 1000+ row CSV
- [ ] Search performance benchmarking with 1000+ records
- [ ] Backup script execution and restore verification
- [ ] Full deployment walkthrough

---

## Quick Reference: Priority Order

If returning to this list, the recommended order is:

1. **Automated tests** (item 5) — safety net before any further changes
2. **Single aliquot exception** (item 4) — governance-critical edge case
3. **Email distribution** (item 1) — depends on SMTP setup
4. **Whole-box shipping** (item 2) — nice-to-have refinement
5. **Staging status** (item 3) — workflow refinement
6. **Manual testing** — run through checklists in `STASIS_Test_Plan.md`
