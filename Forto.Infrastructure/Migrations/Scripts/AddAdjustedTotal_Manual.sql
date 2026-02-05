-- أضف عمود AdjustedTotal لجدول billing.Invoices لو مش موجود
-- شغّل السكربت على نفس الداتابيز اللي الـ API بيستخدمه (في SSMS أو أي عميل SQL)

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('billing.Invoices') AND name = 'AdjustedTotal'
)
BEGIN
    ALTER TABLE billing.Invoices
    ADD AdjustedTotal decimal(18,2) NULL;
END
GO

-- لو شغّلت السكربت يدوي، سجّل الـ migration عشان update-database ما يعيدش التشغيل
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260203000000_AddInvoiceAdjustedTotal')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260203000000_AddInvoiceAdjustedTotal', N'8.0.22');
END
GO
