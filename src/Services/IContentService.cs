using backend.src.DTOs;

namespace backend.src.Services;

public interface IContentService
{
    Task<IReadOnlyList<SeoMetaResponseDto>> ListMetaAsync(CancellationToken cancellationToken);
    Task<SeoMetaResponseDto> CreateMetaAsync(SeoMetaRequestDto request, CancellationToken cancellationToken);
    Task<SeoMetaResponseDto> UpdateMetaAsync(string id, SeoMetaRequestDto request, CancellationToken cancellationToken);
    Task DeleteMetaAsync(string id, CancellationToken cancellationToken);
    Task<string> GetSitemapAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BlogPostResponse>> ListPostsAsync(CancellationToken cancellationToken);
    Task<BlogPostResponse> CreatePostAsync(BlogPostRequest request, CancellationToken cancellationToken);
    Task<BlogPostResponse> UpdatePostAsync(string id, BlogPostRequest request, CancellationToken cancellationToken);
    Task DeletePostAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<StaticPageResponse>> ListPagesAsync(CancellationToken cancellationToken);
    Task<StaticPageResponse> CreatePageAsync(StaticPageRequest request, CancellationToken cancellationToken);
    Task<StaticPageResponse> UpdatePageAsync(string id, StaticPageRequest request, CancellationToken cancellationToken);
    Task DeletePageAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<SeoRedirectResponse>> ListRedirectsAsync(CancellationToken cancellationToken);
    Task<SeoRedirectResponse> CreateRedirectAsync(SeoRedirectRequest request, CancellationToken cancellationToken);
    Task<SeoRedirectResponse> UpdateRedirectAsync(string id, SeoRedirectRequest request, CancellationToken cancellationToken);
    Task DeleteRedirectAsync(string id, CancellationToken cancellationToken);
}
