using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
public sealed class ContentController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("api/seo/meta")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SeoMetaResponseDto>>>> ListMeta(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<SeoMetaResponseDto>>.Ok(await _contentService.ListMetaAsync(cancellationToken)));
    [HttpPost("api/seo/meta")]
    public async Task<ActionResult<ApiResponse<SeoMetaResponseDto>>> CreateMeta(SeoMetaRequestDto request, CancellationToken cancellationToken) => Ok(ApiResponse<SeoMetaResponseDto>.Ok(await _contentService.CreateMetaAsync(request, cancellationToken), "SEO meta created"));
    [HttpPut("api/seo/meta/{id}")]
    public async Task<ActionResult<ApiResponse<SeoMetaResponseDto>>> UpdateMeta(string id, SeoMetaRequestDto request, CancellationToken cancellationToken) => Ok(ApiResponse<SeoMetaResponseDto>.Ok(await _contentService.UpdateMetaAsync(id, request, cancellationToken), "SEO meta updated"));
    [HttpDelete("api/seo/meta/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteMeta(string id, CancellationToken cancellationToken) { await _contentService.DeleteMetaAsync(id, cancellationToken); return Ok(ApiResponse<object>.Ok(null, "SEO meta deleted")); }
    [HttpGet("api/seo/sitemap")]
    public async Task<ContentResult> Sitemap(CancellationToken cancellationToken) => Content(await _contentService.GetSitemapAsync(cancellationToken), "application/xml");

    [HttpGet("api/blog/posts")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BlogPostResponse>>>> ListPosts(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<BlogPostResponse>>.Ok(await _contentService.ListPostsAsync(cancellationToken)));
    [HttpPost("api/blog/posts")]
    public async Task<ActionResult<ApiResponse<BlogPostResponse>>> CreatePost(BlogPostRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<BlogPostResponse>.Ok(await _contentService.CreatePostAsync(request, cancellationToken), "Blog post created"));
    [HttpPut("api/blog/posts/{id}")]
    public async Task<ActionResult<ApiResponse<BlogPostResponse>>> UpdatePost(string id, BlogPostRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<BlogPostResponse>.Ok(await _contentService.UpdatePostAsync(id, request, cancellationToken), "Blog post updated"));
    [HttpDelete("api/blog/posts/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePost(string id, CancellationToken cancellationToken) { await _contentService.DeletePostAsync(id, cancellationToken); return Ok(ApiResponse<object>.Ok(null, "Blog post deleted")); }

    [HttpGet("api/pages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StaticPageResponse>>>> ListPages(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<StaticPageResponse>>.Ok(await _contentService.ListPagesAsync(cancellationToken)));
    [HttpPost("api/pages")]
    public async Task<ActionResult<ApiResponse<StaticPageResponse>>> CreatePage(StaticPageRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<StaticPageResponse>.Ok(await _contentService.CreatePageAsync(request, cancellationToken), "Page created"));
    [HttpPut("api/pages/{id}")]
    public async Task<ActionResult<ApiResponse<StaticPageResponse>>> UpdatePage(string id, StaticPageRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<StaticPageResponse>.Ok(await _contentService.UpdatePageAsync(id, request, cancellationToken), "Page updated"));
    [HttpDelete("api/pages/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePage(string id, CancellationToken cancellationToken) { await _contentService.DeletePageAsync(id, cancellationToken); return Ok(ApiResponse<object>.Ok(null, "Page deleted")); }

    [HttpGet("api/seo/redirects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SeoRedirectResponse>>>> ListRedirects(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<SeoRedirectResponse>>.Ok(await _contentService.ListRedirectsAsync(cancellationToken)));
    [HttpPost("api/seo/redirects")]
    public async Task<ActionResult<ApiResponse<SeoRedirectResponse>>> CreateRedirect(SeoRedirectRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<SeoRedirectResponse>.Ok(await _contentService.CreateRedirectAsync(request, cancellationToken), "Redirect created"));
    [HttpPut("api/seo/redirects/{id}")]
    public async Task<ActionResult<ApiResponse<SeoRedirectResponse>>> UpdateRedirect(string id, SeoRedirectRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<SeoRedirectResponse>.Ok(await _contentService.UpdateRedirectAsync(id, request, cancellationToken), "Redirect updated"));
    [HttpDelete("api/seo/redirects/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRedirect(string id, CancellationToken cancellationToken) { await _contentService.DeleteRedirectAsync(id, cancellationToken); return Ok(ApiResponse<object>.Ok(null, "Redirect deleted")); }
}
