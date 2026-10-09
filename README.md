# Sistem Retribusi Pasar

Prototype aplikasi **ASP.NET Core MVC (.NET 8)** berdasarkan mockup Buku Kas Bendahara Penerimaan / Retribusi Pelayanan Pasar.

## Stack

- ASP.NET Core MVC / Razor Views
- C#
- CSS + vanilla JavaScript ringan
- Repository abstraction
- Storage sementara: **InMemory**
- Target database: **SQL Server**
- Cookie Authentication + role claim
- `PasswordHasher<T>` untuk password demo
- Audit trail dasar

## Login demo

- Username: `admin.retribusi`
- Password: `demo123`

User lain:
- `operator.panorama`
- Password: `demo123`

> Ganti seluruh credential demo sebelum dipakai di environment nyata.

## Modul

1. Dashboard
2. Penerimaan
3. Setoran
4. Penerimaan stok STRD/Karcis
5. Distribusi media ke pasar
6. Tracking nomor seri
7. Tunggakan
8. Master Pasar
9. Master Jenis Retribusi
10. Master Tarif & histori tarif
11. Master Kios/Los/Unit
12. Master Petugas
13. Rekap & Laporan
14. User & Role
15. Audit Log
16. Closing Periode

## Business rule yang sudah diterapkan

- Nomor seri menggunakan **range 6 digit**.
- Range stok tidak boleh overlap pada media yang sama.
- Distribusi hanya boleh memakai range yang ada di stok.
- Range distribusi tidak boleh overlap dengan distribusi aktif lain.
- Penerimaan dengan media wajib memakai serial yang sudah didistribusikan ke **pasar yang sama**.
- Serial tidak boleh digunakan dua kali pada penerimaan aktif.
- Nominal seharusnya dihitung server dari master tarif.
- Transaksi pada periode CLOSED ditolak.
- Penerimaan tidak di-hard-delete; Delete pada UI melakukan **VOID**.
- Satu receipt hanya boleh masuk ke satu setoran.
- Setoran menghitung nilai dari receipt yang dipilih.
- Tunggakan dihitung dari bulan aktif unit yang belum memiliki alokasi pembayaran.
- CRUD mencatat AuditLog.

## Menjalankan project

Requirement:

- Visual Studio 2022 terbaru atau VS Code
- .NET 8 SDK

Via terminal:

```bash
cd RetribusiPasar.Web
dotnet restore
dotnet run
```

Atau buka `RetribusiPasar.sln` dari Visual Studio lalu Run.

> Data in-memory akan kembali ke seed setiap aplikasi restart.

## Struktur

```text
Controllers/
Models/
  Entities/
  ViewModels/
Infrastructure/
  InMemoryDataStore.cs
  Repositories/
Services/
Views/
wwwroot/
Database/
  01_schema.sql
```

### Kenapa Repository abstraction?

Controller tidak tahu apakah data berasal dari `List<T>`, SQL Server, atau sumber lain.

Saat ini:

```text
Controller
   ↓
IRepository<T>
   ↓
InMemoryRepository<T>
```

Target:

```text
Controller
   ↓
IRepository<T>
   ↓
EfRepository<T>
   ↓
EF Core
   ↓
SQL Server
```

Business rule utama berada di `RetributionBusinessService`, bukan di browser.

## Migrasi ke SQL Server / EF Core

### 1. Tambahkan package

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### 2. Buat `AppDbContext`

Mapping tabel dapat mengikuti `Database/01_schema.sql`.

### 3. Buat `EfRepository<T> : IRepository<T>`

Implementasikan:

- `GetAllAsync`
- `GetByIdAsync`
- `AddAsync`
- `UpdateAsync`
- `DeleteAsync`

### 4. Ganti dependency injection

Dari:

```csharp
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(InMemoryRepository<>));
```

