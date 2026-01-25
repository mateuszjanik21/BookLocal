using BookLocal.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Business> Businesses { get; set; }
    public DbSet<MainCategory> MainCategories { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }

    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceVariant> ServiceVariants { get; set; }
    public DbSet<ServiceBundle> ServiceBundles { get; set; }
    public DbSet<ServiceBundleItem> ServiceBundleItems { get; set; }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeService> EmployeeServices { get; set; }
    public DbSet<WorkSchedule> WorkSchedules { get; set; }

    public DbSet<EmployeeDetails> EmployeeDetails { get; set; }
    public DbSet<EmployeeCertificate> EmployeeCertificates { get; set; }
    public DbSet<EmploymentContract> EmploymentContracts { get; set; }
    public DbSet<CommissionRate> CommissionRates { get; set; }
    public DbSet<EmployeePayroll> EmployeePayrolls { get; set; }
    public DbSet<ScheduleException> ScheduleExceptions { get; set; }
    public DbSet<EmployeeFinanceSettings> EmployeeFinanceSettings { get; set; }

    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<DailyFinancialReport> DailyFinancialReports { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<DailyEmployeePerformance> DailyEmployeePerformances { get; set; }

    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<BusinessSubscription> BusinessSubscriptions { get; set; }
    public DbSet<BusinessVerification> BusinessVerifications { get; set; }

    public DbSet<CustomerBusinessProfile> CustomerBusinessProfiles { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
    public DbSet<LoyaltyProgramConfig> LoyaltyProgramConfigs { get; set; }
    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(10, 2)");
        }

        modelBuilder.Entity<Service>().HasQueryFilter(s => !s.IsArchived);
        modelBuilder.Entity<ServiceVariant>().HasQueryFilter(v => !v.Service.IsArchived);
        modelBuilder.Entity<EmployeeService>().HasQueryFilter(es => !es.Service.IsArchived);

        modelBuilder.Entity<EmployeeService>()
            .HasKey(es => new { es.EmployeeId, es.ServiceId });

        modelBuilder.Entity<Reservation>()
            .Property(r => r.Status)
            .HasConversion<string>();

        modelBuilder.Entity<WorkSchedule>()
            .Property(ws => ws.DayOfWeek)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<BusinessVerification>()
            .Property(v => v.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Discount>()
            .Property(d => d.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Reservation>()
            .Property(r => r.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<DailyFinancialReport>()
            .Property(r => r.TotalRevenue)
            .HasColumnType("decimal(12, 2)");
    }
}