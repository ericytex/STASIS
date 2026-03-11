using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using STASIS.Models;
using STASIS.Services;

namespace STASIS.Pages.Shipments
{
    [Authorize(Roles = "Write,Admin")]
    public class CreateModel : PageModel
    {
        private readonly IShipmentService _shipmentService;

        public CreateModel(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        [BindProperty]
        public IFormFile? CsvFile { get; set; }

        [BindProperty]
        public string? RequestorName { get; set; }

        [BindProperty]
        public string? RequestorEmail { get; set; }

        public ShipmentBatch? ImportedBatch { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (CsvFile == null || CsvFile.Length == 0)
            {
                ModelState.AddModelError("CsvFile", "Please select a CSV file.");
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            using var stream = CsvFile.OpenReadStream();
            ImportedBatch = await _shipmentService.ImportBatchFromCsvAsync(
                stream, userId, RequestorName, RequestorEmail);

            return Page();
        }
    }
}
