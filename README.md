# ERP System (نظام تخطيط موارد المؤسسات)

A comprehensive Enterprise Resource Planning (ERP) system designed to manage business operations including inventory, accounting, sales, and purchasing. The system is built with high performance, scalability, and robust security in mind.

نظام متكامل لتخطيط موارد المؤسسات (ERP) مصمم لإدارة العمليات التجارية المختلفة بما في ذلك المخازن، الحسابات، المبيعات والمشتريات. تم بناء النظام ليكون عالي الأداء وقابل للتوسع وآمن بالكامل.

---

## 🏗️ Project Architecture (بنية المشروع)

The project is split into two main sections:
ينقسم المشروع إلى قسمين رئيسيين:

1. **Backend:** Powered by `.NET Core` using **Clean Architecture** (Domain-Driven Design concepts).
2. **FrontEnd:** Built using **Angular** with a professional and responsive UI (PrimeNG).

---

## 🛠️ Tech Stack (التقنيات المستخدمة)

### Backend (الخلفية)
* **Framework:** .NET Core Web API
* **Architecture:** Clean Architecture (Domain, Application, Infrastructure, Api)
* **ORM:** Entity Framework Core
* **Design Patterns:** CQRS (using MediatR), Repository Pattern, Specification Pattern
* **Database:** Microsoft SQL Server

### Frontend (الواجهة الأمامية)
* **Framework:** Angular (SPA)
* **UI Components:** PrimeNG / PrimeFlex
* **Routing:** Angular Router (Lazy Loading)
* **Styling:** SCSS & Custom Modern Windows-Style Theme

---

## 📦 System Modules (وحدات النظام)

* **Inventory Management (إدارة المخازن):**
  * Product & Unit management.
  * Categories and Stock Group configuration.
  * Automated General Ledger account assignment for inventory items.

* **Accounting & General Ledger (الحسابات العامة):**
  * Journal Entries (قيود اليومية) with automated posting and validation.
  * Currency management with exchange rates.
  * Financial reports (Trial Balance, Account Statements).

* **Purchasing (المشتريات):**
  * Vendor management and purchasing invoices.
  * Automatic payment handling based on payment terms (Cash / Credit).

* **Sales (المبيعات):**
  * Customer management and Sales invoices.

---

## 🚀 Getting Started (بدء التشغيل)

### Prerequisites (المتطلبات الأساسية)
* [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
* [Node.js v18+](https://nodejs.org/)
* [SQL Server](https://www.microsoft.com/sql-server/)

---

### 1. Backend Setup (إعداد الخلفية)

1. Navigate to the backend directory:
   ```bash
   cd Backend
   ```
2. Update the connection string in `ERP.Api/appsettings.json` to match your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=ERP_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
3. Run migrations and update the database:
   ```bash
   dotnet ef database update --project ERP.Infrastructure --startup-project ERP.Api
   ```
4. Start the Web API:
   ```bash
   dotnet run --project ERP.Api
   ```
   * The API should now be running at `https://localhost:7000` or `http://localhost:5000`. You can access the Swagger documentation at `/swagger`.

---

### 2. Frontend Setup (إعداد الواجهة الأمامية)

1. Navigate to the frontend directory:
   ```bash
   cd FrontEnd
   ```
2. Install the required dependencies:
   ```bash
   npm install
   ```
3. Start the Angular development server:
   ```bash
   npm run dev
   # or
   ng serve
   ```
4. Open your browser and navigate to `http://localhost:4200`.

---

## 🤝 Contribution & License (المساهمة والترخيص)

Developed with ❤️ by **[Asem Alyaari](https://github.com/Asem-Alyaari)**.
Feel free to open issues or submit pull requests to improve the system.
