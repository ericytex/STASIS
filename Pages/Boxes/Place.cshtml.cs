using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using STASIS.Models;
using STASIS.Services;

namespace STASIS.Pages.Boxes
{
    [Authorize(Roles = "Write,Admin")]
    public class PlaceModel : PageModel
    {
        private readonly IStorageService _storageService;

        public PlaceModel(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [BindProperty(SupportsGet = true)]
        public string? BoxLabel { get; set; }

        [BindProperty]
        public int BoxId { get; set; }

        [BindProperty]
        public int NewRackId { get; set; }

        public Box? FoundBox { get; set; }
        public SelectList FreezerOptions { get; set; } = new SelectList(Enumerable.Empty<object>());
        public List<Rack> AllRacks { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(BoxLabel))
            {
                FoundBox = await _storageService.GetBoxByLabelAsync(BoxLabel);
            }
            await LoadOptionsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            await _storageService.MoveBoxAsync(BoxId, NewRackId, userId);
            TempData["Success"] = "Box moved successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetRacksAsync(int freezerId)
        {
            var racks = await _storageService.GetRacksByFreezer(freezerId);
            return new JsonResult(racks.Select(r => new { r.RackID, r.RackName }));
        }

        private async Task LoadOptionsAsync()
        {
            var freezers = await _storageService.GetAllFreezers();
            FreezerOptions = new SelectList(freezers, "FreezerID", "FreezerName");
        }
    }
}
