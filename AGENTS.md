# AGENTS.md — Backend (Labstore Dashboard)

## Stack bắt buộc
- **Framework:** .NET 10 (ASP.NET Core Web API)
- **Database:** MongoDB (MongoDB.Driver)
- **Auth:** JWT Bearer + Refresh Token

## Stack tự chọn thêm (dùng nếu phù hợp)
- BCrypt.Net — hash mật khẩu
- FluentValidation — validate request
- AutoMapper — mapping DTO ↔ Model
- Cloudinary SDK — upload ảnh/video
- MailKit hoặc SendGrid — gửi email
- ClosedXML — export Excel
- iTextSharp — export PDF
- SignalR — realtime (notification, live chat)
- Serilog — structured logging
- Swagger / Scalar — tài liệu API tự động
- TOTP (Otp.NET) — xác thực 2 bước (2FA)

## Kết nối MongoDB
```
URI: mongodb+srv://huynhdoan2132_db_admin:<db_password>@labstore-database.8td71os.mongodb.net/?appName=labstore-database
Password: admin123
Database: labstore
```
Lưu vào `.env` — không hardcode trong code.

## Cấu trúc thư mục chuẩn
```
backend/
├── src/
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Models/
│   ├── DTOs/
│   ├── Middlewares/
│   ├── Helpers/
│   └── Config/
├── .env                ← không commit
├── .env.example        ← commit (key rỗng)
├── .gitignore
├── AGENTS.md
├── cache.md
└── Program.cs
```

## Chuẩn response API
Tất cả API phải trả về đúng format sau:
```json
{
  "success": true,
  "data": {},
  "message": "string",
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 100
  }
}
```

## Pattern code
- Controller → Service → Repository (không bỏ qua tầng nào)
- Dùng DTO cho request/response — không expose Model trực tiếp ra ngoài
- Viết Swagger XML comment đầy đủ để frontend đọc được

