-- إضافة عمود TotalCostOfStock يدوياً لو الـ migration ما اتبقتش متطبقة
-- شغّل السكربت ده على نفس قاعدة البيانات اللي الـ app بيستخدمها

-- 1) إضافة العمود (لو موجود هيطلع خطأ، اعمل الخطوة 2 بس)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = N'ops'
      AND TABLE_NAME = N'BranchProductStocks'
      AND COLUMN_NAME = N'TotalCostOfStock'
)
BEGIN
    ALTER TABLE [ops].[BranchProductStocks]
    ADD [TotalCostOfStock] decimal(18,3) NOT NULL DEFAULT 0;
END
GO

-- 2) تسجيل الـ migration في التاريخ عشان Update-Database متحاولش تضيف العمود تاني
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260223120000_AddTotalCostOfStockToBranchProductStock')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260223120000_AddTotalCostOfStockToBranchProductStock', N'8.0.22');
END
GO
