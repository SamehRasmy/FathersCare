using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FathersCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TranslateMedicationInstructionsToArabic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE ResidentMedicines SET Instructions = N'كبسولة مساءً. تم إدخالها من روشتة مكتوبة بخط اليد. تهجئة اسم الدواء التجاري تحتاج مراجعة.'
                WHERE Instructions = N'Evening capsule. Imported from handwritten prescription. Brand spelling needs review.';

                UPDATE ResidentMedicines SET Instructions = N'جرعة مساءً قبل العشاء. تم إدخالها من روشتة مكتوبة بخط اليد. اسم الدواء يحتاج مراجعة.'
                WHERE Instructions = N'Evening dose before dinner. Imported from handwritten prescription. Drug name needs review.';

                UPDATE ResidentMedicines SET Instructions = N'جرعة مساءً. تم إدخالها من روشتة مكتوبة بخط اليد. تهجئة اسم الدواء التجاري تحتاج مراجعة.'
                WHERE Instructions = N'Evening dose. Imported from handwritten prescription. Brand spelling needs review.';

                UPDATE ResidentMedicines SET Instructions = N'جرعة صباحية بعد الإفطار. تم إدخالها من روشتة مكتوبة بخط اليد.'
                WHERE Instructions = N'Morning dose after breakfast. Imported from handwritten prescription.';

                UPDATE ResidentMedicines SET Instructions = N'جرعة صباحية بعد الإفطار. تم إدخالها من روشتة مكتوبة بخط اليد. اسم الدواء يحتاج مراجعة.'
                WHERE Instructions = N'Morning dose after breakfast. Imported from handwritten prescription. Drug name needs review.';

                UPDATE ResidentMedicines SET Instructions = N'جرعة صباحية قبل الإفطار. تم إدخالها من روشتة مكتوبة بخط اليد. تهجئة اسم الدواء التجاري تحتاج مراجعة.'
                WHERE Instructions = N'Morning dose before breakfast. Imported from handwritten prescription. Brand spelling needs review.';

                UPDATE ResidentMedicines SET Instructions = N'جرعة صباحية. تم إدخالها من روشتة مكتوبة بخط اليد. التعليمات الدقيقة تحتاج مراجعة.'
                WHERE Instructions = N'Morning dose. Imported from handwritten prescription. Exact directions need review.';

                UPDATE ResidentMedicines SET Instructions = N'جرعة صباحية. تم إدخالها من روشتة مكتوبة بخط اليد. التركيز يحتاج مراجعة.'
                WHERE Instructions = N'Morning dose. Imported from handwritten prescription. Strength needs review.';

                UPDATE ResidentMedicines SET Instructions = N'مرتين يوميًا: صباحًا ومساءً. تم إدخالها من روشتة مكتوبة بخط اليد.'
                WHERE Instructions = N'Twice daily, morning and evening. Imported from handwritten prescription.';

                UPDATE ResidentMedicineBatches SET Notes = N'تم الإدخال من صورة الروشتة الأولى. كبسولة مساءً. تهجئة اسم الدواء التجاري تحتاج مراجعة.'
                WHERE Notes = N'Imported from prescription image 1. Evening capsule. Brand spelling needs review.';

                UPDATE ResidentMedicineBatches SET Notes = N'تم الإدخال من صورة الروشتة الأولى. جرعة مساءً. تهجئة اسم الدواء التجاري تحتاج مراجعة.'
                WHERE Notes = N'Imported from prescription image 1. Evening dose. Brand spelling needs review.';

                UPDATE ResidentMedicineBatches SET Notes = N'تم الإدخال من صورة الروشتة الأولى. جرعات صباحًا ومساءً.'
                WHERE Notes = N'Imported from prescription image 1. Morning and evening doses.';

                UPDATE ResidentMedicineBatches SET Notes = N'تم الإدخال من صورة الروشتة الأولى. جرعة صباحية. التركيز يحتاج مراجعة.'
                WHERE Notes = N'Imported from prescription image 1. Morning dose. Strength needs review.';

                UPDATE ResidentMedicineBatches SET Notes = N'تم الإدخال من صورة الروشتة الأولى. اسم الدواء منقول تقريبيًا ويحتاج مراجعة صيدلية.'
                WHERE Notes = N'Imported from prescription image 1. Name transcribed approximately and needs pharmacy review.';

                UPDATE ResidentMedicineBatches SET Notes = N'تم الإدخال من صورة الروشتة الثانية. جرعة صباحية لدواء مضاد للتجلط.'
                WHERE Notes = N'Imported from prescription image 2. Morning antiplatelet dose.';

                UPDATE ResidentMedicineBatches SET Notes = N'تم الإدخال من صورة الروشتة الثانية. جرعة صباحية. تهجئة اسم الدواء التجاري تحتاج مراجعة.'
                WHERE Notes = N'Imported from prescription image 2. Morning dose. Brand spelling needs review.';

                UPDATE ResidentMedicineBatches SET Notes = N'تم الإدخال من صورة الروشتة الثانية. جرعة صباحية. التعليمات الدقيقة تحتاج مراجعة.'
                WHERE Notes = N'Imported from prescription image 2. Morning dose. Exact directions need review.';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE ResidentMedicines SET Instructions = N'Evening capsule. Imported from handwritten prescription. Brand spelling needs review.'
                WHERE Instructions = N'كبسولة مساءً. تم إدخالها من روشتة مكتوبة بخط اليد. تهجئة اسم الدواء التجاري تحتاج مراجعة.';

                UPDATE ResidentMedicines SET Instructions = N'Evening dose before dinner. Imported from handwritten prescription. Drug name needs review.'
                WHERE Instructions = N'جرعة مساءً قبل العشاء. تم إدخالها من روشتة مكتوبة بخط اليد. اسم الدواء يحتاج مراجعة.';

                UPDATE ResidentMedicines SET Instructions = N'Evening dose. Imported from handwritten prescription. Brand spelling needs review.'
                WHERE Instructions = N'جرعة مساءً. تم إدخالها من روشتة مكتوبة بخط اليد. تهجئة اسم الدواء التجاري تحتاج مراجعة.';

                UPDATE ResidentMedicines SET Instructions = N'Morning dose after breakfast. Imported from handwritten prescription.'
                WHERE Instructions = N'جرعة صباحية بعد الإفطار. تم إدخالها من روشتة مكتوبة بخط اليد.';

                UPDATE ResidentMedicines SET Instructions = N'Morning dose after breakfast. Imported from handwritten prescription. Drug name needs review.'
                WHERE Instructions = N'جرعة صباحية بعد الإفطار. تم إدخالها من روشتة مكتوبة بخط اليد. اسم الدواء يحتاج مراجعة.';

                UPDATE ResidentMedicines SET Instructions = N'Morning dose before breakfast. Imported from handwritten prescription. Brand spelling needs review.'
                WHERE Instructions = N'جرعة صباحية قبل الإفطار. تم إدخالها من روشتة مكتوبة بخط اليد. تهجئة اسم الدواء التجاري تحتاج مراجعة.';

                UPDATE ResidentMedicines SET Instructions = N'Morning dose. Imported from handwritten prescription. Exact directions need review.'
                WHERE Instructions = N'جرعة صباحية. تم إدخالها من روشتة مكتوبة بخط اليد. التعليمات الدقيقة تحتاج مراجعة.';

                UPDATE ResidentMedicines SET Instructions = N'Morning dose. Imported from handwritten prescription. Strength needs review.'
                WHERE Instructions = N'جرعة صباحية. تم إدخالها من روشتة مكتوبة بخط اليد. التركيز يحتاج مراجعة.';

                UPDATE ResidentMedicines SET Instructions = N'Twice daily, morning and evening. Imported from handwritten prescription.'
                WHERE Instructions = N'مرتين يوميًا: صباحًا ومساءً. تم إدخالها من روشتة مكتوبة بخط اليد.';

                UPDATE ResidentMedicineBatches SET Notes = N'Imported from prescription image 1. Evening capsule. Brand spelling needs review.'
                WHERE Notes = N'تم الإدخال من صورة الروشتة الأولى. كبسولة مساءً. تهجئة اسم الدواء التجاري تحتاج مراجعة.';

                UPDATE ResidentMedicineBatches SET Notes = N'Imported from prescription image 1. Evening dose. Brand spelling needs review.'
                WHERE Notes = N'تم الإدخال من صورة الروشتة الأولى. جرعة مساءً. تهجئة اسم الدواء التجاري تحتاج مراجعة.';

                UPDATE ResidentMedicineBatches SET Notes = N'Imported from prescription image 1. Morning and evening doses.'
                WHERE Notes = N'تم الإدخال من صورة الروشتة الأولى. جرعات صباحًا ومساءً.';

                UPDATE ResidentMedicineBatches SET Notes = N'Imported from prescription image 1. Morning dose. Strength needs review.'
                WHERE Notes = N'تم الإدخال من صورة الروشتة الأولى. جرعة صباحية. التركيز يحتاج مراجعة.';

                UPDATE ResidentMedicineBatches SET Notes = N'Imported from prescription image 1. Name transcribed approximately and needs pharmacy review.'
                WHERE Notes = N'تم الإدخال من صورة الروشتة الأولى. اسم الدواء منقول تقريبيًا ويحتاج مراجعة صيدلية.';

                UPDATE ResidentMedicineBatches SET Notes = N'Imported from prescription image 2. Morning antiplatelet dose.'
                WHERE Notes = N'تم الإدخال من صورة الروشتة الثانية. جرعة صباحية لدواء مضاد للتجلط.';

                UPDATE ResidentMedicineBatches SET Notes = N'Imported from prescription image 2. Morning dose. Brand spelling needs review.'
                WHERE Notes = N'تم الإدخال من صورة الروشتة الثانية. جرعة صباحية. تهجئة اسم الدواء التجاري تحتاج مراجعة.';

                UPDATE ResidentMedicineBatches SET Notes = N'Imported from prescription image 2. Morning dose. Exact directions need review.'
                WHERE Notes = N'تم الإدخال من صورة الروشتة الثانية. جرعة صباحية. التعليمات الدقيقة تحتاج مراجعة.';
                """);
        }
    }
}
