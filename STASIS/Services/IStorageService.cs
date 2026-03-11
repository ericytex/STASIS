using STASIS.Models;

namespace STASIS.Services;

public interface IStorageService
{
    Task<List<Freezer>> GetAllFreezers();
    Task<List<Rack>> GetRacksByFreezer(int freezerId);
    Task<List<Box>> GetBoxesByRack(int rackId);
    Task<Box?> GetBoxWithSpecimens(int boxId);

    // Phase 3: Search and movement
    Task<List<Box>> SearchBoxesAsync(string? label, int? freezerId, int? rackId);
    Task<Box?> GetBoxByLabelAsync(string label);
    Task MoveSpecimenAsync(int specimenId, int newBoxId, int row, int col, string userId);
    Task MoveBoxAsync(int boxId, int newRackId, string userId);
    Task ReboxSpecimensAsync(List<(int SpecimenId, int Row, int Col)> placements, int newBoxId, string userId);
    Task MoveToTempAsync(int specimenId, string userId);
    Task CheckAndUnassignEmptyBoxAsync(int boxId);
}
