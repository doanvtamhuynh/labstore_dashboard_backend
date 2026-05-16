## Backend foundation + Authentication
- Endpoints:
  - POST /api/auth/login
  - POST /api/auth/refresh-token
  - POST /api/auth/logout
  - POST /api/auth/change-password
  - POST /api/auth/enable-2fa
  - POST /api/auth/verify-2fa
- Main files:
  - Program.cs
  - src/Controllers/AuthController.cs
  - src/Services/AuthService.cs
  - src/Repositories/AdminUserRepository.cs
  - src/Repositories/RefreshTokenRepository.cs
  - src/Repositories/AuditLogRepository.cs
  - src/Models/AdminUser.cs
  - src/Models/RefreshToken.cs
  - src/Models/AuditLog.cs
  - src/Helpers/ApiResponse.cs
  - src/Middlewares/ExceptionHandlingMiddleware.cs
- Notes:
  - .NET 10 Web API scaffolded with Controller -> Service -> Repository pattern.
  - MongoDB config is loaded from .env, with empty keys documented in .env.example.
  - JWT Bearer auth, refresh-token rotation, BCrypt password hashing, TOTP 2FA, role policies, audit log writes are in place.
  - Swagger UI available at /swagger and OpenAPI JSON at /swagger/v1/swagger.json.
  - First login seeds a SuperAdmin from SEED_ADMIN_EMAIL / SEED_ADMIN_PASSWORD when admin_users is empty.
  - CORS allows localhost and 127.0.0.1 frontend dev origins on ports 5173 and 3000.

## Dashboard & Analytics
- Endpoints:
  - GET /api/dashboard/summary
  - GET /api/dashboard/revenue-chart?period=day|week|month|year
  - GET /api/dashboard/top-products?limit=10
  - GET /api/dashboard/geo-orders
  - GET /api/dashboard/kpi
- Main files:
  - src/Controllers/DashboardController.cs
  - src/Services/DashboardService.cs
  - src/Repositories/DashboardRepository.cs
  - src/DTOs/DashboardDtos.cs
- Notes:
  - Dashboard routes require JWT authentication.
  - Reads generic MongoDB documents from orders, products, and customers collections.
  - Revenue chart groups orders by createdAtUtc; low stock count uses products.stock <= 10.
  - KPI returns conversionRate and averageOrderValue.

## Products
- Endpoints:
  - GET /api/products
  - GET /api/products/{id}
  - POST /api/products
  - PUT /api/products/{id}
  - DELETE /api/products/{id}
  - PATCH /api/products/{id}/status
  - POST /api/products/{id}/images
  - DELETE /api/products/{id}/images/{imageId}
  - POST /api/products/import
  - GET /api/products/export
- Main files:
  - src/Controllers/ProductsController.cs
  - src/Services/ProductService.cs
  - src/Repositories/ProductRepository.cs
  - src/Models/Product.cs
  - src/DTOs/ProductDtos.cs
- Notes:
  - Product list supports search, category/status filter, sort, and pagination.
  - Product create/update supports variants, image URL records, inventory, price, and SEO metadata.
  - Import expects CSV columns: name, sku, price, stock, categoryId.
  - Export returns text/csv.
  - Cloudinary upload is not wired yet; image endpoint stores URL metadata for now.

## Categories
- Endpoints:
  - GET /api/categories
  - POST /api/categories
  - PUT /api/categories/{id}
  - DELETE /api/categories/{id}
  - PATCH /api/categories/reorder
- Main files:
  - src/Controllers/CategoriesController.cs
  - src/Services/CategoryService.cs
  - src/Repositories/CategoryRepository.cs
  - src/Models/Category.cs
  - src/DTOs/CategoryDtos.cs
- Notes:
  - GET returns nested parent-child category tree.
  - Delete blocks categories that still have child categories.
  - Reorder updates SortOrder and ParentId in bulk.

## Orders
- Endpoints:
  - GET /api/orders
  - GET /api/orders/{id}
  - PATCH /api/orders/{id}/status
  - GET /api/orders/{id}/history
  - GET /api/orders/{id}/invoice
  - GET /api/orders/export
- Main files:
  - src/Controllers/OrdersController.cs
  - src/Services/OrderService.cs
  - src/Repositories/OrderRepository.cs
  - src/Models/Order.cs
  - src/DTOs/OrderDtos.cs
- Notes:
  - Order list supports status, date range, customer/code search, and pagination.
  - Status updates append an OrderHistoryEntry with the current admin identity.
  - Invoice endpoint returns a simple valid application/pdf payload.
  - Export returns text/csv.

## Customers
- Endpoints:
  - GET /api/customers
  - GET /api/customers/{id}
  - PATCH /api/customers/{id}/status
  - PUT /api/customers/{id}/segment
  - POST /api/customers/{id}/notes
  - PUT /api/customers/{id}/loyalty
- Main files:
  - src/Controllers/CustomersController.cs
  - src/Services/CustomerService.cs
  - src/Repositories/CustomerRepository.cs
  - src/Repositories/CustomerOrderRepository.cs
  - src/Models/Customer.cs
  - src/DTOs/CustomerDtos.cs
- Notes:
  - Customer list supports search, segment/status filter, and pagination.
  - Detail response includes the latest 50 orders for purchase history.
  - CRM notes store content, creator identity, and timestamp.
  - Loyalty endpoint sets the current loyalty point balance.

## Promotions & Marketing
- Endpoints:
  - CRUD /api/promotions/coupons
  - CRUD /api/promotions/flash-sales
  - CRUD /api/promotions/banners
  - CRUD /api/promotions/affiliate
  - POST /api/promotions/email-campaigns
