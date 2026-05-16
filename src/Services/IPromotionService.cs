using backend.src.DTOs;

namespace backend.src.Services;

public interface IPromotionService
{
    Task<IReadOnlyList<CouponResponse>> ListCouponsAsync(CancellationToken cancellationToken);
    Task<CouponResponse> CreateCouponAsync(CouponRequest request, CancellationToken cancellationToken);
    Task<CouponResponse> UpdateCouponAsync(string id, CouponRequest request, CancellationToken cancellationToken);
    Task DeleteCouponAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<FlashSaleResponse>> ListFlashSalesAsync(CancellationToken cancellationToken);
    Task<FlashSaleResponse> CreateFlashSaleAsync(FlashSaleRequest request, CancellationToken cancellationToken);
    Task<FlashSaleResponse> UpdateFlashSaleAsync(string id, FlashSaleRequest request, CancellationToken cancellationToken);
    Task DeleteFlashSaleAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PromoBannerResponse>> ListBannersAsync(CancellationToken cancellationToken);
    Task<PromoBannerResponse> CreateBannerAsync(PromoBannerRequest request, CancellationToken cancellationToken);
    Task<PromoBannerResponse> UpdateBannerAsync(string id, PromoBannerRequest request, CancellationToken cancellationToken);
    Task DeleteBannerAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AffiliateProgramResponse>> ListAffiliatesAsync(CancellationToken cancellationToken);
    Task<AffiliateProgramResponse> CreateAffiliateAsync(AffiliateProgramRequest request, CancellationToken cancellationToken);
    Task<AffiliateProgramResponse> UpdateAffiliateAsync(string id, AffiliateProgramRequest request, CancellationToken cancellationToken);
    Task DeleteAffiliateAsync(string id, CancellationToken cancellationToken);
    Task<EmailCampaignResponse> CreateEmailCampaignAsync(EmailCampaignRequest request, CancellationToken cancellationToken);
}
