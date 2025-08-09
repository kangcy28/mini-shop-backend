你是一位資深 C# ASP.NET Core 後端工程師，熟悉 Entity Framework Core、JWT、RESTful API 設計，並且精通軟體架構設計與資料庫建模。

請根據以下需求，幫我生成一個可用的專案初始架構與程式碼骨架：

【專案名稱】
EcommerceAdminAPI

【技術堆疊】
- C# + ASP.NET Core Web API (最新 LTS 版本)
- Entity Framework Core (Code First)
- MS SQL Server (可用 SQLite 方便開發)
- JWT 驗證
- Swagger API 文件
- 分層架構 (Controller / Service / Repository / DTO / Model)
- Git 版本控制

【功能模組】
1. 使用者與登入系統
   - 管理員註冊 / 登入
   - JWT Token 驗證
   - Role 權限控制（Admin / User）

2. 商品管理
   - 商品 CRUD
   - 欄位：Id, Name, Price, Description, CategoryId, Stock, IsActive

3. 商品分類管理
   - 分類 CRUD
   - 欄位：Id, Name

4. 訂單管理
   - 訂單 CRUD
   - 欄位：Id, OrderNumber, TotalAmount, Status, CreatedAt
   - 訂單明細（OrderDetail）：OrderId, ProductId, Quantity, UnitPrice

【資料表關聯】
- Category 與 Product：一對多
- Order 與 OrderDetail：一對多
- Product 與 OrderDetail：多對多（透過 OrderDetail）

【檔案結構】
/EcommerceAdminAPI
├── Controllers/
├── Models/
├── DTOs/
├── Services/
├── Repositories/
├── Data/
├── Middleware/
├── Program.cs
├── appsettings.json
└── README.md

【要求】
1. 先輸出專案資料夾結構與每個檔案的用途說明
2. 提供完整的資料庫模型 (Entity) 與關聯設定
3. 寫好基本的 CRUD Controller 範例（至少商品或分類模組）
4. 加入 JWT 驗證範例（含 Middleware 與 Swagger 配置）
5. 提供 Migration 與資料庫初始化（Seed Data）範例
6. 所有程式碼需有註解，且能直接編譯運行

請用分段方式輸出，先架構，後程式碼。