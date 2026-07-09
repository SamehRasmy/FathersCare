# FathersCare Audit Report

تاريخ المراجعة: 2026-07-08

## النطاق

تمت مراجعة طبقات `Domain`, `Application`, `Infrastructure`, `Web`، وملفات الاختبارات والتوثيق الحالية. الهدف من هذه المرحلة هو الجرد والتقييم فقط، بدون تنفيذ تغييرات تشغيلية قبل الموافقة.

## ملخص تنفيذي

المشروع لديه أساس جيد: حل .NET 10 يعمل، طبقات Clean Architecture موجودة، الويب لا يستخدم `AppDbContext` مباشرة، وموديولات رئيسية بدأت تتحول إلى واجهات حقيقية. لكن المشروع ليس موحدا Enterprise-grade بعد. أكبر فجوة حالية هي اختلاف نمط التنفيذ بين الموديولات: موديول الأدوية يحتوي على Commands/Queries/Handlers، بينما المقيمين والغرف والتقارير والتغذية تعتمد غالبا على Application service interfaces تنفذها Services داخل Infrastructure.

الحالة الحالية للبناء والاختبارات:

- `dotnet build FathersCare.sln`: ناجح، 0 تحذيرات، 0 أخطاء.
- `dotnet test FathersCare.sln --no-build`: ناجح، 13 اختبارا.
- الاختبارات الحالية محدودة: 11 Unit Tests حقيقية لقواعد Validation/Navigation، و2 اختبارات افتراضية فارغة في Integration/Web.

## جرد الموديولات

| الموديول | الحالة الحالية | النواقص الأساسية | التقييم |
| --- | --- | --- | --- |
| Dashboard | صفحة `Home.razor` تستخدم `ICareOverviewService` وتعرض بيانات مجمعة. | لا يوجد CQRS، لا يوجد Tests، الاستعلامات والتجميع داخل `CareOverviewService`. | جزئي |
| Residents | صفحات قائمة/تفاصيل/إضافة وتعديل موجودة، Validation موجود في `ResidentUpsertBusinessRules`، رفع صورة ومستندات موجود. | `CreateResidentCommand` موجود بدون Handler، بقية عمليات Create/Update/Delete تتم عبر `IResidentManagementService`، لا توجد Command/Query handlers كاملة، لا توجد Integration Tests حقيقية، الدومين ما زال public setters. | Functional لكن غير موحد |
| Medications | أقوى موديول حاليا: Commands/Queries/Handlers للأجزاء الأساسية، Validation receipt/doses، UI عملية، AuditLogs موجودة في عدة عمليات. | ما زال `Medications.razor` يستخدم `IMedicationManagementService` مباشرة لعمليات كبيرة، لا توجد Handler tests، الأخطاء غير موحدة، بعض قواعد العمل داخل service بدلا من Domain. | الأقرب للنمط المطلوب |
| Nutrition/Kitchen | Domain entities موجودة وصفحة `Nutrition.razor` تعرض بيانات من Overview. | Application folders شبه فارغة، لا Commands/Queries/Handlers، لا Validation، لا Tests، لا workflows لإدارة الوجبات والمخزون. | Prototype |
| Rooms/Floors/Maintenance | UI حقيقية للغرف والأدوار والصيانة، Service موجود، Migration موجودة، Soft delete للغرف موجود، AuditLogs في عمليات مهمة. | لا يوجد CQRS، لا Tests، قواعد مثل السعة والحذف داخل Infrastructure service، رسائل Validation إنجليزية مباشرة. | Functional لكن غير موحد |
| Reports/Finance | Finance entities موجودة و`Reports.razor` تعرض ملخصات من `CareOverviewService`/`CompositeReportService`. | لا توجد عمليات مالية كاملة، لا CQRS، لا Validation، لا Tests، لا تقارير قابلة للتصدير أو صلاحيات مالية. | Prototype |
| Staff | Domain entities وملف توثيق موجودان. | لا UI، لا Application layer workflows، لا Validation، لا Tests، لا صلاحيات تشغيلية. | Skeleton |
| Auth/Permissions | ASP.NET Identity scaffold موجود، صفحات account موجودة، `docs/04-permissions.md` يصف صلاحيات. | الصلاحيات غير مطبقة بشكل واضح على UI/Application، لا policies مخصصة، لا enforcement داخل use cases. | غير مكتمل |

## ملاحظات معمارية

- نمط Application غير موحد. يوجد CQRS في `Medications` فقط، بينما `Residents`, `Rooms`, `Dashboard`, `Nutrition`, `Reports` تعتمد على service facades.
- لا يوجد تسجيل DI واضح لكل `ICommandHandler` و`IQueryHandler`، لذلك حتى الـ handlers الموجودة ليست نمطا عاما قابلا للاستخدام في كل الواجهات.
- `Domain` حاليا أقرب إلى anemic model: أغلب الكيانات public setters بدون constructors/factories أو methods مثل `AssignRoom`, `Admit`, `MarkDoseAsGiven`.
- قواعد عمل مهمة موجودة داخل `Infrastructure` services، مثل سعة الغرف، حذف الغرف، استلام الدواء، توليد الجرعات، واستبدال contacts.
- معالجة الأخطاء غير موحدة: استخدام متكرر لـ `InvalidOperationException` و`ValidationException` برسائل إنجليزية مباشرة، وبعض الرسائل عربية داخل الخدمات. هذا يصعب الترجمة والعرض الاحترافي.
- `EnsureTenantAsync` موجود في أكثر من Service/Repository ويقوم بإنشاء Tenant fallback عند عدم وجود Tenant. هذا مفيد للتجربة، لكنه يحتاج قرار أمني واضح قبل الإنتاج.
- لا يوجد استخدام مباشر لـ `AppDbContext` داخل `FathersCare.Web` حسب الجرد الحالي، وهذا ملتزم بقاعدة المشروع.