## Setup & chạy project
```bash
cd backend
cp .env.example .env        # điền giá trị thật vào .env
dotnet restore
dotnet run
# API chạy tại: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

## Danh sách chức năng backend (theo thứ tự ưu tiên)

### 1. Authentication & Phân quyền
- POST /api/auth/login — đăng nhập, trả về access token + refresh token
- POST /api/auth/refresh-token — làm mới access token
- POST /api/auth/logout — hủy refresh token
- POST /api/auth/change-password — đổi mật khẩu
- POST /api/auth/enable-2fa — bật xác thực 2 bước
- POST /api/auth/verify-2fa — xác thực mã TOTP
- Phân quyền role: SuperAdmin, Manager, Staff, Accountant
- Audit Log: ghi lại mọi hành động của admin (ai, làm gì, lúc nào)

### 2. Dashboard & Thống kê
- GET /api/dashboard/summary — doanh thu, đơn hàng, khách hàng mới, tồn kho thấp
- GET /api/dashboard/revenue-chart — dữ liệu biểu đồ doanh thu (filter: day/week/month/year)
- GET /api/dashboard/top-products — top sản phẩm bán chạy
- GET /api/dashboard/geo-orders — phân bố đơn hàng theo tỉnh/thành
- GET /api/dashboard/kpi — CVR (tỷ lệ chuyển đổi), AOV (giá trị đơn TB)

### 3. Sản phẩm
- GET /api/products — danh sách (filter, search, sort, paging)
- GET /api/products/:id — chi tiết
- POST /api/products — tạo mới (bao gồm variants, SEO meta)
- PUT /api/products/:id — cập nhật
- DELETE /api/products/:id — xóa
- PATCH /api/products/:id/status — đổi trạng thái (hiện/ẩn/hết hàng)
- POST /api/products/:id/images — upload ảnh (Cloudinary)
- DELETE /api/products/:id/images/:imgId — xóa ảnh
- POST /api/products/import — import từ Excel/CSV
- GET /api/products/export — export Excel/CSV

### 4. Danh mục
- GET /api/categories — danh sách (bao gồm nested child)
- POST /api/categories — tạo mới
- PUT /api/categories/:id — cập nhật
- DELETE /api/categories/:id — xóa
- PATCH /api/categories/reorder — cập nhật thứ tự sắp xếp

### 5. Đơn hàng
- GET /api/orders — danh sách (filter: status, date range, customer)
- GET /api/orders/:id — chi tiết đơn hàng
- PATCH /api/orders/:id/status — cập nhật trạng thái
- GET /api/orders/:id/history — lịch sử thay đổi trạng thái
- GET /api/orders/:id/invoice — xuất hóa đơn PDF
- GET /api/orders/export — export danh sách CSV

### 6. Khách hàng
- GET /api/customers — danh sách (filter, search, paging)
- GET /api/customers/:id — chi tiết + lịch sử mua hàng
- PATCH /api/customers/:id/status — khóa / mở khóa tài khoản
- PUT /api/customers/:id/segment — gán nhóm (VIP, thường...)
- POST /api/customers/:id/notes — thêm ghi chú CRM
- PUT /api/customers/:id/loyalty — cập nhật điểm thưởng

### 7. Khuyến mãi & Marketing
- CRUD /api/promotions/coupons — mã giảm giá (%, số tiền, giới hạn dùng)
- CRUD /api/promotions/flash-sales — flash sale theo danh mục
- CRUD /api/promotions/banners — banner / popup quảng cáo
- CRUD /api/promotions/affiliate — chương trình affiliate/referral
- POST /api/promotions/email-campaigns — tạo và gửi email campaign

### 8. Thanh toán
- GET /api/payments — danh sách giao dịch (filter: method, status, date)
- GET /api/payments/:id — chi tiết giao dịch
- POST /api/payments/:id/refund — hoàn tiền
- GET /api/payments/reconciliation — báo cáo đối soát doanh thu

### 9. Vận chuyển
- CRUD /api/shipping/configs — cấu hình phí ship theo vùng/trọng lượng
- GET /api/shipping/providers — danh sách đơn vị vận chuyển đã tích hợp
- GET /api/shipping/tracking/:orderId — theo dõi giao hàng
- CRUD /api/shipping/warehouses — quản lý kho nhiều địa điểm
- CRUD /api/shipping/returns — quản lý trả hàng/hoàn hàng

### 10. Đánh giá & Bình luận
- GET /api/reviews — danh sách (filter: rating, product, status)
- PATCH /api/reviews/:id/status — duyệt / ẩn / xóa
- POST /api/reviews/:id/reply — trả lời đánh giá
- GET /api/reviews/flagged — danh sách đánh giá bị báo cáo

### 11. Hỗ trợ Khách hàng
- CRUD /api/support/tickets — hệ thống ticket
- PATCH /api/support/tickets/:id/assign — phân công nhân viên
- PATCH /api/support/tickets/:id/status — cập nhật trạng thái ticket
- SignalR Hub /hubs/chat — live chat realtime
- CRUD /api/support/faq — câu hỏi thường gặp

### 12. Thông báo
- SignalR Hub /hubs/notifications — push realtime
- GET /api/notifications — danh sách thông báo của admin
- PATCH /api/notifications/:id/read — đánh dấu đã đọc
- POST /api/notifications/push — gửi push notification đến khách hàng
- Email trigger tự động: xác nhận đơn, giao hàng, hủy đơn

### 13. Báo cáo & Xuất dữ liệu
- GET /api/reports/revenue — báo cáo doanh thu (filter time range)
- GET /api/reports/products — sản phẩm bán chạy
- GET /api/reports/inventory — tồn kho (sắp hết, đã hết)
- GET /api/reports/customers — hành vi khách hàng
- GET /api/reports/affiliate — báo cáo affiliate/referral
- GET /api/reports/*/export — xuất Excel hoặc PDF

### 14. SEO & Nội dung
- CRUD /api/seo/meta — meta title/description cho product/category/page
- GET /api/seo/sitemap — tạo sitemap XML tự động
- CRUD /api/blog/posts — bài viết blog (hỗ trợ draft/published)
- CRUD /api/pages — trang tĩnh / landing page
- CRUD /api/seo/redirects — quản lý redirect URL

### 15. Cài đặt Hệ thống
- GET/PUT /api/settings/store — thông tin cửa hàng (tên, logo, địa chỉ, múi giờ)
- GET/PUT /api/settings/general — thuế, tiền tệ, ngôn ngữ
- CRUD /api/settings/admins — quản lý tài khoản admin + phân quyền
- GET /api/settings/audit-log — lịch sử hoạt động admin
- POST /api/settings/backup — backup dữ liệu MongoDB