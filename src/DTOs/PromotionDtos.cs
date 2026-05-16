using backend.src.Models;

namespace backend.src.DTOs;

public sealed record CouponRequest(string Code, CouponDiscountType DiscountType, decimal DiscountValue, int UsageLimit, PromotionStatus Status, DateTime? StartsAtUtc, DateTime? EndsAtUtc);

public sealed record CouponResponse(string Id, string Code, CouponDiscountType DiscountType, decimal DiscountValue, int UsageLimit, int UsedCount, PromotionStatus Status, DateTime? StartsAtUtc, DateTime? EndsAtUtc);

public sealed record FlashSaleRequest(string Name, string? CategoryId, decimal DiscountPercent, PromotionStatus Status, DateTime StartsAtUtc, DateTime EndsAtUtc);

public sealed record FlashSaleResponse(string Id, string Name, string? CategoryId, decimal DiscountPercent, PromotionStatus Status, DateTime StartsAtUtc, DateTime EndsAtUtc);

public sealed record PromoBannerRequest(string Title, string ImageUrl, string? LinkUrl, string Position, PromotionStatus Status, DateTime? StartsAtUtc, DateTime? EndsAtUtc);

public sealed record PromoBannerResponse(string Id, string Title, string ImageUrl, string? LinkUrl, string Position, PromotionStatus Status, DateTime? StartsAtUtc, DateTime? EndsAtUtc);

public sealed record AffiliateProgramRequest(string PartnerName, string TrackingCode, decimal CommissionPercent, PromotionStatus Status);

public sealed record AffiliateProgramResponse(string Id, string PartnerName, string TrackingCode, decimal CommissionPercent, PromotionStatus Status);

public sealed record EmailCampaignRequest(string Subject, string Body, string Segment, DateTime? ScheduledAtUtc);

public sealed record EmailCampaignResponse(string Id, string Subject, string Segment, DateTime? ScheduledAtUtc, PromotionStatus Status);
