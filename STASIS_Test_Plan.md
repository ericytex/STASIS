# STASIS Comprehensive Test Plan

**Created:** March 12, 2026
**Last Updated:** March 19, 2026
**Purpose:** Complete manual and functional test plan covering every feature, business rule, and requirement in the STASIS application.

---

## Table of Contents

1. [Authentication & Authorization](#1-authentication--authorization)
2. [User Administration](#2-user-administration)
3. [Sample Intake — Add](#3-sample-intake--add)
4. [Sample Intake — CSV Import](#4-sample-intake--csv-import)
5. [Sample Search & Detail](#5-sample-search--detail)
6. [Storage Management — Box Search](#6-storage-management--box-search)
7. [Storage Management — Move Specimen](#7-storage-management--move-specimen)
8. [Storage Management — Place Box](#8-storage-management--place-box)
9. [Storage Management — Rebox](#9-storage-management--rebox)
10. [Shipment Workflow — Import & Create](#10-shipment-workflow--import--create)
11. [Shipment Workflow — Approval](#11-shipment-workflow--approval)
12. [Shipment Workflow — Ship & Manifest](#12-shipment-workflow--ship--manifest)
13. [Discard Workflow](#13-discard-workflow)
14. [Approval Dashboard](#14-approval-dashboard)
15. [Special Sample Rules — Filter Paper](#15-special-sample-rules--filter-paper)
16. [Special Sample Rules — Plasma](#16-special-sample-rules--plasma)
17. [Lab Setup — Freezers & Racks](#17-lab-setup--freezers--racks)
18. [Lab Setup — Studies](#18-lab-setup--studies)
19. [Lab Setup — Box Types](#19-lab-setup--box-types)
20. [Audit Trail](#20-audit-trail)
21. [Barcode Scanner & Usability](#21-barcode-scanner--usability)
22. [Performance](#22-performance)
23. [Backup & Restore](#23-backup--restore)
24. [Deployment](#24-deployment)
25. [Known Limitations & Deferred Items](#25-known-limitations--deferred-items)
26. [Requirements Traceability Matrix](#26-requirements-traceability-matrix)

---

## 1. Authentication & Authorization

**Requirements:** REQ-SEC-01, REQ-SEC-02

### 1.1 Login and Session

- [ ] **TC-AUTH-01:** Navigate to any page while logged out — verify redirect to the login page.
- [ ] **TC-AUTH-02:** Log in with `admin@stasis.com` and the configured `AdminSeedPassword` — verify successful login and redirect to home page.
- [ ] **TC-AUTH-03:** Log in with an incorrect password — verify an error message is shown and login is denied.
- [ ] **TC-AUTH-04:** Log out — verify redirect to the login page and that protected pages are no longer accessible.
- [ ] **TC-AUTH-05:** After login, verify the session persists across page navigation (no re-authentication required).

### 1.2 Password Change Enforcement

- [ ] **TC-AUTH-06:** Create a new user (see section 2). Log in as that user — verify immediate redirect to the Change Password page.
- [ ] **TC-AUTH-07:** While forced to change password, attempt to navigate to `/Samples` — verify redirect back to Change Password.
- [ ] **TC-AUTH-08:** While forced to change password, verify the Logout link is still accessible.
- [ ] **TC-AUTH-09:** Change the password successfully — verify `MustChangePassword` is cleared and the user can navigate freely.

### 1.3 Role-Based Access Control

#### Read Role

- [ ] **TC-AUTH-10:** Log in as a Read user. Navigate to `/Samples` — verify access is granted.
- [ ] **TC-AUTH-11:** Navigate to `/Boxes/Search` — verify access is granted.
- [ ] **TC-AUTH-12:** Navigate to `/Shipments/History` — verify access is granted.
- [ ] **TC-AUTH-13:** Navigate to `/Samples/Detail?id=1` — verify access is granted.
- [ ] **TC-AUTH-14:** Navigate to `/Samples/Add` — verify access is denied (403 or redirect).
- [ ] **TC-AUTH-15:** Navigate to `/Samples/Import` — verify access is denied.
- [ ] **TC-AUTH-16:** Navigate to `/Boxes/Move` — verify access is denied.
- [ ] **TC-AUTH-17:** Navigate to `/Boxes/Place` — verify access is denied.
- [ ] **TC-AUTH-18:** Navigate to `/Boxes/Rebox` — verify access is denied.
- [ ] **TC-AUTH-19:** Navigate to `/Shipments/Create` — verify access is denied.
- [ ] **TC-AUTH-20:** Navigate to `/Samples/Discard` — verify access is denied.
- [ ] **TC-AUTH-21:** Navigate to `/Administration/Users` — verify access is denied.
- [ ] **TC-AUTH-22:** Navigate to `/LabSetup/Freezers` — verify access is denied.

#### Write Role

- [ ] **TC-AUTH-23:** Log in as a Write user. Verify access to `/Samples/Add`, `/Samples/Import`, `/Boxes/Move`, `/Boxes/Place`, `/Boxes/Rebox`, `/Shipments/Create`, `/Samples/Discard`.
- [ ] **TC-AUTH-24:** Navigate to `/Administration/Users` — verify access is denied.
- [ ] **TC-AUTH-25:** Navigate to `/LabSetup/Freezers` — verify access is denied.
- [ ] **TC-AUTH-26:** Navigate to `/Administration/Approvals` — verify access is denied.

#### Admin Role

- [ ] **TC-AUTH-27:** Log in as Admin. Verify access to all pages including `/Administration/*` and `/LabSetup/*`.

---

## 2. User Administration

**Requirements:** REQ-SEC-01, REQ-SEC-02

### 2.1 List Users

- [ ] **TC-USR-01:** Navigate to `/Administration/Users` — verify all user accounts are listed with email, role, and department.

### 2.2 Create User

- [ ] **TC-USR-02:** Create a new user with Email, Password, Department, and Role (Read) — verify success message and user appears in the list.
- [ ] **TC-USR-03:** Attempt to create a user with an email that already exists — verify an error is shown.
- [ ] **TC-USR-04:** Attempt to create a user with a weak password (e.g., `abc`) — verify password policy errors are shown (min 8 chars, uppercase, lowercase, digit, non-alphanumeric).
- [ ] **TC-USR-05:** Verify the new user has `MustChangePassword = true` (log in as the new user and confirm redirect to change password).
- [ ] **TC-USR-06:** Verify the new user has `EmailConfirmed = true` (they can log in immediately without email verification).
- [ ] **TC-USR-07:** Verify an audit entry is created for the new user (check `/Administration/Audit`).

### 2.3 Edit User

- [ ] **TC-USR-08:** Edit a user's role from Read to Write — verify the change is saved and the user now has Write access.
- [ ] **TC-USR-09:** Edit a user's department — verify the change is saved.
- [ ] **TC-USR-10:** Toggle `CanApproveShipments` and `CanApproveDiscards` — verify the changes are saved.
- [ ] **TC-USR-11:** Verify an audit entry is created for the edit.

### 2.4 Reset Password

- [ ] **TC-USR-12:** Reset a user's password — verify success message.
- [ ] **TC-USR-13:** After reset, log in as that user — verify they are forced to change their password (`MustChangePassword` is set to true).
- [ ] **TC-USR-14:** Verify an audit entry is created for the password reset.

---

## 3. Sample Intake — Add

**Requirements:** REQ-ACC-01, REQ-ACC-03, REQ-ACC-04, REQ-INV-03, REQ-INV-04

### 3.1 Form Display

- [ ] **TC-ADD-01:** Navigate to `/Samples/Add` — verify all fields are present: BarcodeID, LegacyID, Study (dropdown), Sample Type (dropdown), Collection Date, Box (dropdown with Freezer > Rack context), Position Row, Position Col.
- [ ] **TC-ADD-02:** Verify Study and Sample Type dropdowns are populated from the database.
- [ ] **TC-ADD-03:** Verify Box dropdown shows labels with freezer and rack context (e.g., `BOX-001 (Freezer-A > Rack-1)`).
- [ ] **TC-ADD-04:** Verify the BarcodeID field has autofocus.

### 3.2 Barcode Validation

- [ ] **TC-ADD-05:** Type a barcode that already exists — verify real-time AJAX validation shows a "taken" warning before form submission.
- [ ] **TC-ADD-06:** Submit the form with a duplicate barcode — verify a server-side error message is shown.
- [ ] **TC-ADD-07:** Submit the form with an empty barcode — verify a required-field validation error.

### 3.3 Position Conflict Detection

- [ ] **TC-ADD-08:** Select a box — verify occupied positions are displayed via AJAX (e.g., as a list or visual indicator).
- [ ] **TC-ADD-09:** Submit the form with a position that is already occupied — verify a conflict error is shown (caught from `DbUpdateException`).

### 3.4 Plasma-Specific Fields

- [ ] **TC-ADD-10:** Select Sample Type = "Plasma" — verify the Aliquot Number field appears (values: 1 or 2).
- [ ] **TC-ADD-11:** Submit a Plasma specimen with AliquotNumber = 1 — verify success.
- [ ] **TC-ADD-12:** Submit a Plasma specimen with AliquotNumber = 2 — verify success.

### 3.5 Filter Paper-Specific Fields

- [ ] **TC-ADD-13:** Select Sample Type = "Filter Paper" — verify the Remaining Spots field appears.
- [ ] **TC-ADD-14:** Submit a Filter Paper specimen — verify `RemainingSpots` is saved (default 4 if not specified).

### 3.6 Successful Add

- [ ] **TC-ADD-15:** Fill in all fields with valid data and submit — verify success message, redirect to `/Samples`, and the new specimen appears in search results.
- [ ] **TC-ADD-16:** Verify the specimen's Status is set to "In-Stock".
- [ ] **TC-ADD-17:** Verify the Collection Date is stored in UTC.
- [ ] **TC-ADD-18:** Verify an audit entry is created for the new specimen.

### 3.7 Optional Fields

- [ ] **TC-ADD-19:** Submit a specimen without LegacyID, Study, Collection Date — verify success (these are optional).

---

## 4. Sample Intake — CSV Import

**Requirements:** REQ-ACC-02, REQ-ACC-04, REQ-INV-04

### 4.1 Upload and Preview

- [ ] **TC-IMP-01:** Navigate to `/Samples/Import`. Upload a valid CSV file — verify a preview is shown with parsed rows before commit.
- [ ] **TC-IMP-02:** Verify the CSV format is: `BarcodeID,LegacyID,StudyCode,SampleType,CollectionDate,BoxLabel,PositionRow,PositionCol`.
- [ ] **TC-IMP-03:** Upload a CSV with a header row containing "barcode" (case-insensitive) — verify the header is skipped during parsing.
- [ ] **TC-IMP-04:** Upload a CSV without a header row — verify all rows are parsed as data.

### 4.2 Validation — Barcodes

- [ ] **TC-IMP-05:** Include a row with an empty BarcodeID — verify an error is reported for that row.
- [ ] **TC-IMP-06:** Include a row with a barcode that already exists in the database — verify an error is reported.
- [ ] **TC-IMP-07:** Include two rows with the same barcode within the file — verify a duplicate error is reported on the second row.

### 4.3 Validation — Lookups

- [ ] **TC-IMP-08:** Include a row with a StudyCode that does not exist — verify an error is reported.
- [ ] **TC-IMP-09:** Include a row with a SampleType that does not exist — verify an error is reported.
- [ ] **TC-IMP-10:** Include a row with a BoxLabel that does not exist — verify an error is reported.
- [ ] **TC-IMP-11:** Verify StudyCode lookup is case-insensitive (e.g., `study1` matches `STUDY1`).
- [ ] **TC-IMP-12:** Verify SampleType lookup is case-insensitive.
- [ ] **TC-IMP-13:** Verify BoxLabel lookup is case-insensitive.

### 4.4 Validation — Positions

- [ ] **TC-IMP-14:** Include a row with a non-integer PositionRow or PositionCol — verify an error.
- [ ] **TC-IMP-15:** Include a row with a position already occupied in the database — verify an error.
- [ ] **TC-IMP-16:** Include two rows with the same BoxLabel + PositionRow + PositionCol within the file — verify a duplicate error.

### 4.5 Validation — Special Types

- [ ] **TC-IMP-17:** Import a Filter Paper specimen — verify `RemainingSpots` is automatically set to 4.

### 4.6 CSV Parsing Edge Cases

- [ ] **TC-IMP-18:** Upload a CSV with quoted fields containing commas (e.g., `"Smith, John"`) — verify correct parsing.
- [ ] **TC-IMP-19:** Upload a CSV with escaped quotes inside fields (e.g., `"He said ""hello"""`) — verify correct parsing.

### 4.7 Commit

- [ ] **TC-IMP-20:** After preview, click Commit — verify all valid rows are imported and a success message shows the count.
- [ ] **TC-IMP-21:** After commit, search for imported specimens in `/Samples` — verify they appear with correct data.
- [ ] **TC-IMP-22:** Verify audit entries are created for each imported specimen.
- [ ] **TC-IMP-23:** Upload a file with a mix of valid and invalid rows — verify only valid rows are importable and errors are clearly shown for invalid rows.

### 4.8 Error Cases

- [ ] **TC-IMP-24:** Attempt to submit with no file selected — verify an error message.

---

## 5. Sample Search & Detail

**Requirements:** REQ-RPT-01

### 5.1 Search and Filtering

- [ ] **TC-SRC-01:** Navigate to `/Samples` — verify the search form shows fields for Barcode, Study (dropdown), and Sample Type (dropdown).
- [ ] **TC-SRC-02:** Search by barcode substring — verify matching results are returned.
- [ ] **TC-SRC-03:** Filter by Study — verify only specimens from that study are shown.
- [ ] **TC-SRC-04:** Filter by Sample Type — verify only specimens of that type are shown.
- [ ] **TC-SRC-05:** Combine barcode + study + sample type filters — verify results match all criteria.
- [ ] **TC-SRC-06:** Search with no filters — verify all specimens are shown (paginated).
- [ ] **TC-SRC-07:** Search for a barcode that does not exist — verify no results and no error.

### 5.2 Pagination

- [ ] **TC-SRC-08:** With more than 25 specimens, verify pagination controls appear.
- [ ] **TC-SRC-09:** Click Next — verify the next page of results loads.
- [ ] **TC-SRC-10:** Click Previous — verify the previous page loads.
- [ ] **TC-SRC-11:** Verify page item counts are correct (e.g., "Showing 1-25 of 100").

### 5.3 Results Display

- [ ] **TC-SRC-12:** Verify each result shows: Barcode, Study, Sample Type, Box Label, Position, Status.
- [ ] **TC-SRC-13:** Click a barcode link — verify navigation to `/Samples/Detail` for that specimen.
- [ ] **TC-SRC-14:** Click a box label link — verify navigation to `/Boxes/Search?boxLabel=XXX` and the box detail loads automatically.

### 5.4 Specimen Detail Page

- [ ] **TC-SRC-15:** Navigate to `/Samples/Detail?id=N` for a valid specimen — verify all fields are displayed: barcode, legacy ID, study, sample type, collection date, box, freezer, rack, position, status.
- [ ] **TC-SRC-16:** Navigate to `/Samples/Detail?id=99999` (non-existent) — verify a 404 Not Found response.
- [ ] **TC-SRC-17:** View a Filter Paper specimen — verify RemainingSpots, SpotsShippedInternational, SpotsReservedLocal, and usage history are displayed.
- [ ] **TC-SRC-18:** View a Plasma specimen — verify AliquotNumber is displayed.
- [ ] **TC-SRC-19:** View a specimen with shipment history — verify shipment records are listed.
- [ ] **TC-SRC-20:** View a specimen with a linked discard approval — verify the discard status is shown.
- [ ] **TC-SRC-21:** Verify the "View Box" button links to `/Boxes/Search?boxLabel=XXX`.
- [ ] **TC-SRC-22:** Verify the "Back to Samples" button returns to the samples list.

---

## 6. Storage Management — Box Search

**Requirements:** REQ-RPT-02, NFR-USE-02

### 6.1 Search

- [ ] **TC-BOX-01:** Navigate to `/Boxes/Search` — verify the search form shows a Box Label input (with autocomplete datalist) and a Freezer dropdown.
- [ ] **TC-BOX-02:** Type a partial box label — verify the datalist shows matching suggestions from all boxes in the system.
- [ ] **TC-BOX-03:** Search by box label — verify matching boxes are listed with Label, Type, Category, Freezer, Rack, and specimen count.
- [ ] **TC-BOX-04:** Filter by freezer — verify only boxes in that freezer are shown.
- [ ] **TC-BOX-05:** Combine box label and freezer filters — verify results match both criteria.
- [ ] **TC-BOX-06:** Verify search results are limited to 100 boxes.

### 6.2 Deep Link (from Samples page)

- [ ] **TC-BOX-07:** Click a box label link on the Samples page (e.g., `/Boxes/Search?boxLabel=BOX-001`) — verify the box label input is pre-populated, the search runs automatically, and the box detail is displayed (auto-select when only one result).
- [ ] **TC-BOX-08:** Click the "View Box" button on the Samples/Detail page — verify the same behavior.

### 6.3 Box Detail View

- [ ] **TC-BOX-09:** Click "View" on a search result — verify the box detail section appears below.
- [ ] **TC-BOX-10:** Verify the detail shows: Box Label, Box Type, Box Category, Freezer, Rack, occupancy count (e.g., "Occupied: 5 / 81").
- [ ] **TC-BOX-11:** Verify the color-coded occupancy grid renders correctly:
  - Green cells = Occupied (status "In-Stock")
  - Grey cells = Empty
  - Yellow cells = Staged
  - Blue cells = Temp
- [ ] **TC-BOX-12:** Verify grid size matches box type: 9x9 for "81-slot", 10x10 for "100-slot".
- [ ] **TC-BOX-13:** Hover over a specimen cell — verify tooltip shows full barcode, sample type, and status.
- [ ] **TC-BOX-14:** Verify the specimen list below the grid shows: Barcode, Type, Position (row, col), Status — ordered by row then column.

### 6.4 Edge Cases

- [ ] **TC-BOX-15:** Search for a box with no specimens — verify an empty grid (all grey) and "Occupied: 0 / 81".
- [ ] **TC-BOX-16:** Search for a box that is unassigned (no freezer/rack) — verify "Unassigned" is shown for Freezer and Rack.

---

## 7. Storage Management — Move Specimen

**Requirements:** REQ-MOV-01, REQ-MOV-06, REQ-INV-04

### 7.1 Barcode Lookup

- [ ] **TC-MOV-01:** Navigate to `/Boxes/Move`. Enter a valid barcode — verify the specimen's current location is displayed (box, freezer, rack, position).
- [ ] **TC-MOV-02:** Enter a barcode that does not exist — verify a "not found" message.
- [ ] **TC-MOV-03:** Verify the barcode input has autofocus.

### 7.2 Move to New Position

- [ ] **TC-MOV-04:** After lookup, select a destination box, row, and column. Submit — verify success message and redirect.
- [ ] **TC-MOV-05:** Verify the specimen's new location in `/Boxes/Search` or `/Samples`.
- [ ] **TC-MOV-06:** Attempt to move to a position that is already occupied — verify an error message (position conflict from DbUpdateException).
- [ ] **TC-MOV-07:** Verify an audit entry is created with old and new location.

### 7.3 Move to Temp

- [ ] **TC-MOV-08:** Check the "Move to Temp" checkbox and submit — verify success.
- [ ] **TC-MOV-09:** Verify the specimen's Status changes to "Temp".
- [ ] **TC-MOV-10:** Verify the specimen's PositionRow and PositionCol are cleared (null).
- [ ] **TC-MOV-11:** Verify the specimen is assigned to a Temp-category box.
- [ ] **TC-MOV-12:** Verify an audit entry is created for the temp move.

### 7.4 Temp Recovery

- [ ] **TC-MOV-13:** Move a specimen that has Status="Temp" to a new box and position — verify Status changes back to "In-Stock".

### 7.5 Empty Box Auto-Unassign

- [ ] **TC-MOV-14:** Move the last specimen out of a Standard-category box — verify the box's RackID is set to null (unassigned from rack).
- [ ] **TC-MOV-15:** Move the last specimen out of a Temp-category box — verify the box's RackID is NOT cleared (auto-unassign only applies to Standard boxes).

---

## 8. Storage Management — Place Box

**Requirements:** REQ-INV-02, REQ-MOV-03

### 8.1 Box Lookup

- [ ] **TC-PLC-01:** Navigate to `/Boxes/Place`. Enter a valid box label — verify the box's current location (freezer, rack) is displayed.
- [ ] **TC-PLC-02:** Enter a box label that does not exist — verify a "not found" message.
- [ ] **TC-PLC-03:** Verify the box label input has autofocus.

### 8.2 Cascading Dropdowns

- [ ] **TC-PLC-04:** Select a freezer — verify the rack dropdown is populated via AJAX with racks from that freezer only.
- [ ] **TC-PLC-05:** Change the freezer — verify the rack dropdown updates to the new freezer's racks.

### 8.3 Move Box

- [ ] **TC-PLC-06:** Select a destination freezer and rack, then submit — verify success message and redirect.
- [ ] **TC-PLC-07:** Navigate to `/Boxes/Search` and find the box — verify the new freezer and rack are shown.
- [ ] **TC-PLC-08:** Verify an audit entry is created with old and new RackID.

---

## 9. Storage Management — Rebox

**Requirements:** REQ-MOV-02, REQ-MOV-05

### 9.1 Setup

- [ ] **TC-RBX-01:** Navigate to `/Boxes/Rebox` — verify a destination box dropdown and barcode scan input are shown.
- [ ] **TC-RBX-02:** Select a destination box from the dropdown.

### 9.2 Barcode Scanning Session

- [ ] **TC-RBX-03:** Scan (type + Enter) a valid barcode — verify the specimen is added to the scanned list without a page reload (AJAX lookup).
- [ ] **TC-RBX-04:** Verify the scanned list shows: Barcode, Sample Type, Current Box, Current Position, Status.
- [ ] **TC-RBX-05:** Verify row and column auto-increment for each scanned specimen (e.g., row 1 col 1, row 1 col 2, etc.).
- [ ] **TC-RBX-06:** Verify the row/col values are editable before commit.
- [ ] **TC-RBX-07:** Scan a barcode that does not exist — verify an error message in the UI.
- [ ] **TC-RBX-08:** Continue scanning until the box is full — verify behavior is correct.

### 9.3 Commit

- [ ] **TC-RBX-09:** Click Commit — verify all scanned specimens are moved to the destination box with their assigned positions.
- [ ] **TC-RBX-10:** Verify a success message shows the count of reboxed specimens and redirect to `/Boxes/Search`.
- [ ] **TC-RBX-11:** Attempt to commit with a position conflict (two specimens in the same slot) — verify an error message.
- [ ] **TC-RBX-12:** Verify specimens previously with Status="Temp" are updated to "In-Stock" after rebox.
- [ ] **TC-RBX-13:** Verify empty source boxes (Standard category) have their RackID cleared.
- [ ] **TC-RBX-14:** Verify an audit entry is created for the rebox operation.

### 9.4 Edge Cases

- [ ] **TC-RBX-15:** Attempt to commit with no scanned specimens — verify an error or no-op.

---

## 10. Shipment Workflow — Import & Create

**Requirements:** REQ-SHP-02, REQ-SHP-03, REQ-SHP-04

### 10.1 CSV Import

- [ ] **TC-SHP-01:** Navigate to `/Shipments/Create`. Upload a valid shipment request CSV with Requestor Name and Email — verify the batch is created and an availability report is displayed.
- [ ] **TC-SHP-02:** Verify the CSV format is: `Barcode,SampleType,RequestorName,RequestDate`.
- [ ] **TC-SHP-03:** Verify the batch record includes: ImportDate, ImportedByUserId, RequestorName, RequestorEmail.

### 10.2 Auto-Matching

- [ ] **TC-SHP-04:** Include a barcode that exists and is In-Stock — verify status = "Pending" (available), `TotalAvailable` increments.
- [ ] **TC-SHP-05:** Include a barcode that does not exist in the system — verify status = "Not Found", `TotalNotFound` increments.
- [ ] **TC-SHP-06:** Include a barcode for a specimen with Status="Shipped" — verify status = "Previously Shipped", `TotalPreviouslyShipped` increments.
- [ ] **TC-SHP-07:** Include a barcode for a specimen with Status="Discarded" — verify status = "Discarded", `TotalDiscarded` increments.
- [ ] **TC-SHP-08:** Include a barcode for a specimen with Status="Not Yet Received" — verify status = "Not Yet Received", `TotalNotYetReceived` increments.

### 10.3 Availability Report

- [ ] **TC-SHP-09:** Verify the availability summary shows counts for: Available, Not Found, Previously Shipped, Discarded, Not Yet Received.
- [ ] **TC-SHP-10:** Verify each shipment request row shows: Barcode, Sample Type, Requestor, Date, Status, and matched specimen info.

### 10.4 Approval Record

- [ ] **TC-SHP-11:** After import, verify an Approval record is created with Type="Shipment", OverallStatus="Pending".
- [ ] **TC-SHP-12:** Verify the batch status is "Pending Approval".
- [ ] **TC-SHP-13:** Verify an audit entry is created for the import.

### 10.5 Error Cases

- [ ] **TC-SHP-14:** Attempt to submit with no file — verify an error.

---

## 11. Shipment Workflow — Approval

**Requirements:** REQ-SHP-01

### 11.1 Approval Submission

- [ ] **TC-APR-01:** Navigate to `/Shipments/History`, select a batch in "Pending Approval" status — verify the approval form is shown with Level (ED/Regulatory/PI) and Status (Approved/Rejected) fields.
- [ ] **TC-APR-02:** Submit an ED approval as "Approved" — verify the approval is saved with approver, date, and comments.
- [ ] **TC-APR-03:** Submit a Regulatory approval as "Approved" — verify the approval is saved.

### 11.2 Overall Status Calculation — Shipment

- [ ] **TC-APR-04:** Approve both ED and Regulatory — verify OverallStatus becomes "Approved" and batch Status becomes "Approved". (PI is optional for shipments.)
- [ ] **TC-APR-05:** Reject at the ED level — verify OverallStatus becomes "Rejected" regardless of other levels.
- [ ] **TC-APR-06:** Reject at the Regulatory level — verify OverallStatus becomes "Rejected".
- [ ] **TC-APR-07:** Approve ED only, Regulatory still pending — verify OverallStatus remains "Pending".
- [ ] **TC-APR-08:** Verify approval comments are stored and displayed.
- [ ] **TC-APR-09:** Verify an audit entry is created for each approval action.

---

## 12. Shipment Workflow — Ship & Manifest

**Requirements:** REQ-SHP-06, REQ-SHP-07, REQ-RPT-04

### 12.1 Ship Action

- [ ] **TC-SHIP-01:** Navigate to the detail of an "Approved" batch — verify the Ship form is shown with: Courier, Tracking Number, Destination, International checkbox, Filter Paper Spots input.
- [ ] **TC-SHIP-02:** Fill in shipping details and submit — verify success message.
- [ ] **TC-SHIP-03:** Verify a Shipment record is created with correct date, courier, tracking number, destination, and ShippedByUserId.
- [ ] **TC-SHIP-04:** Verify all matched specimens with Status="Pending" have their Status changed to "Shipped".
- [ ] **TC-SHIP-05:** Verify ShipmentContent records are created for each shipped specimen.
- [ ] **TC-SHIP-06:** Verify the batch Status changes to "Shipped".
- [ ] **TC-SHIP-07:** Verify an audit entry is created for the shipment.

### 12.2 Manifest Download

- [ ] **TC-SHIP-08:** After shipping, click the manifest download link — verify a CSV file is downloaded.
- [ ] **TC-SHIP-09:** Verify the manifest CSV contains: Barcode, SampleType, ConditionAtShipment, ShippingBoxPosition, SpotsUsed.
- [ ] **TC-SHIP-10:** Verify the downloaded file name and content type are correct.

### 12.3 History Page

- [ ] **TC-SHIP-11:** Navigate to `/Shipments/History` — verify all batches are listed, ordered by import date (newest first).
- [ ] **TC-SHIP-12:** Click on a batch — verify the detail view shows requests, approval status grid, and shipment info.

---

## 13. Discard Workflow

**Requirements:** REQ-MOV-07

### 13.1 Barcode Lookup

- [ ] **TC-DIS-01:** Navigate to `/Samples/Discard`. Enter one or more barcodes (one per line) and click Look Up.
- [ ] **TC-DIS-02:** Verify found specimens are listed with their details.
- [ ] **TC-DIS-03:** Verify barcodes not found in the system are shown in a "Not Found" list.
- [ ] **TC-DIS-04:** Verify specimens already Discarded are shown as invalid with reason.
- [ ] **TC-DIS-05:** Verify specimens already Shipped are shown as invalid with reason.
- [ ] **TC-DIS-06:** Verify specimens with a pending discard approval are shown as invalid with reason.

### 13.2 Discard Request

- [ ] **TC-DIS-07:** Select valid specimens and submit the discard request — verify success message with approval ID and specimen count.
- [ ] **TC-DIS-08:** Verify an Approval record is created with Type="Discard", OverallStatus="Pending".
- [ ] **TC-DIS-09:** Verify each selected specimen has its `DiscardApprovalID` set.
- [ ] **TC-DIS-10:** Verify an audit entry is created for the discard request.

### 13.3 Discard Approval (3-Level)

- [ ] **TC-DIS-11:** Navigate to `/Administration/Approvals`. Find the discard approval — verify it shows as Pending.
- [ ] **TC-DIS-12:** Submit ED approval as "Approved" — verify OverallStatus remains "Pending" (Regulatory and PI still needed).
- [ ] **TC-DIS-13:** Submit Regulatory approval as "Approved" — verify OverallStatus remains "Pending" (PI still needed).
- [ ] **TC-DIS-14:** Submit PI approval as "Approved" — verify OverallStatus becomes "Approved".
- [ ] **TC-DIS-15:** Reject at any level (ED, Regulatory, or PI) — verify OverallStatus becomes "Rejected".

### 13.4 Execute Discard

- [ ] **TC-DIS-16:** On an approved discard, click Execute — verify specimens are set to Status="Discarded".
- [ ] **TC-DIS-17:** Attempt to execute a discard that is not yet approved — verify the action is blocked.
- [ ] **TC-DIS-18:** After discard, search for the specimens — verify their Status is "Discarded".
- [ ] **TC-DIS-19:** Verify audit entries are created for each discarded specimen.

### 13.5 Edge Cases

- [ ] **TC-DIS-20:** Enter barcodes separated by commas — verify correct parsing.
- [ ] **TC-DIS-21:** Enter barcodes with extra whitespace — verify correct parsing.
- [ ] **TC-DIS-22:** Enter an empty barcode list — verify appropriate handling.

---

## 14. Approval Dashboard

**Requirements:** REQ-SHP-01, REQ-MOV-07

- [ ] **TC-DASH-01:** Navigate to `/Administration/Approvals` — verify the default filter shows only "Pending" approvals.
- [ ] **TC-DASH-02:** Filter by Type = "Discard" — verify only discard approvals are shown.
- [ ] **TC-DASH-03:** Filter by Type = "Shipment" — verify only shipment approvals are shown.
- [ ] **TC-DASH-04:** Filter by Status = "Approved" — verify only approved approvals are shown.
- [ ] **TC-DASH-05:** Filter by Status = "Rejected" — verify only rejected approvals are shown.
- [ ] **TC-DASH-06:** Clear filters — verify all approvals are shown.
- [ ] **TC-DASH-07:** Verify each approval row shows: ID, Type, Requested Date, Requested By, Overall Status, ED/Regulatory/PI statuses, Item Count.
- [ ] **TC-DASH-08:** Click on an approval — verify the detail view shows full approval info, linked specimens (for discards), or linked batches (for shipments).

---

## 15. Special Sample Rules — Filter Paper

**Requirements:** REQ-SPL-01, REQ-SPL-02, REQ-SPL-03, REQ-SPL-04

### 15.1 Spot Tracking

- [ ] **TC-FP-01:** Add a Filter Paper specimen — verify `RemainingSpots = 4` (or the value entered).
- [ ] **TC-FP-02:** Ship a Filter Paper specimen with 1 spot — verify `RemainingSpots` decrements by 1.
- [ ] **TC-FP-03:** Ship again with 1 spot — verify `RemainingSpots` decrements again.
- [ ] **TC-FP-04:** Verify a `FilterPaperUsage` record is created for each shipment.
- [ ] **TC-FP-05:** View the specimen on `/Samples/Detail` — verify usage history shows each usage with date, spots used, and international/local flag.

### 15.2 Depletion

- [ ] **TC-FP-06:** Ship enough spots to bring `RemainingSpots` to 0 — verify Status changes to "Depleted".
- [ ] **TC-FP-07:** Attempt to ship a Depleted Filter Paper specimen — verify validation prevents it (spots exceed remaining).

### 15.3 International vs. Local Limits

- [ ] **TC-FP-08:** Ship 2 spots as international — verify `SpotsShippedInternational = 2`.
- [ ] **TC-FP-09:** Attempt to ship a 3rd international spot — verify validation error: international limit exceeded (max 2).
- [ ] **TC-FP-10:** Ship 2 spots as local (domestic) — verify `SpotsReservedLocal = 2`.
- [ ] **TC-FP-11:** Attempt to ship a 3rd local spot — verify validation error: local limit exceeded (max 2).

### 15.4 Validation Pre-Flight

- [ ] **TC-FP-12:** Attempt to ship with `filterPaperSpotsPerSpecimen` greater than `RemainingSpots` — verify validation error.
- [ ] **TC-FP-13:** Verify the Ship form includes an "International" checkbox and a "Filter Paper Spots" input.

---

## 16. Special Sample Rules — Plasma

**Requirements:** REQ-SPL-05, REQ-SPL-06

### 16.1 Aliquot-2 Block

- [ ] **TC-PL-01:** Create a Plasma specimen with AliquotNumber = 2.
- [ ] **TC-PL-02:** Import a shipment request that matches the Aliquot-2 specimen.
- [ ] **TC-PL-03:** Attempt to ship the batch — verify validation error: "Plasma Aliquot-2 cannot be shipped outbound."
- [ ] **TC-PL-04:** Create a Plasma specimen with AliquotNumber = 1 — verify it can be shipped without error.

### 16.2 Single Aliquot Exception (Deferred — See Section 25)

- [ ] **TC-PL-05:** *(Not yet implemented)* When only one Plasma aliquot exists for a participant, shipping should require a SingleAliquotException approval.

---

## 17. Lab Setup — Freezers & Racks

**Requirements:** REQ-INV-01, REQ-INV-02

### 17.1 Freezers

- [ ] **TC-FRZ-01:** Navigate to `/LabSetup/Freezers` — verify all freezers are listed.
- [ ] **TC-FRZ-02:** Add a new freezer with Name, Temperature, and Location — verify success and it appears in the list.
- [ ] **TC-FRZ-03:** Attempt to add a freezer with a name that already exists — verify a uniqueness error.
- [ ] **TC-FRZ-04:** Edit a freezer's name — verify the change is saved.
- [ ] **TC-FRZ-05:** Delete a freezer that has no racks — verify success.
- [ ] **TC-FRZ-06:** Attempt to delete a freezer that has racks — verify an error message (delete guard).
- [ ] **TC-FRZ-07:** Verify audit entries are created for create and rename operations.

### 17.2 Racks

- [ ] **TC-RCK-01:** Navigate to `/LabSetup/Racks` — verify all racks are listed with their freezer assignment.
- [ ] **TC-RCK-02:** Add a new rack with Name and Freezer — verify success.
- [ ] **TC-RCK-03:** Edit a rack's name — verify the change is saved.
- [ ] **TC-RCK-04:** Delete a rack that has no boxes — verify success.
- [ ] **TC-RCK-05:** Attempt to delete a rack that has boxes — verify an error message (delete guard).
- [ ] **TC-RCK-06:** Verify audit entries are created for create and rename operations.

---

## 18. Lab Setup — Studies

- [ ] **TC-STD-01:** Navigate to `/LabSetup/Studies` — verify all studies are listed.
- [ ] **TC-STD-02:** Add a new study with Code, Name, and Principal Investigator — verify success.
- [ ] **TC-STD-03:** Attempt to add a study with a StudyCode that already exists — verify a uniqueness error (DbUpdateException caught).
- [ ] **TC-STD-04:** Edit a study's code or name — verify the change is saved.
- [ ] **TC-STD-05:** Delete a study that has no specimens — verify success.
- [ ] **TC-STD-06:** Attempt to delete a study that has specimens — verify an error message (delete guard).
- [ ] **TC-STD-07:** Verify audit entries are created for create and edit operations.

---

## 19. Lab Setup — Box Types

- [ ] **TC-BXT-01:** Navigate to `/LabSetup/BoxTypes` — verify the page displays the valid box types (81-slot, 100-slot, Filter Paper Binder) and categories (Standard, Temp, Trash, Shipping).
- [ ] **TC-BXT-02:** Verify this is an informational reference page only (no create/edit/delete actions).

---

## 20. Audit Trail

**Requirements:** REQ-RPT-03

### 20.1 Audit Log Page

- [ ] **TC-AUD-01:** Navigate to `/Administration/Audit` — verify the audit log page loads with filter fields: Table Name, User, Date From, Date To.
- [ ] **TC-AUD-02:** Verify audit entries are displayed with: Table, Record ID, Field, Old Value, New Value, User, Timestamp.
- [ ] **TC-AUD-03:** Filter by Table Name — verify only entries for that table are shown.
- [ ] **TC-AUD-04:** Filter by User — verify only entries by that user are shown.
- [ ] **TC-AUD-05:** Filter by Date range — verify only entries within the range are shown (date-from inclusive, date-to inclusive with +1 day offset).
- [ ] **TC-AUD-06:** Combine filters — verify results match all criteria.

### 20.2 Pagination

- [ ] **TC-AUD-07:** Verify pagination at 50 entries per page.
- [ ] **TC-AUD-08:** Navigate between pages — verify correct entries are shown.

### 20.3 Audit Coverage

Verify that audit entries are created for all of the following operations:

- [ ] **TC-AUD-09:** Specimen created (via Add).
- [ ] **TC-AUD-10:** Specimen imported (via CSV Import).
- [ ] **TC-AUD-11:** Specimen moved (via Move).
- [ ] **TC-AUD-12:** Specimen moved to Temp.
- [ ] **TC-AUD-13:** Specimens reboxed.
- [ ] **TC-AUD-14:** Discard request created.
- [ ] **TC-AUD-15:** Discard approval decision.
- [ ] **TC-AUD-16:** Discard executed (specimens marked Discarded).
- [ ] **TC-AUD-17:** Shipment batch imported.
- [ ] **TC-AUD-18:** Shipment approval decision.
- [ ] **TC-AUD-19:** Shipment shipped.
- [ ] **TC-AUD-20:** Box moved to new rack.
- [ ] **TC-AUD-21:** Freezer created / renamed.
- [ ] **TC-AUD-22:** Rack created / renamed.
- [ ] **TC-AUD-23:** Study created / edited.
- [ ] **TC-AUD-24:** User created.
- [ ] **TC-AUD-25:** User edited (role/department change).
- [ ] **TC-AUD-26:** User password reset.

---

## 21. Barcode Scanner & Usability

**Requirements:** NFR-USE-01, NFR-USE-02

### 21.1 Autofocus

- [ ] **TC-USB-01:** Navigate to `/Samples/Add` — verify cursor is in the BarcodeID field.
- [ ] **TC-USB-02:** Navigate to `/Boxes/Move` — verify cursor is in the Barcode field.
- [ ] **TC-USB-03:** Navigate to `/Boxes/Place` — verify cursor is in the Box Label field.
- [ ] **TC-USB-04:** Navigate to `/Boxes/Search` — verify cursor is in the Box Label field.
- [ ] **TC-USB-05:** Navigate to `/Boxes/Rebox` — verify cursor is in the Scan Barcode field.
- [ ] **TC-USB-06:** Navigate to `/Samples/Discard` — verify cursor is in the Barcodes field.

### 21.2 Physical Scanner Testing

- [ ] **TC-USB-07:** **Samples/Add** — Scan a barcode with a USB scanner. Verify it populates the BarcodeID field. Fill remaining fields and press Enter — verify form submits. After redirect, verify cursor returns to BarcodeID.
- [ ] **TC-USB-08:** **Boxes/Move** — Scan a barcode. Verify lookup triggers on Enter. After move and redirect, verify cursor returns to barcode field.
- [ ] **TC-USB-09:** **Boxes/Place** — Scan a box label. Verify lookup triggers on Enter.
- [ ] **TC-USB-10:** **Boxes/Rebox** — Scan a barcode. Verify the specimen is added to the scanned list on Enter (no button click needed). Scan another — verify auto-increment of row/col. Continue scanning until box is full.
- [ ] **TC-USB-11:** **Boxes/Search** — Type or scan a box label. Verify search triggers on Enter.
- [ ] **TC-USB-12:** **General** — Verify no page requires a mouse click to initiate lookup/submit when using a scanner. The scanner sends characters followed by Enter.

### 21.3 Scanner Configuration Notes

- If the scanner sends Tab instead of Enter, configure it to send CR/LF. This is a scanner setting, not an app issue.
- If scan speed drops characters, check the scanner's inter-character delay setting.

---

## 22. Performance

**Requirements:** NFR-PER-01, NFR-PER-02

### 22.1 Bulk Import Performance

**Prerequisite:** Generate a test CSV with 1000+ rows. Ensure referenced studies, sample types, and boxes exist.

```
BarcodeID,LegacyID,StudyCode,SampleType,CollectionDate,BoxLabel,PositionRow,PositionCol
TEST-0001,,STUDY1,Plasma,2025-01-01,BOX-001,1,1
TEST-0002,,STUDY1,Plasma,2025-01-01,BOX-001,1,2
...
```

- [ ] **TC-PRF-01:** Upload the CSV — verify preview completes in < 10 seconds for 1000 rows.
- [ ] **TC-PRF-02:** Click Commit — verify import completes in < 30 seconds for 1000 rows.
- [ ] **TC-PRF-03:** Verify no browser timeout or server error.

**If too slow:** Check if per-row `AnyAsync` position checks are the bottleneck. Consider pre-loading all occupied positions for referenced boxes. Consider wrapping commit in a single transaction with `AddRange`.

### 22.2 Search Performance

**Prerequisite:** Seed the database with 1000+ specimen records.

- [ ] **TC-PRF-04:** Search by barcode substring — verify results in < 2 seconds.
- [ ] **TC-PRF-05:** Filter by study — verify results in < 2 seconds.
- [ ] **TC-PRF-06:** Filter by sample type — verify results in < 2 seconds.
- [ ] **TC-PRF-07:** Combined filters (barcode + study + type) — verify results in < 2 seconds.
- [ ] **TC-PRF-08:** Pagination: click through all pages — verify each page loads in < 2 seconds.

**If too slow:** Run `EXPLAIN ANALYZE` on the PostgreSQL query:
```sql
EXPLAIN ANALYZE SELECT * FROM "tbl_Specimens"
WHERE "BarcodeID" LIKE '%TEST%' AND "StudyID" = 1
ORDER BY "SpecimenID" LIMIT 25 OFFSET 0;
```
Verify indexes are applied via `dotnet ef database update`. Note: `LIKE '%substring%'` cannot use btree indexes, but `LIKE 'prefix%'` will.

### 22.3 Database Indexes

Verify the following indexes exist (migration `AddSearchPerformanceIndexes`):

- [ ] **TC-PRF-09:** Index on `tbl_Specimens.Status`.
- [ ] **TC-PRF-10:** Index on `tbl_Specimens.StudyID`.
- [ ] **TC-PRF-11:** Index on `tbl_Specimens.SampleTypeID`.
- [ ] **TC-PRF-12:** Unique index on `tbl_Specimens.BarcodeID`.
- [ ] **TC-PRF-13:** Unique index on `tbl_Specimens.(BoxID, PositionRow, PositionCol)`.

---

## 23. Backup & Restore

**Requirements:** NFR-SYS-02

### 23.1 Backup Script

```bash
#!/bin/bash
BACKUP_DIR="/var/backups/stasis"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
DB_NAME="stasis"
DB_USER="stasis_app"

mkdir -p "$BACKUP_DIR"

# Full database dump (custom format for selective restore)
pg_dump -U "$DB_USER" -d "$DB_NAME" -Fc -f "$BACKUP_DIR/stasis_$TIMESTAMP.dump"

# Keep only last 30 days of backups
find "$BACKUP_DIR" -name "stasis_*.dump" -mtime +30 -delete

echo "Backup completed: stasis_$TIMESTAMP.dump"
```

### 23.2 Crontab (daily at 2 AM)

```
0 2 * * * /opt/stasis/backup.sh >> /var/log/stasis-backup.log 2>&1
```

### 23.3 Restore

```bash
# List contents of a backup
pg_restore -l /var/backups/stasis/stasis_20260312_020000.dump

# Full restore (WARNING: drops and recreates all objects)
pg_restore -U stasis_app -d stasis --clean --if-exists \
  /var/backups/stasis/stasis_20260312_020000.dump

# Restore specific table only
pg_restore -U stasis_app -d stasis --table=tbl_Specimens \
  /var/backups/stasis/stasis_20260312_020000.dump
```

### 23.4 Test Checklist

- [ ] **TC-BAK-01:** Run the backup script — verify a `.dump` file is created.
- [ ] **TC-BAK-02:** Create a test database, restore from the dump — verify data integrity.
- [ ] **TC-BAK-03:** Verify the application can connect to the restored database and function normally.
- [ ] **TC-BAK-04:** Document the backup location and retention policy in your operations runbook.

---

## 24. Deployment

### 24.1 Pre-Deployment

- [ ] **TC-DEP-01:** PostgreSQL 17 installed and running.
- [ ] **TC-DEP-02:** `stasis_app` database role created with appropriate permissions.
- [ ] **TC-DEP-03:** `stasis` database created, owned by `stasis_app`.
- [ ] **TC-DEP-04:** EF migrations applied: `dotnet ef database update --project STASIS.csproj`.
- [ ] **TC-DEP-05:** Connection string configured via environment variable or user secrets (not in `appsettings.json`).
- [ ] **TC-DEP-06:** `AdminSeedPassword` set via environment variable or config.
- [ ] **TC-DEP-07:** HTTPS certificate configured for production domain.
- [ ] **TC-DEP-08:** (Optional) Email/SMTP settings configured if email notifications are needed.

### 24.2 Application Deployment (Linux/systemd)

- [ ] **TC-DEP-09:** Publish: `dotnet publish STASIS.csproj -c Release -o /opt/stasis/app`.
- [ ] **TC-DEP-10:** .NET 10 runtime installed on server.
- [ ] **TC-DEP-11:** Reverse proxy (Nginx/Apache) configured to forward to Kestrel.

Example Nginx config:
```nginx
server {
    listen 443 ssl;
    server_name stasis.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

- [ ] **TC-DEP-12:** systemd service created for auto-start.

Example service file:
```ini
[Unit]
Description=STASIS Application
After=network.target postgresql.service

[Service]
WorkingDirectory=/opt/stasis/app
ExecStart=/usr/bin/dotnet /opt/stasis/app/STASIS.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=stasis;Username=stasis_app;Password=SECURE_PASSWORD
Environment=AdminSeedPassword=YOUR_SECURE_ADMIN_PASSWORD

[Install]
WantedBy=multi-user.target
```

- [ ] **TC-DEP-13:** Firewall: only ports 443 (HTTPS) and 22 (SSH) exposed. Kestrel port 5000 is internal only.

### 24.3 Application Deployment (Windows/IIS)

- [ ] **TC-DEP-14:** .NET 10 Hosting Bundle installed.
- [ ] **TC-DEP-15:** Application published to IIS site folder.
- [ ] **TC-DEP-16:** Application Pool configured (No Managed Code, per-site).
- [ ] **TC-DEP-17:** Environment variables set in `web.config` `<environmentVariables>` section.
- [ ] **TC-DEP-18:** HTTPS binding configured with SSL certificate.

### 24.4 Post-Deployment Verification

- [ ] **TC-DEP-19:** Navigate to the application URL — verify login page loads.
- [ ] **TC-DEP-20:** Log in as `admin@stasis.com` — verify home page loads.
- [ ] **TC-DEP-21:** Change admin password on first login (forced by `MustChangePassword`).
- [ ] **TC-DEP-22:** Create a test user with Read role — verify they can search but not add.
- [ ] **TC-DEP-23:** Create a test user with Write role — verify they can add specimens.
- [ ] **TC-DEP-24:** Add a specimen — verify it appears in search.
- [ ] **TC-DEP-25:** Run through one complete workflow: Add specimen, Move to another box, Ship via shipment import, verify audit trail records all steps.

---

## 25. Known Limitations & Deferred Items

The following features have groundwork in place but are not yet fully implemented. Tests in this section should be executed after the features are completed.

### 25.1 Email Distribution of Availability Report (Phase 4, item 4.4)

- **Status:** `EmailSender` service exists and is registered. Availability report renders on-screen.
- **Missing:** No code connects `EmailSender` to the shipment workflow. After import, the system should email the report to Regulatory, the Requestor, and PIs.
- [ ] **TC-DEF-01:** *(Future)* After importing a shipment request, verify an email is sent to the configured recipients with the availability report.

### 25.2 Whole-Box Shipping (Phase 4, item 4.8)

- **Status:** `Shipment.IsEntireBox` and `ShippedBoxID` model fields exist. Database columns are ready.
- **Missing:** No UI to select "ship entire box" during the Ship action.
- [ ] **TC-DEF-02:** *(Future)* During ship, select "Ship Entire Box" and choose a box — verify all specimens in that box are marked Shipped and `IsEntireBox = true`.

### 25.3 Staging Status (Phase 4, item 4.6)

- **Status:** Specimens go directly from current status to "Shipped".
- **Missing:** No intermediate "Staged" status between approval and shipping.
- [ ] **TC-DEF-03:** *(Future)* After approval, verify specimens can be set to Status="Staged". After shipping, verify transition from Staged to Shipped.

### 25.4 Single Aliquot Exception (Phase 6, item 6.6)

- **Status:** Approval model supports `ApprovalType = "SingleAliquotException"`. Plasma Aliquot-2 is blocked.
- **Missing:** No validation to detect when a Plasma specimen is the last remaining aliquot.
- [ ] **TC-DEF-04:** *(Future)* When shipping Plasma-1 and no other Plasma aliquot is In-Stock for the same participant, verify a SingleAliquotException approval is required.

### 25.5 Automated Tests (Phase 7, item 7.6)

- **Status:** Test guide and priority scenarios documented. No xUnit project exists.
- [ ] **TC-DEF-05:** *(Future)* Create `STASIS.Tests` project and implement priority test scenarios (see section 26.2).

---

## 26. Requirements Traceability Matrix

### 26.1 Requirement-to-Test Mapping

| Requirement | Description | Test Cases | Status |
|-------------|-------------|------------|--------|
| REQ-ACC-01 | Individual sample registration | TC-ADD-01 to TC-ADD-19 | Implemented |
| REQ-ACC-02 | Bulk import from CSV | TC-IMP-01 to TC-IMP-24 | Implemented |
| REQ-ACC-03 | Minimum data fields per specimen | TC-ADD-01, TC-ADD-15 | Implemented |
| REQ-ACC-04 | Barcode uniqueness validation | TC-ADD-05 to TC-ADD-07, TC-IMP-05 to TC-IMP-07 | Implemented |
| REQ-INV-01 | Location hierarchy (Freezer > Rack > Box > Position) | TC-FRZ-*, TC-RCK-*, TC-BOX-* | Implemented |
| REQ-INV-02 | Box management | TC-PLC-01 to TC-PLC-08 | Implemented |
| REQ-INV-03 | Sample placement (row/col) | TC-ADD-08 to TC-ADD-09 | Implemented |
| REQ-INV-04 | Conflict detection (duplicate position) | TC-ADD-09, TC-IMP-15 to TC-IMP-16, TC-MOV-06 | Implemented |
| REQ-MOV-01 | Individual specimen move | TC-MOV-01 to TC-MOV-07 | Implemented |
| REQ-MOV-02 | Bulk move / re-boxing | TC-RBX-01 to TC-RBX-15 | Implemented |
| REQ-MOV-03 | Box relocation | TC-PLC-01 to TC-PLC-08 | Implemented |
| REQ-MOV-04 | Empty box auto-unassign | TC-MOV-14 to TC-MOV-15 | Implemented |
| REQ-MOV-05 | Box populate (incremental add) | TC-RBX-03 to TC-RBX-06 | Implemented |
| REQ-MOV-06 | Move to Temp | TC-MOV-08 to TC-MOV-13 | Implemented |
| REQ-MOV-07 | Discard workflow (with 3-level approval) | TC-DIS-01 to TC-DIS-22 | Implemented |
| REQ-SHP-01 | Shipment approval workflow | TC-APR-01 to TC-APR-09 | Implemented |
| REQ-SHP-02 | Shipment request import | TC-SHP-01 to TC-SHP-03 | Implemented |
| REQ-SHP-03 | Availability check (auto-matching) | TC-SHP-04 to TC-SHP-08 | Implemented |
| REQ-SHP-04 | Availability report | TC-SHP-09 to TC-SHP-10 | Implemented |
| REQ-SHP-05 | Report distribution (email) | TC-DEF-01 | Deferred |
| REQ-SHP-06 | Status tracking (Pending > Shipped) | TC-SHIP-04 to TC-SHIP-06 | Partial (no Staging) |
| REQ-SHP-07 | Shipping container positions | TC-SHIP-05 | Implemented |
| REQ-SHP-08 | Ship entire box | TC-DEF-02 | Deferred |
| REQ-SPL-01 | Filter paper usage tracking | TC-FP-01 to TC-FP-05 | Implemented |
| REQ-SPL-02 | Filter paper 4-spot tracking | TC-FP-01 to TC-FP-04 | Implemented |
| REQ-SPL-03 | Filter paper depletion | TC-FP-06 to TC-FP-07 | Implemented |
| REQ-SPL-04 | Filter paper international/local limits | TC-FP-08 to TC-FP-13 | Implemented |
| REQ-SPL-05 | Plasma Aliquot-2 shipping block | TC-PL-01 to TC-PL-04 | Implemented |
| REQ-SPL-06 | Single aliquot exception approval | TC-DEF-04 | Deferred |
| REQ-RPT-01 | Search by barcode/study/type | TC-SRC-01 to TC-SRC-07 | Implemented |
| REQ-RPT-02 | Box location lookup | TC-BOX-01 to TC-BOX-16, TC-SRC-14 | Implemented |
| REQ-RPT-03 | Audit trail | TC-AUD-01 to TC-AUD-26 | Implemented |
| REQ-RPT-04 | Shipment manifest (CSV) | TC-SHIP-08 to TC-SHIP-10 | Implemented |
| REQ-SEC-01 | Role-based access control | TC-AUTH-10 to TC-AUTH-27 | Implemented |
| REQ-SEC-02 | User roles (Read/Write/Admin) | TC-AUTH-10 to TC-AUTH-27, TC-USR-01 to TC-USR-14 | Implemented |
| NFR-USE-01 | Barcode scanner usability | TC-USB-01 to TC-USB-12 | Implemented |
| NFR-USE-02 | Color-coded box occupancy grid | TC-BOX-11 to TC-BOX-13 | Implemented |
| NFR-PER-01 | Bulk import < 30 seconds for 1000 rows | TC-PRF-01 to TC-PRF-03 | Needs benchmarking |
| NFR-PER-02 | Search < 2 seconds | TC-PRF-04 to TC-PRF-08 | Needs benchmarking |
| NFR-SYS-02 | Daily backups | TC-BAK-01 to TC-BAK-04 | Documented |

### 26.2 Automated Test Priority (Future)

When the xUnit test project is created, prioritize these service-level tests:

| Priority | Service | Test Scenario | Requirement |
|----------|---------|---------------|-------------|
| 1 | SampleService | `AddSpecimen_WithDuplicateBarcode_ThrowsException` | REQ-ACC-04 |
| 2 | SampleService | `ImportSpecimensFromCsv_ValidFile_ImportsAllRows` | REQ-ACC-02 |
| 3 | SampleService | `ImportSpecimensFromCsv_DuplicateBarcodes_ReportsErrors` | REQ-ACC-04 |
| 4 | SampleService | `RequestDiscardAsync_CreatesApprovalRecord` | REQ-MOV-07 |
| 5 | SampleService | `ExecuteDiscardAsync_SetsStatusToDiscarded` | REQ-MOV-07 |
| 6 | SampleService | `ApproveDiscardAsync_AllThreeApproved_SetsApproved` | REQ-MOV-07 |
| 7 | SampleService | `ApproveDiscardAsync_AnyRejected_SetsRejected` | REQ-MOV-07 |
| 8 | StorageService | `MoveSpecimenAsync_UpdatesPosition` | REQ-MOV-01 |
| 9 | StorageService | `MoveSpecimenAsync_ToOccupiedPosition_ThrowsException` | REQ-INV-04 |
| 10 | StorageService | `MoveToTempAsync_SetsStatusToTemp` | REQ-MOV-06 |
| 11 | StorageService | `MoveSpecimenAsync_FromTemp_ResetsToInStock` | REQ-MOV-06 |
| 12 | StorageService | `CheckAndUnassignEmptyBoxAsync_ClearsRackId` | REQ-MOV-04 |
| 13 | StorageService | `CheckAndUnassignEmptyBoxAsync_TempBox_DoesNotClear` | REQ-MOV-04 |
| 14 | StorageService | `ReboxSpecimensAsync_MovesAll` | REQ-MOV-02 |
| 15 | ShipmentService | `ImportBatchFromCsvAsync_MatchesSpecimens` | REQ-SHP-03 |
| 16 | ShipmentService | `ImportBatchFromCsvAsync_ClassifiesStatuses` | REQ-SHP-04 |
| 17 | ShipmentService | `ApproveBatchAsync_EDAndRegApproved_SetsApproved` | REQ-SHP-01 |
| 18 | ShipmentService | `ApproveBatchAsync_AnyRejected_SetsRejected` | REQ-SHP-01 |
| 19 | ShipmentService | `ValidateShipmentAsync_Plasma2_ReturnsError` | REQ-SPL-05 |
| 20 | ShipmentService | `ValidateShipmentAsync_FilterPaperExceedsRemaining_ReturnsError` | REQ-SPL-01 |
| 21 | ShipmentService | `ValidateShipmentAsync_FilterPaperExceedsIntlLimit_ReturnsError` | REQ-SPL-04 |
| 22 | ShipmentService | `ValidateShipmentAsync_FilterPaperExceedsLocalLimit_ReturnsError` | REQ-SPL-04 |
| 23 | ShipmentService | `ShipBatchAsync_DecrementsFilterPaperSpots` | REQ-SPL-01 |
| 24 | ShipmentService | `ShipBatchAsync_DepletesFilterPaper_SetsDepletedStatus` | REQ-SPL-03 |
| 25 | ShipmentService | `ShipBatchAsync_CreatesFilterPaperUsage` | REQ-SPL-02 |
| 26 | ShipmentService | `ShipBatchAsync_SetsSpecimenStatusShipped` | REQ-SHP-06 |
| 27 | LabSetupService | `DeleteFreezer_WithRacks_ReturnsFalse` | REQ-INV-01 |
| 28 | LabSetupService | `DeleteRack_WithBoxes_ReturnsFalse` | REQ-INV-01 |

---

## Test Execution Summary

| Section | Test Cases | Category |
|---------|-----------|----------|
| 1. Authentication & Authorization | TC-AUTH-01 to TC-AUTH-27 | Security |
| 2. User Administration | TC-USR-01 to TC-USR-14 | Security |
| 3. Sample Add | TC-ADD-01 to TC-ADD-19 | Core Workflow |
| 4. Sample Import | TC-IMP-01 to TC-IMP-24 | Core Workflow |
| 5. Sample Search & Detail | TC-SRC-01 to TC-SRC-22 | Core Workflow |
| 6. Box Search | TC-BOX-01 to TC-BOX-16 | Core Workflow |
| 7. Move Specimen | TC-MOV-01 to TC-MOV-15 | Core Workflow |
| 8. Place Box | TC-PLC-01 to TC-PLC-08 | Core Workflow |
| 9. Rebox | TC-RBX-01 to TC-RBX-15 | Core Workflow |
| 10. Shipment Import | TC-SHP-01 to TC-SHP-14 | Core Workflow |
| 11. Shipment Approval | TC-APR-01 to TC-APR-09 | Governance |
| 12. Ship & Manifest | TC-SHIP-01 to TC-SHIP-12 | Core Workflow |
| 13. Discard | TC-DIS-01 to TC-DIS-22 | Governance |
| 14. Approval Dashboard | TC-DASH-01 to TC-DASH-08 | Governance |
| 15. Filter Paper Rules | TC-FP-01 to TC-FP-13 | Domain Rules |
| 16. Plasma Rules | TC-PL-01 to TC-PL-05 | Domain Rules |
| 17. Freezers & Racks | TC-FRZ-01 to TC-FRZ-07, TC-RCK-01 to TC-RCK-06 | Setup |
| 18. Studies | TC-STD-01 to TC-STD-07 | Setup |
| 19. Box Types | TC-BXT-01 to TC-BXT-02 | Setup |
| 20. Audit Trail | TC-AUD-01 to TC-AUD-26 | Compliance |
| 21. Barcode Scanner | TC-USB-01 to TC-USB-12 | Usability |
| 22. Performance | TC-PRF-01 to TC-PRF-13 | Non-Functional |
| 23. Backup & Restore | TC-BAK-01 to TC-BAK-04 | Operations |
| 24. Deployment | TC-DEP-01 to TC-DEP-25 | Operations |
| 25. Deferred Items | TC-DEF-01 to TC-DEF-05 | Future |
| **Total** | **~260 test cases** | |
