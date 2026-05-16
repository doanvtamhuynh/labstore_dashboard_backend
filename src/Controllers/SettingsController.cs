using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("store")]
    public async Task<ActionResult<ApiResponse<StoreSettingsRequest>>> GetStore(CancellationToken cancellationToken) => Ok(ApiResponse<StoreSettingsRequest>.Ok(await _settingsService.GetStoreAsync(cancellationToken)));
    [HttpPut("store")]
    public async Task<ActionResult<ApiResponse<StoreSettingsRequest>>> UpdateStore(StoreSettingsRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<StoreSettingsRequest>.Ok(await _settingsService.UpdateStoreAsync(request, cancellationToken), "Store settings updated"));
    [HttpGet("general")]
    public async Task<ActionResult<ApiResponse<GeneralSettingsRequest>>> GetGeneral(CancellationToken cancellationToken) => Ok(ApiResponse<GeneralSettingsRequest>.Ok(await _settingsService.GetGeneralAsync(cancellationToken)));
    [HttpPut("general")]
    public async Task<ActionResult<ApiResponse<GeneralSettingsRequest>>> UpdateGeneral(GeneralSettingsRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<GeneralSettingsRequest>.Ok(await _settingsService.UpdateGeneralAsync(request, cancellationToken), "General settings updated"));
    [HttpGet("admins")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminUserResponse>>>> ListAdmins(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<AdminUserResponse>>.Ok(await _settingsService.ListAdminsAsync(cancellationToken)));
    [HttpPost("admins")]
    public async Task<ActionResult<ApiResponse<AdminUserResponse>>> CreateAdmin(AdminCreateRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<AdminUserResponse>.Ok(await _settingsService.CreateAdminAsync(request, cancellationToken), "Admin created"));
    [HttpPut("admins/{id}")]
    public async Task<ActionResult<ApiResponse<AdminUserResponse>>> UpdateAdmin(string id, AdminUpdateRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<AdminUserResponse>.Ok(await _settingsService.UpdateAdminAsync(id, request, cancellationToken), "Admin updated"));
    [HttpDelete("admins/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAdmin(string id, CancellationToken cancellationToken) { await _settingsService.DeleteAdminAsync(id, cancellationToken); return Ok(ApiResponse<object>.Ok(null, "Admin deleted")); }
    [HttpGet("audit-log")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AuditLogResponse>>>> AuditLog(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<AuditLogResponse>>.Ok(await _settingsService.ListAuditLogsAsync(cancellationToken)));
    [HttpPost("backup")]
    public async Task<ActionResult<ApiResponse<BackupResponse>>> Backup(CancellationToken cancellationToken) => Ok(ApiResponse<BackupResponse>.Ok(await _settingsService.BackupAsync(cancellationToken), "Backup queued"));
}
