using System.Xml.Linq;
using backend.src.DTOs;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class ContentService : IContentService
{
    private readonly ICrudRepository<SeoMetaEntry> _meta;
    private readonly ICrudRepository<BlogPost> _posts;
    private readonly ICrudRepository<StaticPage> _pages;
    private readonly ICrudRepository<SeoRedirect> _redirects;

    public ContentService(ICrudRepository<SeoMetaEntry> meta, ICrudRepository<BlogPost> posts, ICrudRepository<StaticPage> pages, ICrudRepository<SeoRedirect> redirects)
    {
        _meta = meta;
        _posts = posts;
        _pages = pages;
        _redirects = redirects;
    }

    public async Task<IReadOnlyList<SeoMetaResponseDto>> ListMetaAsync(CancellationToken cancellationToken) => (await _meta.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    public async Task<SeoMetaResponseDto> CreateMetaAsync(SeoMetaRequestDto request, CancellationToken cancellationToken)
    {
        var item = new SeoMetaEntry { EntityType = request.EntityType, EntityId = request.EntityId, MetaTitle = request.MetaTitle, MetaDescription = request.MetaDescription, Slug = request.Slug };
        await _meta.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }
    public async Task<SeoMetaResponseDto> UpdateMetaAsync(string id, SeoMetaRequestDto request, CancellationToken cancellationToken)
    {
        var item = await RequireAsync(_meta, id, cancellationToken);
        item.EntityType = request.EntityType; item.EntityId = request.EntityId; item.MetaTitle = request.MetaTitle; item.MetaDescription = request.MetaDescription; item.Slug = request.Slug;
        await _meta.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }
    public Task DeleteMetaAsync(string id, CancellationToken cancellationToken) => _meta.DeleteAsync(id, cancellationToken);

    public async Task<string> GetSitemapAsync(CancellationToken cancellationToken)
    {
        var pages = await _pages.ListAsync(cancellationToken);
        var posts = await _posts.ListAsync(cancellationToken);
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = pages.Where(p => p.Status == PublishStatus.Published).Select(p => $"/{p.Slug}")
            .Concat(posts.Where(p => p.Status == PublishStatus.Published).Select(p => $"/blog/{p.Slug}"));
        var doc = new XDocument(new XElement(ns + "urlset", urls.Select(url => new XElement(ns + "url", new XElement(ns + "loc", url)))));
        return doc.ToString();
    }

    public async Task<IReadOnlyList<BlogPostResponse>> ListPostsAsync(CancellationToken cancellationToken) => (await _posts.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    public async Task<BlogPostResponse> CreatePostAsync(BlogPostRequest request, CancellationToken cancellationToken)
    {
        var item = new BlogPost { Title = request.Title, Slug = request.Slug, Content = request.Content, Status = request.Status };
        await _posts.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }
    public async Task<BlogPostResponse> UpdatePostAsync(string id, BlogPostRequest request, CancellationToken cancellationToken)
    {
        var item = await RequireAsync(_posts, id, cancellationToken);
        item.Title = request.Title; item.Slug = request.Slug; item.Content = request.Content; item.Status = request.Status; item.UpdatedAtUtc = DateTime.UtcNow;
        await _posts.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }
    public Task DeletePostAsync(string id, CancellationToken cancellationToken) => _posts.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<StaticPageResponse>> ListPagesAsync(CancellationToken cancellationToken) => (await _pages.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    public async Task<StaticPageResponse> CreatePageAsync(StaticPageRequest request, CancellationToken cancellationToken)
    {
        var item = new StaticPage { Title = request.Title, Slug = request.Slug, Content = request.Content, Status = request.Status };
        await _pages.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }
    public async Task<StaticPageResponse> UpdatePageAsync(string id, StaticPageRequest request, CancellationToken cancellationToken)
    {
        var item = await RequireAsync(_pages, id, cancellationToken);
        item.Title = request.Title; item.Slug = request.Slug; item.Content = request.Content; item.Status = request.Status; item.UpdatedAtUtc = DateTime.UtcNow;
        await _pages.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }
    public Task DeletePageAsync(string id, CancellationToken cancellationToken) => _pages.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<SeoRedirectResponse>> ListRedirectsAsync(CancellationToken cancellationToken) => (await _redirects.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    public async Task<SeoRedirectResponse> CreateRedirectAsync(SeoRedirectRequest request, CancellationToken cancellationToken)
    {
        var item = new SeoRedirect { FromUrl = request.FromUrl, ToUrl = request.ToUrl, StatusCode = request.StatusCode };
        await _redirects.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }
    public async Task<SeoRedirectResponse> UpdateRedirectAsync(string id, SeoRedirectRequest request, CancellationToken cancellationToken)
    {
        var item = await RequireAsync(_redirects, id, cancellationToken);
        item.FromUrl = request.FromUrl; item.ToUrl = request.ToUrl; item.StatusCode = request.StatusCode;
        await _redirects.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }
    public Task DeleteRedirectAsync(string id, CancellationToken cancellationToken) => _redirects.DeleteAsync(id, cancellationToken);

    private static async Task<T> RequireAsync<T>(ICrudRepository<T> repository, string id, CancellationToken cancellationToken) where T : class => await repository.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Content item not found");
    private static SeoMetaResponseDto ToResponse(SeoMetaEntry item) => new(item.Id!, item.EntityType, item.EntityId, item.MetaTitle, item.MetaDescription, item.Slug);
    private static BlogPostResponse ToResponse(BlogPost item) => new(item.Id!, item.Title, item.Slug, item.Content, item.Status, item.CreatedAtUtc);
    private static StaticPageResponse ToResponse(StaticPage item) => new(item.Id!, item.Title, item.Slug, item.Content, item.Status, item.CreatedAtUtc);
    private static SeoRedirectResponse ToResponse(SeoRedirect item) => new(item.Id!, item.FromUrl, item.ToUrl, item.StatusCode);
}
