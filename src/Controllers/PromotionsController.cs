using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/promotions")]
public sealed class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet("coupons")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CouponResponse>>>> ListCoupons(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<CouponResponse>>.Ok(await _promotionService.ListCouponsAsync(cancellationToken)));

    [HttpPost("coupons")]
    public async Task<ActionResult<ApiResponse<CouponResponse>>> CreateCoupon(CouponRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<CouponResponse>.Ok(await _promotionService.CreateCouponAsync(request, cancellationToken), "Coupon created"));

    [HttpPut("coupons/{id}")]
    public async Task<ActionResult<ApiResponse<CouponResponse>>> UpdateCoupon(string id, CouponRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<CouponResponse>.Ok(await _promotionService.UpdateCouponAsync(id, request, cancellationToken), "Coupon updated"));

    [HttpDelete("coupons/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCoupon(string id, CancellationToken cancellationToken)
    {
        await _promotionService.DeleteCouponAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Coupon deleted"));
    }

    [HttpGet("flash-sales")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FlashSaleResponse>>>> ListFlashSales(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<FlashSaleResponse>>.Ok(await _promotionService.ListFlashSalesAsync(cancellationToken)));

    [HttpPost("flash-sales")]
    public async Task<ActionResult<ApiResponse<FlashSaleResponse>>> CreateFlashSale(FlashSaleRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<FlashSaleResponse>.Ok(await _promotionService.CreateFlashSaleAsync(request, cancellationToken), "Flash sale created"));

    [HttpPut("flash-sales/{id}")]
    public async Task<ActionResult<ApiResponse<FlashSaleResponse>>> UpdateFlashSale(string id, FlashSaleRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<FlashSaleResponse>.Ok(await _promotionService.UpdateFlashSaleAsync(id, request, cancellationToken), "Flash sale updated"));

    [HttpDelete("flash-sales/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteFlashSale(string id, CancellationToken cancellationToken)
    {
        await _promotionService.DeleteFlashSaleAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Flash sale deleted"));
    }

    [HttpGet("banners")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PromoBannerResponse>>>> ListBanners(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<PromoBannerResponse>>.Ok(await _promotionService.ListBannersAsync(cancellationToken)));

    [HttpPost("banners")]
    public async Task<ActionResult<ApiResponse<PromoBannerResponse>>> CreateBanner(PromoBannerRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<PromoBannerResponse>.Ok(await _promotionService.CreateBannerAsync(request, cancellationToken), "Banner created"));

    [HttpPut("banners/{id}")]
    public async Task<ActionResult<ApiResponse<PromoBannerResponse>>> UpdateBanner(string id, PromoBannerRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<PromoBannerResponse>.Ok(await _promotionService.UpdateBannerAsync(id, request, cancellationToken), "Banner updated"));

    [HttpDelete("banners/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBanner(string id, CancellationToken cancellationToken)
    {
        await _promotionService.DeleteBannerAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Banner deleted"));
    }

    [HttpGet("affiliate")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AffiliateProgramResponse>>>> ListAffiliates(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<AffiliateProgramResponse>>.Ok(await _promotionService.ListAffiliatesAsync(cancellationToken)));

    [HttpPost("affiliate")]
    public async Task<ActionResult<ApiResponse<AffiliateProgramResponse>>> CreateAffiliate(AffiliateProgramRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<AffiliateProgramResponse>.Ok(await _promotionService.CreateAffiliateAsync(request, cancellationToken), "Affiliate program created"));

    [HttpPut("affiliate/{id}")]
    public async Task<ActionResult<ApiResponse<AffiliateProgramResponse>>> UpdateAffiliate(string id, AffiliateProgramRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<AffiliateProgramResponse>.Ok(await _promotionService.UpdateAffiliateAsync(id, request, cancellationToken), "Affiliate program updated"));

    [HttpDelete("affiliate/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAffiliate(string id, CancellationToken cancellationToken)
    {
        await _promotionService.DeleteAffiliateAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Affiliate program deleted"));
    }

    [HttpPost("email-campaigns")]
    public async Task<ActionResult<ApiResponse<EmailCampaignResponse>>> CreateEmailCampaign(EmailCampaignRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<EmailCampaignResponse>.Ok(await _promotionService.CreateEmailCampaignAsync(request, cancellationToken), "Email campaign queued"));
}
