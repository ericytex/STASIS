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
    public class MoveModel : PageModel
    {
        private readonly ISampleService _sampleService;
        private readonly IStorageService _storageService;

        public MoveModel(ISampleService sampleService, IStorageService storageService)
        {
            _sampleService = sampleService;
            _storageService = storageService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Barcode { get; set; }

        [BindProperty]
        public int? SpecimenId { get; set; }

        [BindProperty]
        public int NewBoxId { get; set; }

        [BindProperty]
        public int NewRow { get; set; }

        [BindProperty]
        public int NewCol { get; set; }

        [BindProperty]
        public bool MoveToTemp { get; set; }

        public Specimen? FoundSpecimen { get; set; }
        public SelectList BoxOptions { get; set; } = new SelectList(Enumerable.Empty<object>());

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(Barcode))
            {
                FoundSpecimen = await _sampleService.GetSpecimenByBarcode(Barcode);
            }
            await LoadBoxOptionsAsync();
        }

        public async Task<IActionResult> OnPostMoveAsync()
        {
            if (SpecimenId == null)
            {
                ModelState.AddModelError(string.Empty, "No specimen selected.");
                await LoadBoxOptionsAsync();
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            if (MoveToTemp)
            {
                await _storageService.MoveToTempAsync(SpecimenId.Value, userId);
                TempData["Success"] = "Specimen moved to temporary storage.";
                return RedirectToPage();
            }

            try
            {
                await _storageService.MoveSpecimenAsync(SpecimenId.Value, NewBoxId, NewRow, NewCol, userId);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "That position is already occupied.");
                FoundSpecimen = await _sampleService.GetSpecimenByBarcode(Barcode ?? "");
                await LoadBoxOptionsAsync();
                return Page();
            }

            TempData["Success"] = "Specimen moved successfully.";
            return RedirectToPage();
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
    }
}
