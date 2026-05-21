# ERP Project - Developer & AI Agent Guidelines

هذا الملف يحتوي على القواعد والأنماط المعمارية الصارمة لمشروع **ERP**. يجب على أي مطور أو وكيل ذكاء اصطناعي (AI Agent) قراءة هذه القواعد وفهمها قبل البدء في أي مهمة.

---

## 1. المعمارية (Clean Architecture Layers)

يجب الالتزام بتقسيم المسؤوليات كما يلي:
- **ERP.Domain**: يحتوي فقط على الكيانات (Entities)، كائنات القيمة (Value Objects)، واجهات المستودعات (Repository Interfaces)، والمواصفات (Specifications). **يمنع** أن تعتمد هذه الطبقة على أي طبقة أخرى.
- **ERP.Application**: يحتوي على منطق العمل (Use Cases) عبر أنماط **CQRS**. يحتوي على الـ Commands, Queries, DTOs, والـ Validators.
- **ERP.Infrastructure**: يحتوي على التنفيذ الفعلي (Data Access, Identity, Services).
- **ERP.Api**: نقطة الدخول، يجب أن تظل "نحيفة" (Thin)؛ مسؤوليتها فقط استقبال الطلبات وتوجيهها عبر MediatR.

---

## 2. نمط CQRS و MediatR

عند إضافة أي ميزة جديدة:
1. قم بإنشاء مجلد داخل `ERP.Application/Features/[FeatureName]`.
2. قسم العمل إلى **Commands** (للعمليات التي تغير البيانات) و **Queries** (لجلب البيانات).
3. لا تقم بكتابة أي منطق أعمال داخل الـ Controller؛ استخدم `_mediator.Send()` فقط.

---

## 3. التعامل مع البيانات (Repository & Specification)

- **لا تستخدم** `DbContext` مباشرة في طبقة الـ Application.
- استخدم `IUnitOfWork` للوصول إلى المستودعات.
- عند الحاجة لفلترة أو بحث أو تصنيف، قم بإنشاء **Specification** جديد يرث من `BaseSpecification<T>`.
- **مثال**: لجلب المنتجات المفعلة، لا تكتب `Where(x => x.IsActive)`، بل أنشئ `ProductsWithStatusSpecification`.

---

## 4. التحقق والValidation

- استخدم **FluentValidation** لكل طلب (Request).
- يتم التحقق تلقائياً عبر `ValidationBehavior` في الـ MediatR Pipeline.
- لا تضف كود `if (!ModelState.IsValid)` في الـ Controller.

---

## 5. معالجة الأخطاء والتدوين (Logging & Exceptions)

- جميع الأخطاء تتم معالجتها مركزياً عبر `ExceptionMiddleware`.
- استخدم `Log.Information` أو `Log.Error` من مكتبة **Serilog** لتدوين العمليات الهامة.
- لا تستخدم `try-catch` إلا في حالات نادرة جداً وضرورية.

---

## 6. التحويل بين الكائنات (Mapping)

- استخدم مكتبة **Mapster** للتحويل بين الـ Entities والـ DTOs.
- قم بتعريف خرائط التحويل (Mappings) داخل طبقة الـ Application إذا كانت معقدة.

---

## 7. القواعد الذهبية (SOLID & DDD)

1. **S (Single Responsibility)**: كل كلاس يؤدي وظيفة واحدة فقط.
2. **O (Open/Closed)**: الكود قابل للتوسع دون الحاجة لتعديل الكود القديم (استخدم Specifications لهذا).
3. **Rich Domain Model**: يفضل وضع منطق الأعمال البسيط داخل الـ Entity نفسه بدلاً من الـ Service (مثل تغيير الحالة أو التحقق من صحة كيان).
4. **Value Objects**: استخدم كائنات القيمة للعناصر التي لا تملك معرفاً مستقلاً (مثل العنوان أو العملة).

---

**ملاحظة هامة للذكاء الاصطناعي**: قبل البدء، تأكد من مطابقة الكود الجديد مع هذه الأنماط. إذا طلبت منك المهمة كسر أحد هذه القواعد، قم بالتنبيه أولاً.
