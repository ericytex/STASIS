using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using STASIS.Services;

namespace STASIS.Pages.Samples
{
    [Authorize(Roles = "Write,Admin")]
    public class ImportModel : PageModel
    {
        private readonly ISampleService _sampleService;
        private readonly IAuditService _auditService;

        public ImportModel(ISampleService sampleService, IAuditService auditService)
        {
            _sampleService = sampleService;
            _auditService = auditService;
        }

        [BindProperty]
        public IFormFile? CsvFile { get; set; }

        public ImportResult? PreviewResult { get; set; }

        [TempData]
        public int ImportedCount { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostPreviewAsync()
        {
            if (CsvFile == null || CsvFile.Length == 0)
            {
                ModelState.AddModelError("CsvFile", "Please select a CSV file.");
                return Page();
            }

            using var stream = CsvFile.OpenReadStream();
            PreviewResult = await _sampleService.ImportSpecimensFromCsv(stream);

            // Store the valid rows in TempData as a serialized CSV so they can be committed
            if (PreviewResult.SuccessRows.Count > 0)
            {
                // Re-read the file and store it for the commit step
                CsvFile = CsvFile; // keep the form visible
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCommitAsync()
        {
            if (CsvFile == null || CsvFile.Length == 0)
            {
                ModelState.AddModelError("CsvFile", "Please re-upload the CSV file to commit.");
                return Page();
            }

            using var stream = CsvFile.OpenReadStream();
            var result = await _sampleService.ImportSpecimensFromCsv(stream);

            if (result.SuccessRows.Count == 0)
            {
                PreviewResult = result;
                ModelState.AddModelError(string.Empty, "No valid rows to import.");
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            int committed = 0;

            foreach (var row in result.SuccessRows)
            {
                if (row.Specimen != null)
                {
                    await _sampleService.AddSpecimen(row.Specimen);
                    await _auditService.LogChangeAsync("tbl_Specimens",
                        row.Specimen.SpecimenID.ToString(),
                        "Imported", null, row.Specimen.BarcodeID, userId);
                    committed++;
                }
            }

            TempData["Success"] = $"Successfully imported {committed} specimen(s). {result.ErrorRows.Count} row(s) skipped.";
            return RedirectToPage("/Samples");
        }
    }
}
