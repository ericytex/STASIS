using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using STASIS.Models;
using STASIS.Services;

namespace STASIS.Pages.Shipments
{
    public class HistoryModel : PageModel
    {
        private readonly IShipmentService _shipmentService;

        public HistoryModel(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        public List<ShipmentBatch> Batches { get; set; } = new();

        // Detail view
        [BindProperty(SupportsGet = true)]
        public int? BatchId { get; set; }
        public ShipmentBatch? SelectedBatch { get; set; }

        // Approval form
        [BindProperty]
        public string? ApprovalLevel { get; set; }
        [BindProperty]
        public string? ApprovalStatus { get; set; }
        [BindProperty]
        public string? ApprovalComments { get; set; }

        // Ship form
        [BindProperty]
        public string? Courier { get; set; }
        [BindProperty]
        public string? TrackingNumber { get; set; }
        [BindProperty]
        public string? Destination { get; set; }

        public async Task OnGetAsync()
        {
            Batches = await _shipmentService.GetAllBatchesAsync();

            if (BatchId.HasValue)
                SelectedBatch = await _shipmentService.GetBatchByIdAsync(BatchId.Value);
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            if (BatchId == null || string.IsNullOrEmpty(ApprovalLevel) || string.IsNullOrEmpty(ApprovalStatus))
                return RedirectToPage();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            await _shipmentService.ApproveBatchAsync(BatchId.Value, userId,
                ApprovalLevel, ApprovalStatus, ApprovalComments);

            TempData["Success"] = $"{ApprovalLevel} approval recorded as {ApprovalStatus}.";
            return RedirectToPage(new { BatchId });
        }

        public async Task<IActionResult> OnPostShipAsync()
        {
            if (BatchId == null) return RedirectToPage();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            await _shipmentService.ShipBatchAsync(BatchId.Value,
                Courier ?? "", TrackingNumber, Destination, userId);

            TempData["Success"] = "Shipment recorded. Specimens marked as Shipped.";
            return RedirectToPage(new { BatchId });
        }

        public async Task<IActionResult> OnGetManifestAsync(int shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null) return NotFound();

            var sb = new StringBuilder();
            sb.AppendLine("Barcode,SampleType,Condition,BoxPosition,SpotsUsed");
            foreach (var sc in shipment.ShipmentContents)
            {
                sb.AppendLine($"\"{sc.Specimen?.BarcodeID}\",\"{sc.Specimen?.SampleType?.TypeName}\",\"{sc.ConditionAtShipment}\",\"{sc.ShippingBoxPosition}\",{sc.SpotsUsed}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"manifest-shipment-{shipmentId}.csv");
        }
    }
}
