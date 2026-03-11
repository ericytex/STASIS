using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STASIS.Models;
using STASIS.Services;

namespace STASIS.Pages.Boxes
{
    [Authorize(Roles = "Write,Admin")]
    public class ReboxModel : PageModel
    {
        private readonly ISampleService _sampleService;
        private readonly IStorageService _storageService;

        public ReboxModel(ISampleService sampleService, IStorageService storageService)
        {
            _sampleService = sampleService;
            _storageService = storageService;
        }

        [BindProperty]
        public int DestinationBoxId { get; set; }

        [BindProperty]
        public string? ScanBarcode { get; set; }

        [BindProperty]
        public string ScannedListJson { get; set; } = "[]";

        public SelectList BoxOptions { get; set; } = new SelectList(Enumerable.Empty<object>());

        public async Task OnGetAsync()
        {
            await LoadBoxOptionsAsync();
        }

        public async Task<IActionResult> OnGetLookupAsync(string barcode)
        {
            var specimen = await _sampleService.GetSpecimenByBarcode(barcode);
            if (specimen == null)
                return new JsonResult(new { found = false });

            return new JsonResult(new
            {
                found = true,
                specimenId = specimen.SpecimenID,
                barcodeId = specimen.BarcodeID,
                sampleType = specimen.SampleType?.TypeName,
                currentBox = specimen.Box?.BoxLabel,
                currentPos = specimen.PositionRow.HasValue ? $"({specimen.PositionRow},{specimen.PositionCol})" : "-",
                status = specimen.Status
            });
        }

        public async Task<IActionResult> OnPostCommitAsync()
        {
            var items = System.Text.Json.JsonSerializer.Deserialize<List<ReboxItem>>(ScannedListJson);
            if (items == null || items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No specimens scanned.");
                await LoadBoxOptionsAsync();
                return Page();
            }

            var placements = items.Select(i => (i.SpecimenId, i.Row, i.Col)).ToList();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            try
            {
                await _storageService.ReboxSpecimensAsync(placements, DestinationBoxId, userId);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "A position conflict occurred. Check for duplicates.");
                await LoadBoxOptionsAsync();
                return Page();
            }

            TempData["Success"] = $"Successfully reboxed {items.Count} specimen(s).";
            return RedirectToPage("/Boxes/Search");
        }

        private async Task LoadBoxOptionsAsync()
        {
            var freezers = await _storageService.GetAllFreezers();
            var allBoxes = new List<Box>();
            foreach (var f in freezers)
            {
                var racks = await _storageService.GetRacksByFreezer(f.FreezerID);
                foreach (var r in racks)
                {
                    var boxes = await _storageService.GetBoxesByRack(r.RackID);
                    foreach (var b in boxes) { b.Rack = r; r.Freezer = f; }
                    allBoxes.AddRange(boxes);
                }
            }
            BoxOptions = new SelectList(
                allBoxes.OrderBy(b => b.BoxLabel).Select(b => new {
                    b.BoxID,
                    Display = $"{b.BoxLabel} ({b.Rack?.Freezer?.FreezerName} > {b.Rack?.RackName})"
                }), "BoxID", "Display");
        }

        public class ReboxItem
        {
            public int SpecimenId { get; set; }
            public int Row { get; set; }
            public int Col { get; set; }
        }
    }
}
