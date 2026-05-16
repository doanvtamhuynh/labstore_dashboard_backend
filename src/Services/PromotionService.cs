using backend.src.DTOs;
using backend.src.Models;
using backend.src.Repositories;
using backend.src.Config;
using Microsoft.Extensions.Options;

namespace backend.src.Services;

public sealed class PromotionService : IPromotionService
{
    private readonly ICrudRepository<Coupon> _coupons;
    private readonly ICrudRepository<FlashSale> _flashSales;
    private readonly ICrudRepository<PromoBanner> _banners;
    private readonly ICrudRepository<AffiliateProgram> _affiliates;
    private readonly ICrudRepository<EmailCampaign> _emailCampaigns;
    private readonly IEmailService _emailService;
    private readonly SmtpOptions _smtpOptions;

    public PromotionService(
        ICrudRepository<Coupon> coupons,
        ICrudRepository<FlashSale> flashSales,
        ICrudRepository<PromoBanner> banners,
        ICrudRepository<AffiliateProgram> affiliates,
        ICrudRepository<EmailCampaign> emailCampaigns,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions)
    {
        _coupons = coupons;
        _flashSales = flashSales;
        _banners = banners;
        _affiliates = affiliates;
        _emailCampaigns = emailCampaigns;
        _emailService = emailService;
        _smtpOptions = smtpOptions.Value;
    }

