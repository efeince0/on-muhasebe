IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [Cariler] (
        [Id] int NOT NULL IDENTITY,
        [CariKodu] nvarchar(20) NOT NULL,
        [Unvan] nvarchar(150) NOT NULL,
        [CariTipi] nvarchar(15) NOT NULL,
        [VergiDairesi] nvarchar(80) NULL,
        [VergiNo] nvarchar(15) NULL,
        [Telefon] nvarchar(20) NULL,
        [Eposta] nvarchar(100) NULL,
        [Adres] nvarchar(250) NULL,
        [AcilisBakiye] decimal(18,2) NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Cariler] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [Roller] (
        [Id] int NOT NULL IDENTITY,
        [RolAdi] nvarchar(50) NOT NULL,
        [Aciklama] nvarchar(200) NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Roller] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [Stoklar] (
        [Id] int NOT NULL IDENTITY,
        [StokKodu] nvarchar(20) NOT NULL,
        [StokAdi] nvarchar(150) NOT NULL,
        [Kategori] nvarchar(50) NULL,
        [Birim] nvarchar(15) NOT NULL,
        [AlisFiyati] decimal(18,2) NOT NULL,
        [SatisFiyati] decimal(18,2) NOT NULL,
        [KdvOrani] decimal(5,2) NOT NULL,
        [KritikStok] decimal(18,3) NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Stoklar] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [Faturalar] (
        [Id] int NOT NULL IDENTITY,
        [FaturaNo] nvarchar(20) NOT NULL,
        [FaturaTipi] nvarchar(10) NOT NULL,
        [CariId] int NOT NULL,
        [Tarih] date NOT NULL,
        [AraToplam] decimal(18,2) NOT NULL,
        [ToplamKdv] decimal(18,2) NOT NULL,
        [GenelToplam] decimal(18,2) NOT NULL,
        [Aciklama] nvarchar(250) NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Faturalar] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Faturalar_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [Kullanicilar] (
        [Id] int NOT NULL IDENTITY,
        [KullaniciAdi] nvarchar(50) NOT NULL,
        [AdSoyad] nvarchar(100) NOT NULL,
        [Eposta] nvarchar(100) NULL,
        [SifreHash] nvarchar(255) NOT NULL,
        [RolId] int NOT NULL,
        [SonGiris] datetime2 NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Kullanicilar] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Kullanicilar_Roller_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roller] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [RolIzinleri] (
        [Id] int NOT NULL IDENTITY,
        [RolId] int NOT NULL,
        [Modul] nvarchar(50) NOT NULL,
        [Islem] nvarchar(20) NOT NULL,
        [IzinVar] bit NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_RolIzinleri] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RolIzinleri_Roller_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roller] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [CariIslemler] (
        [Id] int NOT NULL IDENTITY,
        [IslemNo] nvarchar(20) NOT NULL,
        [CariId] int NOT NULL,
        [IslemTipi] nvarchar(10) NOT NULL,
        [Tarih] date NOT NULL,
        [Tutar] decimal(18,2) NOT NULL,
        [OdemeSekli] nvarchar(20) NOT NULL,
        [Aciklama] nvarchar(250) NULL,
        [FaturaId] int NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_CariIslemler] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CariIslemler_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CariIslemler_Faturalar_FaturaId] FOREIGN KEY ([FaturaId]) REFERENCES [Faturalar] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [FaturaSatirlari] (
        [Id] int NOT NULL IDENTITY,
        [FaturaId] int NOT NULL,
        [StokId] int NOT NULL,
        [Miktar] decimal(18,3) NOT NULL,
        [BirimFiyat] decimal(18,2) NOT NULL,
        [KdvOrani] decimal(5,2) NOT NULL,
        [SatirTutari] decimal(18,2) NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_FaturaSatirlari] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FaturaSatirlari_Faturalar_FaturaId] FOREIGN KEY ([FaturaId]) REFERENCES [Faturalar] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_FaturaSatirlari_Stoklar_StokId] FOREIGN KEY ([StokId]) REFERENCES [Stoklar] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE TABLE [StokHareketleri] (
        [Id] int NOT NULL IDENTITY,
        [HareketNo] nvarchar(20) NOT NULL,
        [StokId] int NOT NULL,
        [HareketTipi] nvarchar(15) NOT NULL,
        [Miktar] decimal(18,3) NOT NULL,
        [Tarih] date NOT NULL,
        [Aciklama] nvarchar(250) NULL,
        [FaturaId] int NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        [OlusturanKullaniciId] int NULL,
        [GuncellemeTarihi] datetime2 NULL,
        [GuncelleyenKullaniciId] int NULL,
        [Aktif] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_StokHareketleri] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StokHareketleri_Faturalar_FaturaId] FOREIGN KEY ([FaturaId]) REFERENCES [Faturalar] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StokHareketleri_Stoklar_StokId] FOREIGN KEY ([StokId]) REFERENCES [Stoklar] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE INDEX [IX_CariIslemler_CariId] ON [CariIslemler] ([CariId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE INDEX [IX_CariIslemler_FaturaId] ON [CariIslemler] ([FaturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CariIslemler_IslemNo] ON [CariIslemler] ([IslemNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cariler_CariKodu] ON [Cariler] ([CariKodu]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE INDEX [IX_Faturalar_CariId] ON [Faturalar] ([CariId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Faturalar_FaturaNo] ON [Faturalar] ([FaturaNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE INDEX [IX_FaturaSatirlari_FaturaId] ON [FaturaSatirlari] ([FaturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE INDEX [IX_FaturaSatirlari_StokId] ON [FaturaSatirlari] ([StokId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Kullanicilar_KullaniciAdi] ON [Kullanicilar] ([KullaniciAdi]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE INDEX [IX_Kullanicilar_RolId] ON [Kullanicilar] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolIzinleri_RolId_Modul_Islem] ON [RolIzinleri] ([RolId], [Modul], [Islem]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roller_RolAdi] ON [Roller] ([RolAdi]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE INDEX [IX_StokHareketleri_FaturaId] ON [StokHareketleri] ([FaturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StokHareketleri_HareketNo] ON [StokHareketleri] ([HareketNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE INDEX [IX_StokHareketleri_StokId] ON [StokHareketleri] ([StokId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Stoklar_StokKodu] ON [Stoklar] ([StokKodu]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826072404_IlkSurum'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260826072404_IlkSurum', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908130146_DokumanKisitlari'
)
BEGIN
    EXEC(N'ALTER TABLE [StokHareketleri] ADD CONSTRAINT [CK_StokHareketleri_Miktar] CHECK ([HareketTipi] = ''Sayim'' OR [Miktar] > 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908130146_DokumanKisitlari'
)
BEGIN
    EXEC(N'ALTER TABLE [FaturaSatirlari] ADD CONSTRAINT [CK_FaturaSatirlari_Miktar] CHECK ([Miktar] > 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908130146_DokumanKisitlari'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Cariler_VergiNo] ON [Cariler] ([VergiNo]) WHERE [VergiNo] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908130146_DokumanKisitlari'
)
BEGIN
    EXEC(N'ALTER TABLE [CariIslemler] ADD CONSTRAINT [CK_CariIslemler_Tutar] CHECK ([Tutar] > 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908130146_DokumanKisitlari'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908130146_DokumanKisitlari', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    ALTER TABLE [CariIslemler] DROP CONSTRAINT [FK_CariIslemler_Faturalar_FaturaId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    ALTER TABLE [StokHareketleri] DROP CONSTRAINT [FK_StokHareketleri_Faturalar_FaturaId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    DROP INDEX [IX_StokHareketleri_FaturaId] ON [StokHareketleri];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    DROP INDEX [IX_StokHareketleri_StokId] ON [StokHareketleri];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    DROP INDEX [IX_CariIslemler_CariId] ON [CariIslemler];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    DROP INDEX [IX_CariIslemler_FaturaId] ON [CariIslemler];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokHareketleri]') AND [c].[name] = N'FaturaId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [StokHareketleri] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [StokHareketleri] DROP COLUMN [FaturaId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CariIslemler]') AND [c].[name] = N'FaturaId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [CariIslemler] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [CariIslemler] DROP COLUMN [FaturaId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    CREATE INDEX [IX_StokHareketleri_StokId_Tarih] ON [StokHareketleri] ([StokId], [Tarih]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    CREATE INDEX [IX_CariIslemler_CariId_Tarih] ON [CariIslemler] ([CariId], [Tarih]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260913180626_FaturaBaglantilariKaldirildi'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260913180626_FaturaBaglantilariKaldirildi', N'10.0.11');
END;

COMMIT;
GO

