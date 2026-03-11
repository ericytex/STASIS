using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using STASIS.Data;
using STASIS.Models;
using STASIS.Services;

namespace STASIS.Pages.LabSetup
{
    [Authorize(Roles = "Admin")]
    public class StudiesModel : PageModel
    {
        private readonly StasisDbContext _context;
        private readonly IAuditService _auditService;

        public StudiesModel(StasisDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public List<Study> Studies { get; set; } = new();

        [BindProperty]
        public StudyInput Input { get; set; } = new();

        public class StudyInput
        {
            public int? StudyID { get; set; }

            [Required]
            [StringLength(50)]
            [Display(Name = "Study Code")]
            public string StudyCode { get; set; } = string.Empty;

            [StringLength(200)]
            [Display(Name = "Study Name")]
            public string? StudyName { get; set; }

            [StringLength(200)]
            [Display(Name = "Principal Investigator")]
            public string? PrincipalInvestigator { get; set; }
        }

        public async Task OnGetAsync()
        {
            Studies = await _context.Studies.OrderBy(s => s.StudyCode).ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid)
            {
                Studies = await _context.Studies.OrderBy(s => s.StudyCode).ToListAsync();
                return Page();
            }

            var study = new Study
            {
                StudyCode = Input.StudyCode,
                StudyName = Input.StudyName,
                PrincipalInvestigator = Input.PrincipalInvestigator
            };

            _context.Studies.Add(study);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("Input.StudyCode", "This study code is already in use.");
                Studies = await _context.Studies.OrderBy(s => s.StudyCode).ToListAsync();
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            await _auditService.LogChangeAsync("tbl_Studies", study.StudyID.ToString(),
                "Created", null, study.StudyCode, userId);

            TempData["Success"] = $"Study \"{study.StudyCode}\" added.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid || Input.StudyID == null)
            {
                Studies = await _context.Studies.OrderBy(s => s.StudyCode).ToListAsync();
                return Page();
            }

            var existing = await _context.Studies.FindAsync(Input.StudyID.Value);
            if (existing == null) return NotFound();

            var oldCode = existing.StudyCode;
            existing.StudyCode = Input.StudyCode;
            existing.StudyName = Input.StudyName;
            existing.PrincipalInvestigator = Input.PrincipalInvestigator;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("Input.StudyCode", "This study code is already in use.");
                Studies = await _context.Studies.OrderBy(s => s.StudyCode).ToListAsync();
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if (oldCode != existing.StudyCode)
                await _auditService.LogChangeAsync("tbl_Studies", existing.StudyID.ToString(),
                    "StudyCode", oldCode, existing.StudyCode, userId);

            TempData["Success"] = $"Study \"{existing.StudyCode}\" updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var study = await _context.Studies
                .Include(s => s.Specimens)
                .FirstOrDefaultAsync(s => s.StudyID == id);

            if (study == null) return NotFound();

            if (study.Specimens.Count > 0)
            {
                TempData["Error"] = "Cannot delete study — it has specimens associated with it.";
                return RedirectToPage();
            }

            _context.Studies.Remove(study);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Study deleted.";
            return RedirectToPage();
        }
    }
}
