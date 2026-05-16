using backend.src.Models;

namespace backend.src.DTOs;

public sealed record SeoMetaRequestDto(string EntityType, string EntityId, string MetaTitle, string MetaDescription, string Slug);
public sealed record SeoMetaResponseDto(string Id, string EntityType, string EntityId, string MetaTitle, string MetaDescription, string Slug);
public sealed record BlogPostRequest(string Title, string Slug, string Content, PublishStatus Status);
public sealed record BlogPostResponse(string Id, string Title, string Slug, string Content, PublishStatus Status, DateTime CreatedAtUtc);
public sealed record StaticPageRequest(string Title, string Slug, string Content, PublishStatus Status);
public sealed record StaticPageResponse(string Id, string Title, string Slug, string Content, PublishStatus Status, DateTime CreatedAtUtc);
public sealed record SeoRedirectRequest(string FromUrl, string ToUrl, int StatusCode);
public sealed record SeoRedirectResponse(string Id, string FromUrl, string ToUrl, int StatusCode);
