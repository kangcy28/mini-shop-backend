# EcommerceAdmin API

A comprehensive ASP.NET Core Web API for e-commerce administration with JWT authentication, product management, and order processing capabilities.

## 技術堆疊 (Tech Stack)

- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core (Code First)
- **Database**: SQLite (可改用 SQL Server)
- **Authentication**: JWT Bearer Token
- **Documentation**: Swagger/OpenAPI
- **Password Hashing**: BCrypt
- **Architecture**: Layered Architecture (Controllers/Services/Repositories/DTOs/Models)

## 功能模組 (Features)

### 1. 身份驗證系統 (Authentication System)
- 用戶註冊與登入 (User registration & login)
- JWT Token 生成與驗證 (JWT token generation & validation)
- 角色權限控制 (Role-based authorization: Admin/User)

### 2. 商品管理 (Product Management)
- 商品 CRUD 操作 (Product CRUD operations)
- 按分類篩選商品 (Filter products by category)
- 活躍商品查詢 (Active products query)
- 商品庫存管理 (Product inventory management)

### 3. 商品分類管理 (Category Management)
- 分類 CRUD 操作 (Category CRUD operations)
- 分類與商品關聯 (Category-Product relationships)

### 4. 訂單管理 (Order Management) [預備功能]
- 訂單與訂單明細模型已建立 (Order & OrderDetail models implemented)
- Repository 層已完成 (Repository layer completed)

## 專案結構 (Project Structure)

```
EcommerceAdminAPI/
├── Controllers/
│   ├── AuthController.cs          # 身份驗證控制器
│   ├── CategoriesController.cs    # 分類管理控制器
│   └── ProductsController.cs      # 商品管理控制器
├── Models/
│   ├── User.cs                    # 用戶模型
│   ├── Category.cs                # 分類模型
│   ├── Product.cs                 # 商品模型
│   ├── Order.cs                   # 訂單模型
│   └── OrderDetail.cs             # 訂單明細模型
├── DTOs/
│   ├── AuthResponseDto.cs         # 身份驗證回應 DTO
│   ├── LoginDto.cs                # 登入 DTO
│   ├── RegisterDto.cs             # 註冊 DTO
│   ├── CategoryDto.cs             # 分類相關 DTO
│   ├── ProductDto.cs              # 商品相關 DTO
│   └── OrderDto.cs                # 訂單相關 DTO
├── Services/
│   ├── IAuthService.cs & AuthService.cs           # 身份驗證服務
│   ├── ICategoryService.cs & CategoryService.cs   # 分類服務
│   └── IProductService.cs & ProductService.cs     # 商品服務
├── Repositories/
│   ├── IGenericRepository.cs & GenericRepository.cs    # 通用存儲庫
│   ├── IProductRepository.cs & ProductRepository.cs    # 商品存儲庫
│   └── IOrderRepository.cs & OrderRepository.cs        # 訂單存儲庫
├── Data/
│   ├── AppDbContext.cs            # 資料庫上下文
│   └── DataSeeder.cs              # 種子資料
├── Middleware/
│   ├── JwtMiddleware.cs           # JWT 中間件
│   └── AuthorizeAttribute.cs      # 授權屬性
├── Program.cs                     # 應用程式入口點
├── appsettings.json               # 應用程式配置
└── EcommerceAdminAPI.csproj       # 專案檔案
```

## 快速開始 (Getting Started)

### 1. 安裝相依套件
```bash
dotnet restore
```

### 2. 建立並初始化資料庫
```bash
dotnet run
```
應用程式啟動時會自動建立 SQLite 資料庫並植入種子資料。

### 3. 存取 API 文件
應用程式啟動後，在瀏覽器中開啟：
```
https://localhost:7xxx/
```
將會顯示 Swagger UI 介面。

## 預設帳號 (Default Accounts)

### 管理員帳號
- **Email**: admin@example.com
- **Password**: Admin123!
- **Role**: Admin

### 一般用戶帳號
- **Email**: user@example.com
- **Password**: User123!
- **Role**: User

## API 端點 (API Endpoints)

### 身份驗證 (Authentication)
- `POST /api/auth/login` - 用戶登入
- `POST /api/auth/register` - 用戶註冊

### 分類管理 (Categories) [需要身份驗證]
- `GET /api/categories` - 取得所有分類
- `GET /api/categories/{id}` - 取得特定分類
- `POST /api/categories` - 建立分類 [需要 Admin 權限]
- `PUT /api/categories/{id}` - 更新分類 [需要 Admin 權限]
- `DELETE /api/categories/{id}` - 刪除分類 [需要 Admin 權限]

### 商品管理 (Products) [需要身份驗證]
- `GET /api/products` - 取得所有商品
- `GET /api/products/{id}` - 取得特定商品
- `GET /api/products/active` - 取得活躍商品
- `GET /api/products/category/{categoryId}` - 取得特定分類的商品
- `POST /api/products` - 建立商品 [需要 Admin 權限]
- `PUT /api/products/{id}` - 更新商品 [需要 Admin 權限]
- `DELETE /api/products/{id}` - 刪除商品 [需要 Admin 權限]

## 身份驗證流程 (Authentication Flow)

1. 使用 `/api/auth/login` 或 `/api/auth/register` 取得 JWT token
2. 在後續請求的 Header 中加入：
   ```
   Authorization: Bearer {your-jwt-token}
   ```
3. 系統會自動驗證 token 並檢查用戶權限

## 資料庫關聯 (Database Relationships)

- **Category ↔ Product**: 一對多關係
- **Order ↔ OrderDetail**: 一對多關係  
- **Product ↔ OrderDetail**: 多對多關係（透過 OrderDetail）

## 開發注意事項 (Development Notes)

- 所有 API 回應都包含適當的 HTTP 狀態碼
- 錯誤處理機制完整，包含詳細錯誤訊息
- 使用 BCrypt 進行密碼雜湊處理
- JWT token 設定 24 小時過期時間
- 支援 CORS 跨域請求
- 資料驗證使用 Data Annotations

## 安全性特點 (Security Features)

- JWT Bearer Token 身份驗證
- 密碼使用 BCrypt 雜湊加密
- 角色基礎的存取控制
- 輸入資料驗證
- SQL Injection 防護（使用 EF Core）