- Main files:
  - src/Controllers/PromotionsController.cs
  - src/Services/PromotionService.cs
  - src/Repositories/MongoCrudRepository.cs
  - src/Models/PromotionModels.cs
  - src/DTOs/PromotionDtos.cs
- Notes:
  - Coupons support percentage/fixed discount, usage limits, status, and schedule fields.
  - Flash sales support category targeting, discount percent, status, and schedule.
  - Banners support image URL, link URL, position, status, and schedule.
  - Affiliate programs support partner name, tracking code, commission percent, and status.
  - Email campaign endpoint stores draft campaign data in MongoDB; email provider send integration is not wired yet.

## Payments
- Endpoints:
  - GET /api/payments
  - GET /api/payments/{id}
  - POST /api/payments/{id}/refund
  - GET /api/payments/reconciliation
- Main files:
  - src/Controllers/PaymentsController.cs
  - src/Services/PaymentService.cs
  - src/Repositories/PaymentRepository.cs
  - src/Models/Payment.cs
  - src/DTOs/PaymentDtos.cs
- Notes:
  - Payment list supports method/status/date filters and pagination.
  - Refund validates remaining refundable amount and marks fully refunded transactions.
  - Reconciliation returns gross revenue, refunded amount, net revenue, and transaction count.

## Shipping
- Endpoints:
  - CRUD /api/shipping/configs
  - GET /api/shipping/providers
  - GET /api/shipping/tracking/{orderId}
  - CRUD /api/shipping/warehouses
  - CRUD /api/shipping/returns
- Main files:
  - src/Controllers/ShippingController.cs
  - src/Services/ShippingService.cs
  - src/Models/ShippingModels.cs
  - src/DTOs/ShippingDtos.cs
- Notes:
  - Shipping configs support region, weight range, fee, and active status.
  - Providers read from shipping_providers and fall back to default GHN/GHTK/Viettel Post entries when empty.
  - Tracking currently reflects order status by orderId.
  - Warehouses and shipping returns use Mongo CRUD repository.

## Reviews & Comments
- Endpoints:
  - GET /api/reviews
  - PATCH /api/reviews/{id}/status
  - POST /api/reviews/{id}/reply
  - GET /api/reviews/flagged
- Main files:
  - src/Controllers/ReviewsController.cs
  - src/Services/ReviewService.cs
  - src/Repositories/ReviewRepository.cs
  - src/Models/Review.cs
  - src/DTOs/ReviewDtos.cs
- Notes:
  - Review list supports rating, product, status filters, and pagination.
  - Status supports Pending, Approved, Hidden, Deleted.
  - Reply stores admin identity and reply timestamp.
  - Flagged endpoint returns all reviews marked IsFlagged.

## Support
- Endpoints:
  - CRUD /api/support/tickets
  - PATCH /api/support/tickets/{id}/assign
  - PATCH /api/support/tickets/{id}/status
  - SignalR Hub /hubs/chat
  - CRUD /api/support/faq
- Main files:
  - src/Controllers/SupportController.cs
  - src/Services/SupportService.cs
  - src/Hubs/ChatHub.cs
  - src/Models/SupportModels.cs
  - src/DTOs/SupportDtos.cs
- Notes:
  - Tickets support status, priority, assignment, and initial customer message.
  - FAQ supports category, publish flag, and sort order.
  - ChatHub supports joining a ticket group and broadcasting ticket messages.

## Notifications
- Endpoints:
  - SignalR Hub /hubs/notifications
  - GET /api/notifications
  - PATCH /api/notifications/{id}/read
  - POST /api/notifications/push
- Main files:
  - src/Controllers/NotificationsController.cs
  - src/Services/NotificationService.cs
  - src/Hubs/NotificationsHub.cs
  - src/Models/Notification.cs
  - src/DTOs/NotificationDtos.cs
- Notes:
  - Notifications support Admin/Customer audience and optional recipient id.
  - Push endpoint stores notification and broadcasts notificationReceived via SignalR.
  - Email trigger automation is not wired yet.

## Reports & Export
- Endpoints:
  - GET /api/reports/revenue
  - GET /api/reports/products
  - GET /api/reports/inventory
  - GET /api/reports/customers
  - GET /api/reports/affiliate
  - GET /api/reports/{reportName}/export
- Main files:
  - src/Controllers/ReportsController.cs
  - src/Services/ReportService.cs
  - src/Repositories/ReportRepository.cs
  - src/DTOs/ReportDtos.cs
- Notes:
  - Revenue supports optional FromDate/ToDate query.
  - Export returns CSV data for revenue, products, inventory, customers, and affiliate reports.

## SEO & Content
- Endpoints:
  - CRUD /api/seo/meta
  - GET /api/seo/sitemap
  - CRUD /api/blog/posts
  - CRUD /api/pages
  - CRUD /api/seo/redirects
- Main files:
  - src/Controllers/ContentController.cs
  - src/Services/ContentService.cs
  - src/Models/ContentModels.cs
  - src/DTOs/ContentDtos.cs
- Notes:
  - Sitemap builds XML from published pages and blog posts.
  - Blog posts and static pages support Draft/Published/Archived status.

## Settings
- Endpoints:
  - GET/PUT /api/settings/store
  - GET/PUT /api/settings/general
  - CRUD /api/settings/admins
  - GET /api/settings/audit-log
  - POST /api/settings/backup
- Main files:
  - src/Controllers/SettingsController.cs
  - src/Services/SettingsService.cs
  - src/Repositories/SettingsRepository.cs
  - src/Models/SettingsModels.cs
  - src/DTOs/SettingsDtos.cs
- Notes:
  - Admin create hashes password with BCrypt.
  - Audit log returns latest 200 entries.
  - Backup endpoint returns queued placeholder response.
