using BookLocal.Data.Data.CMS;
using BookLocal.Data.Data.PlatformaInternetowa;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.Data.Data
{
    public class BookLocalContext : DbContext
    {
        internal readonly IEnumerable<object> Pracownicy;

        public BookLocalContext(DbContextOptions<BookLocalContext> options)
            : base(options)
        {
        }
        public DbSet<SekcjaCms> SekcjaCms { get; set; } = default!;
        public DbSet<OdnosnikCms> OdnosnikCms { get; set; } = default!;
        public DbSet<ZawartoscCms> ZawartoscCms { get; set; } = default!;
        public DbSet<NaglowekCms> NaglowekCms { get; set; } = default!;
        public DbSet<Adres> Adres { get; set; } = default!;
        public DbSet<Firma> Firma { get; set; } = default!;
        public DbSet<Konwersacja> Konwersacja { get; set; } = default!;
        public DbSet<Opinia> Opinia { get; set; } = default!;
        public DbSet<Pensja> Pensja { get; set; } = default!;
        public DbSet<Pracownik> Pracownik { get; set; } = default!;
        public DbSet<Przedsiebiorca> Przedsiebiorca { get; set; } = default!;
        public DbSet<Rezerwacja> Rezerwacja { get; set; } = default!;
        public DbSet<SzczegolyUslugi> SzczegolyUslugi { get; set; } = default!;
        public DbSet<TransakcjaRozliczeniowa> TransakcjaRozliczeniowa { get; set; } = default!;
        public DbSet<Usluga> Usluga { get; set; } = default!;
        public DbSet<Uzytkownik> Uzytkownik { get; set; } = default!;
        public DbSet<Wiadomosc> Wiadomosc { get; set; } = default!;
        

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetModificationDates(); 
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetModificationDates()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                    (e.Entity is ZawartoscCms || e.Entity is SekcjaCms) &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified));

            var currentTime = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.Entity is ZawartoscCms zawartosc)
                {
                    zawartosc.DataModyfikacji = currentTime;
                }
                else if (entry.Entity is SekcjaCms sekcja)
                {
                    sekcja.LastModifiedDate = currentTime;
                }
            }
        }
    }
}