    public async Task<IReadOnlyList<CouponResponse>> ListCouponsAsync(CancellationToken cancellationToken)
    {
        return (await _coupons.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    public async Task<CouponResponse> CreateCouponAsync(CouponRequest request, CancellationToken cancellationToken)
    {
        var coupon = new Coupon { Code = request.Code.Trim().ToUpperInvariant(), DiscountType = request.DiscountType, DiscountValue = request.DiscountValue, UsageLimit = request.UsageLimit, Status = request.Status, StartsAtUtc = request.StartsAtUtc, EndsAtUtc = request.EndsAtUtc };
        await _coupons.CreateAsync(coupon, cancellationToken);
        return ToResponse(coupon);
    }

    public async Task<CouponResponse> UpdateCouponAsync(string id, CouponRequest request, CancellationToken cancellationToken)
    {
        var coupon = await RequireAsync(_coupons, id, cancellationToken);
        coupon.Code = request.Code.Trim().ToUpperInvariant();
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.UsageLimit = request.UsageLimit;
        coupon.Status = request.Status;
        coupon.StartsAtUtc = request.StartsAtUtc;
        coupon.EndsAtUtc = request.EndsAtUtc;
        coupon.UpdatedAtUtc = DateTime.UtcNow;
        await _coupons.UpdateAsync(id, coupon, cancellationToken);
        return ToResponse(coupon);
    }

    public Task DeleteCouponAsync(string id, CancellationToken cancellationToken) => _coupons.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<FlashSaleResponse>> ListFlashSalesAsync(CancellationToken cancellationToken)
    {
        return (await _flashSales.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    public async Task<FlashSaleResponse> CreateFlashSaleAsync(FlashSaleRequest request, CancellationToken cancellationToken)
    {
        var item = new FlashSale { Name = request.Name.Trim(), CategoryId = request.CategoryId, DiscountPercent = request.DiscountPercent, Status = request.Status, StartsAtUtc = request.StartsAtUtc, EndsAtUtc = request.EndsAtUtc };
        await _flashSales.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<FlashSaleResponse> UpdateFlashSaleAsync(string id, FlashSaleRequest request, CancellationToken cancellationToken)
    {
        var item = await RequireAsync(_flashSales, id, cancellationToken);
        item.Name = request.Name.Trim();
        item.CategoryId = request.CategoryId;
        item.DiscountPercent = request.DiscountPercent;
        item.Status = request.Status;
        item.StartsAtUtc = request.StartsAtUtc;
        item.EndsAtUtc = request.EndsAtUtc;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _flashSales.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public Task DeleteFlashSaleAsync(string id, CancellationToken cancellationToken) => _flashSales.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<PromoBannerResponse>> ListBannersAsync(CancellationToken cancellationToken)
    {
        return (await _banners.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    public async Task<PromoBannerResponse> CreateBannerAsync(PromoBannerRequest request, CancellationToken cancellationToken)
    {
        var item = new PromoBanner { Title = request.Title.Trim(), ImageUrl = request.ImageUrl.Trim(), LinkUrl = request.LinkUrl, Position = request.Position.Trim(), Status = request.Status, StartsAtUtc = request.StartsAtUtc, EndsAtUtc = request.EndsAtUtc };
        await _banners.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<PromoBannerResponse> UpdateBannerAsync(string id, PromoBannerRequest request, CancellationToken cancellationToken)
    {
        var item = await RequireAsync(_banners, id, cancellationToken);
        item.Title = request.Title.Trim();
        item.ImageUrl = request.ImageUrl.Trim();
        item.LinkUrl = request.LinkUrl;
        item.Position = request.Position.Trim();
        item.Status = request.Status;
        item.StartsAtUtc = request.StartsAtUtc;
        item.EndsAtUtc = request.EndsAtUtc;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _banners.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public Task DeleteBannerAsync(string id, CancellationToken cancellationToken) => _banners.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<AffiliateProgramResponse>> ListAffiliatesAsync(CancellationToken cancellationToken)
    {
        return (await _affiliates.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    public async Task<AffiliateProgramResponse> CreateAffiliateAsync(AffiliateProgramRequest request, CancellationToken cancellationToken)
    {
        var item = new AffiliateProgram { PartnerName = request.PartnerName.Trim(), TrackingCode = request.TrackingCode.Trim(), CommissionPercent = request.CommissionPercent, Status = request.Status };
        await _affiliates.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<AffiliateProgramResponse> UpdateAffiliateAsync(string id, AffiliateProgramRequest request, CancellationToken cancellationToken)
    {
        var item = await RequireAsync(_affiliates, id, cancellationToken);
        item.PartnerName = request.PartnerName.Trim();
        item.TrackingCode = request.TrackingCode.Trim();
        item.CommissionPercent = request.CommissionPercent;
        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _affiliates.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public Task DeleteAffiliateAsync(string id, CancellationToken cancellationToken) => _affiliates.DeleteAsync(id, cancellationToken);

    public async Task<EmailCampaignResponse> CreateEmailCampaignAsync(EmailCampaignRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body) || string.IsNullOrWhiteSpace(request.Segment))
        {
            throw new InvalidOperationException("Email campaign subject, body, and segment are required");
        }

        var item = new EmailCampaign
        {
            Subject = request.Subject.Trim(),
            Body = request.Body,
            Segment = request.Segment.Trim(),
            ScheduledAtUtc = request.ScheduledAtUtc,
            Status = request.ScheduledAtUtc is null || request.ScheduledAtUtc <= DateTime.UtcNow ? PromotionStatus.Active : PromotionStatus.Draft
        };

        var recipients = ResolveRecipients(item.Segment);
        if (item.Status == PromotionStatus.Active)
        {
            var result = await _emailService.SendAsync(item.Subject, item.Body, recipients, cancellationToken);
            item.DeliveryStatus = result.Status;
            item.DeliveryError = result.Error;
            item.Status = result.Sent ? PromotionStatus.Active : PromotionStatus.Paused;
        }
        else
        {
            item.DeliveryStatus = "scheduled";
        }

        await _emailCampaigns.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    private static async Task<T> RequireAsync<T>(ICrudRepository<T> repository, string id, CancellationToken cancellationToken) where T : class
    {
        return await repository.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Promotion item not found");
    }

    private static CouponResponse ToResponse(Coupon item) => new(item.Id!, item.Code, item.DiscountType, item.DiscountValue, item.UsageLimit, item.UsedCount, item.Status, item.StartsAtUtc, item.EndsAtUtc);
    private static FlashSaleResponse ToResponse(FlashSale item) => new(item.Id!, item.Name, item.CategoryId, item.DiscountPercent, item.Status, item.StartsAtUtc, item.EndsAtUtc);
    private static PromoBannerResponse ToResponse(PromoBanner item) => new(item.Id!, item.Title, item.ImageUrl, item.LinkUrl, item.Position, item.Status, item.StartsAtUtc, item.EndsAtUtc);
    private static AffiliateProgramResponse ToResponse(AffiliateProgram item) => new(item.Id!, item.PartnerName, item.TrackingCode, item.CommissionPercent, item.Status);
    private static EmailCampaignResponse ToResponse(EmailCampaign item) => new(item.Id!, item.Subject, item.Segment, item.ScheduledAtUtc, item.Status, item.DeliveryStatus, item.DeliveryError);

    private IReadOnlyList<string> ResolveRecipients(string segment)
    {
        var direct = SmtpEmailService.SplitRecipients(segment);
        if (direct.Any(item => item.Contains('@')))
        {
            return direct;
        }

        return SmtpEmailService.SplitRecipients(_smtpOptions.DefaultRecipients);
    }
}
