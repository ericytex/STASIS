using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using STASIS.Models;
using STASIS.Services;

namespace STASIS.Pages.Boxes
{
    public class SearchModel : PageModel
    {
        private readonly IStorageService _storageService;

        public SearchModel(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [BindProperty(SupportsGet = true)]
        public string? BoxLabel { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FreezerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? RackId { get; set; }

        public List<Box> Boxes { get; set; } = new();
        public SelectList FreezerOptions { get; set; } = new SelectList(Enumerable.Empty<object>());

        // Selected box for detail view
        [BindProperty(SupportsGet = true)]
        public int? SelectedBoxId { get; set; }
        public Box? SelectedBox { get; set; }

        public async Task OnGetAsync()
        {
            var freezers = await _storageService.GetAllFreezers();
            FreezerOptions = new SelectList(freezers, "FreezerID", "FreezerName");

            bool hasFilter = !string.IsNullOrEmpty(BoxLabel) || FreezerId.HasValue || RackId.HasValue;
            if (hasFilter)
            {
                Boxes = await _storageService.SearchBoxesAsync(BoxLabel, FreezerId, RackId);
            }

            if (SelectedBoxId.HasValue)
            {
                SelectedBox = await _storageService.GetBoxWithSpecimens(SelectedBoxId.Value);
            }
        }

        public int GetGridSize(Box box)
        {
            return box.BoxType switch
            {
                "100-slot" => 10,
                "81-slot" => 9,
                _ => 9
            };
        }
    }
}