## ملاحظات UI/UX

- توجد بدايات جيدة لتجربة عربية RTL وخطوط عربية، لكن التوحيد غير مكتمل.
- الترجمة غير موحدة: `Residents` يستخدم resource/localizer أكثر من غيره، بينما `Medications`, `Rooms`, `Nutrition`, `Reports` تحتوي على نصوص UI مباشرة.
- لا توجد مكتبة Design System كافية تحت `Components/Shared` غير `LanguageSwitcher`. أغلب البطاقات والجداول والأزرار مبنية داخل الصفحات نفسها.
- صفحات scaffold مثل `Counter.razor` و`Weather.razor` ما زالت موجودة ويجب حذفها من المنتج النهائي إذا لا يوجد استخدام حقيقي لها.
- حالات Loading/Empty/Error موجودة بشكل متفاوت وليست موحدة على كل الصفحات.
- Responsive وAccessibility يحتاجان مراجعة صفحة بصفحة بعد تثبيت Design System.

## الاختبارات والجودة

- Unit tests الحالية تغطي `ResidentUpsertBusinessRules`, `ReceiveMedicineBusinessRules`, و`MedicationNavigationHelper`.
- لا توجد Unit Tests للـ Command/Query handlers الموجودة في Medications.
- لا توجد Domain entity behavior tests لأن الدومين لا يحتوي سلوك حقيقي كاف.
- `tests/FathersCare.IntegrationTests/UnitTest1.cs` و`tests/FathersCare.WebTests/UnitTest1.cs` اختبارات فارغة افتراضية.
- لا يوجد coverage tooling أو target 70%.
- لا يوجد GitHub Actions workflow واضح لتشغيل build/test/format على Pull Requests.

## مخاطر أمنية وتقنية

- رفع الملفات في `LocalFileStorageService` يتحقق من الامتداد فقط، ولا يفحص حجم الملف أو `contentType` أو signature/magic bytes. الاسم المخزن آمن نسبيا لأنه يولد `Guid`، لكن validation غير كاف.
- الصلاحيات موثقة لكنها غير مفعلة بوضوح داخل Application layer، وهذا خطر مهم قبل الإنتاج.
- لا توجد secrets واضحة في `appsettings.json`; الاتصال الحالي LocalDB trusted connection. يجب إبقاء أي connection حقيقي خارج Git.
- `appsettings.Development.json` يحتوي `DetailedErrors: true` وهذا مناسب للتطوير فقط.
- بعض العمليات تسجل AuditLog، لكن التغطية ليست مضمونة لكل العمليات المهمة في كل الموديولات.
- Soft delete موجود في كيانات كثيرة ويستخدم في بعض العمليات، لكن يحتاج توحيد كقاعدة Domain/Application وليس فقط شرط داخل services.
- لا توجد Pagination عامة للقوائم القابلة للنمو مثل المقيمين، الجرعات، الدفعات، السجلات المالية.
- توجد صور شخصية/تجريبية في جذر المستودع مثل `1000181067.png` و`306280655_1465965933875835_2243183241966178612_n.jpg`. يجب نقلها إلى `docs/assets/` أو حذفها بعد الموافقة.

## خطة التنفيذ المقترحة بعد الموافقة

1. توحيد Backend architecture:
   - إضافة registration/use-case dispatching للـ Commands/Queries.
   - تحويل Residents وRooms كبداية إلى Commands/Queries/Handlers.
   - البدء بـ `CreateResidentCommandHandler` ثم Update/Delete/Upload.

2. تقوية Domain model:
   - نقل invariants الأساسية للكيانات.
   - إضافة factory methods وbehavior methods مع توافق EF Core.
   - توحيد soft delete وaudit operations.

3. توحيد الأخطاء والترجمة:
   - Result pattern أو Domain/Application exceptions مخصصة.
   - Error codes قابلة للترجمة بدل رسائل hardcoded.

4. الاختبارات:
   - حذف `UnitTest1.cs` الافتراضية.
   - إضافة Unit Tests لكل Handler وقواعد Domain.
   - إضافة Integration Tests فعلية لكل موديول مهم.
   - إضافة coverage.

5. Design System وUI:
   - بناء مكونات مشتركة للأزرار، الجداول، البطاقات، badges، modals، validation summaries، loading/empty/error states.
   - نقل النصوص إلى resources.
   - حذف صفحات scaffold.
   - مراجعة RTL/LTR وresponsive وa11y.

6. الأمان والأداء:
   - تطبيق permissions في UI وApplication.
   - تقوية file upload validation.
   - مراجعة الاستعلامات، pagination، indexes.

7. التوثيق وCI:
   - تحديث docs/modules والـ roadmap.
   - إضافة `CONTRIBUTING.md`.
   - إضافة GitHub Actions.
   - تنظيف جذر المستودع.

## قرار مطلوب قبل المرحلة التالية

لم يتم تنفيذ أي تغييرات تشغيلية في هذه المرحلة، فقط إنشاء هذا التقرير. أحتاج موافقتك قبل بدء المرحلة التالية، وأقترح أن تبدأ المرحلة 2 بترتيب Backend architecture لموديول Residents ثم Rooms، لأنهما الأكثر استخداما حاليا وفيهما أكبر أثر مباشر على جودة المنتج.
