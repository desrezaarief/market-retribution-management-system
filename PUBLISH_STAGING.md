# Publish Staging / Demo Gratis

Project ini sekarang memakai **EF Core + SQL Server**. Data tidak lagi disimpan di RAM, sehingga data staging tetap ada setelah aplikasi restart selama database hosting tetap tersedia.

## 1. Siapkan website dan SQL Server pada hosting

Buat website ASP.NET Core .NET 8 dan satu database SQL Server dari control panel hosting.

Catat informasi berikut:

- SQL Server host/server
- Database name
- Username database
- Password database

## 2. Isi connection string Production

Buka:

`RetribusiPasar.Web/appsettings.Production.json`

Ganti:

```json
"DefaultConnection": "Server=YOUR_SQL_SERVER;Database=YOUR_DATABASE;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

dengan connection string database hosting.

Jika control panel hosting memberikan connection string lengkap, lebih aman gunakan string tersebut apa adanya.

> Jangan commit file yang sudah berisi password database ke repository publik.

## 3. Password login staging

Untuk database baru, aplikasi membuat akun demo otomatis:

- `admin.retribusi`
- `operator.panorama`

Password awal berasal dari bagian `Seed` pada `appsettings.Production.json`.

Sebelum publish ke internet, sebaiknya ubah password seed dari `demo123` ke password yang hanya kamu dan reviewer tahu.

Contoh:

```json
"Seed": {
  "AdminPassword": "GantiPasswordStaging!",
  "OperatorPassword": "GantiPasswordOperator!"
}
```

Seed hanya dijalankan ketika database belum mempunyai data pasar. Mengubah config setelah database sudah terisi tidak otomatis mengubah password user yang sudah ada.

## 4. Database otomatis pada first run

Default staging:

```json
"Database": {
  "AutoCreate": true,
  "SeedDemoData": true
}
```

Pada first run aplikasi akan:

1. connect ke SQL Server;
2. membuat tabel jika database masih kosong;
3. membuat seed/master data demo;
4. membuat user demo;
5. menjalankan aplikasi seperti biasa.

Untuk staging ini sengaja memakai `EnsureCreated()` agar deploy pertama sederhana dan tidak membutuhkan command migration di server.

## 5. Fallback jika hosting tidak mengizinkan CREATE TABLE

Jika aplikasi gagal membuat tabel tetapi database dapat diakses:

1. buka SQL management tool/control panel hosting;
2. jalankan `Database/01_schema.sql`;
3. ubah `Database:AutoCreate` menjadi `false`;
4. biarkan `Database:SeedDemoData` tetap `true`;
5. restart website.

Aplikasi akan mengisi seed data ke tabel yang sudah dibuat manual.

## 6. Publish dari Visual Studio

Cara paling mudah jika provider memberi Web Deploy profile:

1. Right click `RetribusiPasar.Web`.
2. Pilih **Publish**.
3. Import publish profile yang diberikan hosting.
4. Pastikan Configuration = **Release**.
5. Publish.

Jika provider hanya menerima file/FTP, kamu bisa memakai profile `StagingFolder` yang sudah disediakan atau jalankan:

```bash
dotnet publish -c Release
```

hasilnya ada di folder publish dan dapat di-upload ke hosting.

## 7. Test setelah publish

Tes urutan berikut:

1. login;
2. buka Dashboard;
3. tambah Master Pasar;
4. refresh halaman dan pastikan data tetap ada;
5. tambah stok media;
6. distribusikan media;
7. buat penerimaan;
8. buat setoran;
9. logout/login lagi;
10. pastikan seluruh data tetap ada.

Jika langkah 4 dan 10 berhasil, persistence SQL Server sudah bekerja.

## Catatan staging

- `SeedDemoData=true` cocok untuk demo/review.
- Jangan masukkan data pribadi/sensitif sungguhan pada staging gratis.
- Untuk production nanti, ubah ke migration-based deployment dan matikan demo seed.
