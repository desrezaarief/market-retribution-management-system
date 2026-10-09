# Database

Aplikasi menggunakan EF Core SQL Server.

- `01_schema.sql`: fallback manual schema untuk database kosong.
- Secara default staging menggunakan `Database:AutoCreate=true`, sehingga script manual biasanya tidak perlu dijalankan.
- Relasi Setoran ke Penerimaan disimpan pada `TrxDepositReceipt` dengan unique index pada `ReceiptId`, sehingga satu penerimaan tidak dapat dimasukkan ke dua setoran sekaligus.