menjadi kurang lebih:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
```

Controller dan mayoritas service tidak perlu diubah.

### 5. Khusus relasi Setoran-Penerimaan

Pada versi in-memory, `Deposit.ReceiptIds` berupa list ID untuk mempermudah prototype.

Di SQL Server gunakan tabel:

```text
TrxDeposit
TrxDepositReceipt
TrxReceipt
```

sebagaimana sudah disiapkan pada `01_schema.sql`.

## Catatan sebelum production

Yang masih perlu dikeraskan sebelum go-live:

- Gunakan SQL Server, jangan InMemory.
- CSRF sudah memakai antiforgery pada POST form; tetap aktifkan HTTPS.
- Tambahkan lockout/rate limit login.
- Tambahkan password policy dan reset password.
- Master penting lebih baik soft-delete/nonaktif daripada hard delete.
- Tambahkan referential-integrity guard pada Delete master.
- Tambahkan attachment bukti transfer/setoran bila diperlukan.
- Tambahkan backup database harian.
- Tambahkan structured logging (Serilog atau provider lain).
- Simpan secret/connection string di environment variable / secret store.
- Tambahkan automated test untuk rule serial, tarif, closing period dan deposit.
- Pertimbangkan row-version/concurrency token saat sudah multi-user.

## Batas prototype

Environment pembuatan file ini tidak memiliki runtime `.NET`, sehingga project tidak dapat dikompilasi langsung di environment generator. Struktur dan referensi sudah dicek secara statis, tetapi tetap jalankan `dotnet build` di mesin development sebelum mulai pengembangan lanjutan.


## Revisi form Penerimaan

Form Penerimaan sekarang mengikuti aturan berikut:

- Jika **Unit/Kios/Los** dipilih, sistem otomatis mengambil dari master:
  - Pasar
  - Jenis Retribusi
  - Petugas
  - Media (STRD/Karcis/Tanpa Media)
  - Tarif aktif
- Field Pasar/Jenis/Petugas dikunci pada UI selama Unit dipilih.
- Backend tetap meng-overwrite nilai tersebut dari Master Unit sehingga tidak hanya mengandalkan JavaScript.
- Jika transaksi tidak memakai Unit (contoh Pelataran), Pasar dan Jenis Retribusi dapat dipilih manual.
- Media tampil sebagai field read-only agar user tahu serial yang sedang dicari adalah STRD atau Karcis.
- Sistem menampilkan range serial yang **sudah didistribusikan ke pasar tersebut dan belum dipakai**.
- Klik range tersedia akan memilih nomor pertama dari range sebagai bantuan input.
- Pesan validasi serial sekarang menyebutkan media, pasar, dan range distribusi yang tersedia.

## Format angka

Output uang pada view menggunakan `Helpers/FormatHelper.cs`.

Contoh:

```text
Rp25.000
Rp1.250.000
```

Jangan lagi menulis Razor seperti:

```cshtml
Rp@Model.Amount.ToString("N0")
```

Gunakan:

```cshtml
@FormatHelper.Rupiah(Model.Amount)
```


## Revisi UI modern

Versi terbaru menambahkan:

- tampilan modern minimalis dengan tema biru-ungu
- dashboard chart yang lebih visual (donut + bar chart)
- loading overlay untuk semua submit form
- modal create/edit untuk mayoritas halaman index agar input lebih cepat
- style tabel, tombol, form, alert dan panel yang lebih modern


## Revisi v4 - CRUD modal, Select2, numeric formatting

Perbaikan utama:

- Bug submit modal diperbaiki: form Create/Edit/Delete yang tidak memiliki `action` eksplisit sekarang otomatis menggunakan URL form asal, sehingga POST tidak lagi masuk ke halaman Index/grid.
- Create/Edit/Delete dibuka sebagai modal pada halaman CRUD yang mendukungnya.
- Seluruh dropdown di halaman aplikasi diinisialisasi dengan Select2 (dengan fallback ke native select jika library gagal dimuat).
- Input nominal memakai format ribuan langsung saat mengetik (`1000` -> `1.000`), lalu dinormalisasi menjadi digit murni sebelum POST agar model binding tetap aman.
- Nomor seri dan nomor telepon hanya menerima digit tanpa separator agar leading zero tetap terjaga.
- Terminologi penerimaan `VOID` pada UI diganti menjadi `Hapus`; implementasinya tetap soft-delete/audit-friendly dengan status `DELETED`.
- Aksi Ubah/Hapus di grid menggunakan icon-only button dengan title/aria-label.


## Revisi UX v5

- Posisi scroll sidebar disimpan di `sessionStorage`, jadi setelah pindah menu sidebar tetap berada pada posisi sebelumnya.
- Badge jenis chart seperti Donut / Bar / Tabel di Dashboard dihapus.
- Area user kanan atas dibuat compact: icon theme, icon user, dan icon logout.
- Light/Dark mode dapat ditoggle dan preferensi disimpan di `localStorage`.
- Warna legend/tick/grid Chart.js ikut menyesuaikan ketika theme berubah.


## Fix Receipt Unit Auto-fill (v5.1)

Perbaikan bug pada Catat Penerimaan: saat Unit dipilih, frontend tidak lagi mensyaratkan MarketId/RetributionTypeId sudah terisi sebelum memanggil `Receipts/FormContext`. Backend sekarang dapat menurunkan Pasar, Jenis Retribusi, Media, Petugas, Tarif, nominal, dan range serial dari unit, kemudian Select2 di-refresh secara eksplisit.


## v6 - SQL Server / Staging Ready

Versi ini mempertahankan UI dan flow v5.1, tetapi persistence sudah diganti:

```text
Controller / Service
       ↓
IRepository<T>
       ↓
EF Core Repository
       ↓
SQL Server
```

Perubahan backend:

- `AppDbContext`
- `EfRepository<T>`
- `EfDepositRepository` khusus junction Setoran-Penerimaan
- SQL Server auto-create untuk database staging kosong
- seed demo idempotent untuk first run
- SQL schema fallback
- production connection string template
- staging publish guide

Untuk panduan publish baca `PUBLISH_STAGING.md`.


## v6.1 SQL Server transaction fix

`EnableRetryOnFailure` dihapus dari konfigurasi SQL Server karena aplikasi menggunakan explicit database transactions pada database initializer dan repository setoran. Ini mencegah error `SqlServerRetryingExecutionStrategy does not support user-initiated transactions` pada startup/penggunaan setoran.
