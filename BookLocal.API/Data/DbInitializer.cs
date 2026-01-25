using Bogus;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Data;

public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {

        // Role
        string[] roleNames = { "owner", "customer", "superadmin" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName.ToLower()))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName.ToLower()));
            }
        }

        // Subskrypcje (Plany)
        if (!await context.SubscriptionPlans.AnyAsync())
        {
            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan
                {
                    Name = "Free",
                    PriceMonthly = 0,
                    PriceYearly = 0,
                    MaxEmployees = 1,
                    MaxServices = 5,
                    HasAdvancedReports = false,
                    HasMarketingTools = false,
                    CommissionPercentage = 0,
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Name = "Silver",
                    PriceMonthly = 49.99m,
                    PriceYearly = 499.00m,
                    MaxEmployees = 5,
                    MaxServices = 20,
                    HasAdvancedReports = true,
                    HasMarketingTools = false,
                    CommissionPercentage = 2.5m,
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Name = "Gold",
                    PriceMonthly = 99.99m,
                    PriceYearly = 999.00m,
                    MaxEmployees = 100,
                    MaxServices = 100,
                    HasAdvancedReports = true,
                    HasMarketingTools = true,
                    CommissionPercentage = 1.0m,
                    IsActive = true
                }
            };
            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
        }

        // Kategorie Główne
        if (!await context.MainCategories.AnyAsync())
        {
            var mainCategories = new List<MainCategory>
            {
                new MainCategory { Name = "Fryzjer" }, new MainCategory { Name = "Barber" },
                new MainCategory { Name = "Masaż" }, new MainCategory { Name = "Fizjoterapia" },
                new MainCategory { Name = "Psycholog" }, new MainCategory { Name = "Pilates" },
                new MainCategory { Name = "Joga" }, new MainCategory { Name = "Kosmetyczka" },
                new MainCategory { Name = "Groomer" }, new MainCategory { Name = "Tatuaż" },
                new MainCategory { Name = "Medycyna Estetyczna" }, new MainCategory { Name = "Trening Personalny" }
            };
            await context.MainCategories.AddRangeAsync(mainCategories);
            await context.SaveChangesAsync();
        }

        // --- SUPER ADMIN SEEDING (Always Run) ---
        var superAdmin = new User
        {
            FirstName = "Admin",
            LastName = "System",
            UserName = "admin@booklocal.com",
            Email = "admin@booklocal.com",
            PhotoUrl = "https://ui-avatars.com/api/?name=Admin+System&background=random"
        };

        // Fix for "Cannot access disposed object" or context tracking issues if any? No, scoped.
        // Actually, just FindByEmailAsync is enough.
        if (await userManager.FindByEmailAsync(superAdmin.Email) == null)
        {
            await userManager.CreateAsync(superAdmin, "Admin123!");
            await userManager.AddToRoleAsync(superAdmin, "superadmin");
        }

        // Jeśli mamy więcej niż 1 użytkownika (czyli coś poza Adminem), nie seedujemy dalej
        if (await userManager.Users.CountAsync() > 1) return;

        Randomizer.Seed = new Random(42);
        var faker = new Faker("pl");

        // Użytkownicy 
        var owners = new List<User>();
        var ownerFaker = new Faker<User>("pl")
            .RuleFor(u => u.UserName, f => f.Internet.Email(f.Person.FirstName))
            .RuleFor(u => u.Email, (f, u) => u.UserName)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.PhotoUrl, f => f.Internet.Avatar());

        for (int i = 0; i < 29; i++)
        {
            var owner = ownerFaker.Generate();
            await userManager.CreateAsync(owner, "P@ssword1");
            await userManager.AddToRoleAsync(owner, "owner");
            owners.Add(owner);
        }
        var staticOwner = new User { FirstName = "Jan", LastName = "Testowy", UserName = "owner@test.com", Email = "owner@test.com", PhotoUrl = faker.Internet.Avatar() };
        await userManager.CreateAsync(staticOwner, "P@ssword1");
        await userManager.AddToRoleAsync(staticOwner, "owner");
        owners.Add(staticOwner);



        var customers = new List<User>();
        var customerFaker = new Faker<User>("pl")
            .RuleFor(u => u.UserName, f => f.Internet.Email())
            .RuleFor(u => u.Email, (f, u) => u.UserName)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.PhotoUrl, f => f.Internet.Avatar());

        for (int i = 0; i < 769; i++)
        {
            var customer = customerFaker.Generate();
            await userManager.CreateAsync(customer, "P@ssword1");
            await userManager.AddToRoleAsync(customer, "customer");
            customers.Add(customer);
        }
        var staticCustomer = new User { FirstName = "Anna", LastName = "Testowa", UserName = "customer@test.com", Email = "customer@test.com", PhotoUrl = faker.Internet.Avatar() };
        await userManager.CreateAsync(staticCustomer, "P@ssword1");
        await userManager.AddToRoleAsync(staticCustomer, "customer");
        customers.Add(staticCustomer);


        // Firmy, Usługi i Pracownicy

        // ... (Existing Owners/Customers seeding remains) ...

        // Firmy, Usługi i Pracownicy
        var realisticDataStore = RealisticDataStore.GetData();
        var mainCategoriesFromDb = await context.MainCategories.ToListAsync();
        var availableMainCategories = mainCategoriesFromDb.Where(mc => realisticDataStore.ContainsKey(mc.Name)).ToList();
        var businesses = new List<Business>();

        // Pre-fetch plans
        var subscriptionPlans = await context.SubscriptionPlans.ToListAsync();
        var freePlan = subscriptionPlans.First(p => p.Name == "Free");
        var silverPlan = subscriptionPlans.First(p => p.Name == "Silver");
        var goldPlan = subscriptionPlans.First(p => p.Name == "Gold");

        foreach (var owner in owners)
        {
            var mainCategoryForBusiness = faker.PickRandom(availableMainCategories);
            var realisticData = realisticDataStore[mainCategoryForBusiness.Name];
            var businessName = faker.PickRandom(realisticData.BusinessNameTemplates).Replace("{Name}", faker.Name.LastName());

            var business = new Faker<Business>("pl")
                .RuleFor(b => b.Name, f => businessName)
                .RuleFor(b => b.Description, f => f.Lorem.Sentence(25))
                .RuleFor(b => b.City, f => f.Address.City())
                .RuleFor(b => b.Address, f => f.Address.StreetAddress())
                .RuleFor(b => b.NIP, f => f.Random.Replace("##########"))
                .RuleFor(b => b.PhotoUrl, f => f.Image.PicsumUrl(640, 480))
                .RuleFor(b => b.VerificationStatus, f => VerificationStatus.Approved)
                .RuleFor(b => b.IsVerified, f => true)
                .Generate();
            business.Owner = owner;

            // Categories & Services (Existing logic) ...
            var businessCategories = new List<ServiceCategory>();
            var subCategoryNames = faker.PickRandom(realisticData.SubCategoryNames, faker.Random.Number(1, Math.Min(3, realisticData.SubCategoryNames.Count))).ToList();

            foreach (var subCatName in subCategoryNames)
            {
                var subCategory = new ServiceCategory
                {
                    Name = subCatName,
                    PhotoUrl = faker.Image.PicsumUrl(200, 200),
                    Business = business,
                    MainCategory = mainCategoryForBusiness
                };

                var servicesForSubCategory = new List<ServiceTemplate>();
                if (realisticData.Services.Any())
                {
                    int amountToPick = faker.Random.Number(2, 6);
                    if (amountToPick > 0)
                    {
                        servicesForSubCategory = faker.PickRandom(realisticData.Services, amountToPick).ToList();
                    }
                }

                var services = new List<Service>();
                foreach (var template in servicesForSubCategory)
                {
                    var basePrice = Math.Round(faker.Random.Decimal(template.MinPrice, template.MaxPrice) / 5) * 5;

                    var service = new Service
                    {
                        Name = template.Name,
                        Description = faker.Lorem.Sentence(),
                        Business = business,
                        ServiceCategory = subCategory,
                        IsArchived = false,
                        Variants = new List<ServiceVariant>
                        {
                            new ServiceVariant { Name = "Standard", Price = basePrice, DurationMinutes = template.DurationMinutes, CleanupTimeMinutes = 10, IsDefault = true, IsActive = true },
                            new ServiceVariant { Name = "Premium", Price = basePrice * 1.3m, DurationMinutes = template.DurationMinutes + 15, CleanupTimeMinutes = 15, IsDefault = false, IsActive = true }
                        }
                    };
                    services.Add(service);
                }
                subCategory.Services = services;
                businessCategories.Add(subCategory);
            }
            business.Categories = businessCategories;

            // Employees
            var employees = new Faker<Employee>("pl")
                .RuleFor(e => e.FirstName, f => f.Name.FirstName())
                .RuleFor(e => e.LastName, f => f.Name.LastName())
                .RuleFor(e => e.Position, f => f.PickRandom(realisticData.EmployeePositions))
                .RuleFor(e => e.PhotoUrl, f => f.Internet.Avatar())
                .RuleFor(e => e.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Past(40, DateTime.UtcNow.AddYears(-18))))
                .Generate(faker.Random.Number(2, 5));

            var ownerAsEmployee = new Employee
            {
                FirstName = owner.FirstName,
                LastName = owner.LastName,
                Position = "Właściciel",
                PhotoUrl = owner.PhotoUrl,
                Business = business,
                DateOfBirth = new DateOnly(1985, 1, 1)
            };
            employees.Add(ownerAsEmployee);

            foreach (var emp in employees)
            {
                emp.Business = business;
                emp.EmployeeDetails = new EmployeeDetails
                {
                    Bio = faker.Lorem.Sentences(2),
                    Specialization = faker.PickRandom(realisticData.SubCategoryNames)
                    // Removing Pesel, Address, BankAccountNumber as they are missing in Model
                };

                emp.FinanceSettings = new EmployeeFinanceSettings
                {
                    CommuteType = WorkCommuteType.Local,
                    HasPit2Filed = true,
                    HourlyRate = Math.Round(faker.Random.Decimal(25, 80), 2)
                };

                // Seed Contracts
                context.EmploymentContracts.Add(new EmploymentContract
                {
                    Employee = emp,
                    ContractType = faker.PickRandom<ContractType>(),
                    BaseSalary = faker.Random.Decimal(4000, 8000),
                    StartDate = new DateOnly(2024, 1, 1),
                    EndDate = new DateOnly(2025, 12, 31),
                    IsActive = true
                });
            }
            business.Employees = employees;
            businesses.Add(business);

            // Discounts
            var discount = new Discount
            {
                Business = business,
                Code = "START2025",
                Value = 10,
                Type = DiscountType.Percentage,
                ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(2)),
                IsActive = true
            };
            context.Discounts.Add(discount);
        }
        await context.Businesses.AddRangeAsync(businesses);
        await context.SaveChangesAsync();


        // Seed Subscriptions for Businesses (Bias towards GOLD)
        foreach (var bus in businesses)
        {
            // 80% Gold, 20% Silver, 0% Free (for testing reports)
            var plan = faker.Random.Bool(0.8f) ? goldPlan : silverPlan;

            context.BusinessSubscriptions.Add(new BusinessSubscription
            {
                BusinessId = bus.BusinessId,
                PlanId = plan.PlanId,
                StartDate = DateTime.UtcNow.AddMonths(-4),
                EndDate = DateTime.UtcNow.AddMonths(2),
                IsActive = true,
                IsAutoRenew = true
            });
        }
        await context.SaveChangesAsync();


        // Assign Services and Schedules
        foreach (var business in businesses)
        {
            var allServicesInBusiness = business.Categories.SelectMany(c => c.Services).ToList();
            foreach (var employee in business.Employees)
            {
                if (allServicesInBusiness.Any())
                {
                    int amountToPick = faker.Random.Number(3, Math.Min(8, allServicesInBusiness.Count));
                    var servicesForEmployee = faker.PickRandom(allServicesInBusiness, amountToPick);
                    foreach (var service in servicesForEmployee)
                    {
                        context.EmployeeServices.Add(new EmployeeService { EmployeeId = employee.EmployeeId, ServiceId = service.ServiceId });
                    }
                }

                // WorkSchedule (Fixed 9-17 Mon-Fri for simplicity and reliability)
                for (int i = 0; i < 7; i++)
                {
                    var day = (DayOfWeek)i;
                    var isWeekend = day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
                    context.WorkSchedules.Add(new WorkSchedule
                    {
                        EmployeeId = employee.EmployeeId,
                        DayOfWeek = day,
                        IsDayOff = isWeekend,
                        StartTime = isWeekend ? null : new TimeSpan(9, 0, 0),
                        EndTime = isWeekend ? null : new TimeSpan(17, 0, 0)
                    });
                }
            }
        }
        await context.SaveChangesAsync();


        // Rezerwacje - MASSIVE SEEDING
        Console.WriteLine("Generowanie rezerwacji (to może chwilę potrwać)...");
        var reservations = new List<Reservation>();

        var allBusinesses = await context.Businesses
             .Include(b => b.Employees).ThenInclude(e => e.FinanceSettings)
             .Include(b => b.Categories).ThenInclude(c => c.Services).ThenInclude(s => s.Variants)
             .ToListAsync();

        var businessPlans = await context.BusinessSubscriptions.Include(bs => bs.Plan).ToDictionaryAsync(bs => bs.BusinessId, bs => bs.Plan);

        var currentYear = DateTime.UtcNow.Year;
        // Generate for last 3 months
        var startDate = DateTime.UtcNow.AddMonths(-3).Date;
        var endDate = DateTime.UtcNow.AddDays(1).Date;

        // Cache schedules
        var schedules = await context.WorkSchedules.ToListAsync();

        foreach (var customer in customers)
        {
            // Much more visits per customer: 20-50
            int reservationCount = faker.Random.Number(20, 50);

            for (int i = 0; i < reservationCount; i++)
            {
                // Assign to random business
                var randomBusiness = faker.PickRandom(allBusinesses.Where(b => b.Categories.SelectMany(c => c.Services).Any()));
                if (randomBusiness == null) continue;

                var servicesInBusiness = randomBusiness.Categories.SelectMany(c => c.Services).ToList();
                var employeesInBusiness = randomBusiness.Employees.ToList();

                if (!servicesInBusiness.Any() || !employeesInBusiness.Any()) continue;

                var randomService = faker.PickRandom(servicesInBusiness);
                // Pick employee that performs this service (ideally), but for now pick random employee in business (simplified)
                // Better: Pick random employee
                var randomEmployee = faker.PickRandom(employeesInBusiness);

                if (!randomService.Variants.Any()) continue;
                var randomVariant = faker.PickRandom(randomService.Variants);

                // Find valid slot
                DateTime? validStartTime = null;
                int attempts = 0;
                while (validStartTime == null && attempts < 10)
                {
                    var baseDate = faker.Date.Between(startDate, endDate).Date;
                    var daySchedule = schedules.FirstOrDefault(s => s.EmployeeId == randomEmployee.EmployeeId && s.DayOfWeek == baseDate.DayOfWeek);

                    if (daySchedule != null && !daySchedule.IsDayOff && daySchedule.StartTime.HasValue && daySchedule.EndTime.HasValue)
                    {
                        // Generate time between Start and End - Duration
                        var startTs = daySchedule.StartTime.Value;
                        var endTs = daySchedule.EndTime.Value;
                        var maxMinutes = (endTs - startTs).TotalMinutes - randomVariant.DurationMinutes - 15; // buffer

                        if (maxMinutes > 0)
                        {
                            var offsetMinutes = faker.Random.Int(0, (int)maxMinutes);
                            validStartTime = baseDate.Add(startTs).AddMinutes(offsetMinutes);
                        }
                    }
                    attempts++;
                }

                if (validStartTime == null) continue; // Skip if cant find slot

                var startTime = validStartTime.Value;
                var duration = randomVariant.DurationMinutes + randomVariant.CleanupTimeMinutes;
                var endTime = startTime.AddMinutes(duration);

                var status = startTime < DateTime.UtcNow
                    ? faker.Random.Bool(0.9f) ? ReservationStatus.Completed : ReservationStatus.Cancelled // 90% Completed
                    : ReservationStatus.Confirmed;

                var paymentMethod = faker.PickRandom<PaymentMethod>();

                var reservation = new Reservation
                {
                    Customer = customer,
                    BusinessId = randomBusiness.BusinessId,
                    EmployeeId = randomEmployee.EmployeeId,
                    ServiceVariantId = randomVariant.ServiceVariantId,
                    AgreedPrice = randomVariant.Price,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = status,
                    PaymentMethod = paymentMethod
                };
                reservations.Add(reservation);
            }
        }
        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();

        // Process Payments
        var completedRes = reservations.Where(r => r.Status == ReservationStatus.Completed).ToList();
        var paymentsToAdd = new List<Payment>();

        foreach (var res in completedRes)
        {
            decimal commission = 0;
            if (res.PaymentMethod == PaymentMethod.Online && businessPlans.ContainsKey(res.BusinessId))
            {
                var rate = businessPlans[res.BusinessId].CommissionPercentage;
                commission = Math.Round(res.AgreedPrice * (rate / 100m), 2);
            }

            paymentsToAdd.Add(new Payment
            {
                ReservationId = res.ReservationId,
                BusinessId = res.BusinessId,
                PaymentMethod = res.PaymentMethod,
                Amount = res.AgreedPrice,
                CommissionAmount = commission,
                Currency = "PLN",
                Status = PaymentStatus.Completed,
                TransactionDate = res.EndTime
            });
        }
        await context.Payments.AddRangeAsync(paymentsToAdd);
        await context.SaveChangesAsync();

        Console.WriteLine($"Stworzono {reservations.Count} rezerwacji i {paymentsToAdd.Count} płatności.");

        // Reviews
        var reviews = new List<Review>();
        foreach (var r in completedRes.Where(x => faker.Random.Bool(0.4f)))
        {
            reviews.Add(new Review
            {
                Rating = faker.Random.Int(4, 5), // Mostly good reviews
                Comment = faker.Rant.Review(),
                CreatedAt = r.EndTime.AddHours(2),
                ReviewerName = r.Customer.FirstName,
                BusinessId = r.BusinessId,
                UserId = r.Customer.Id,
                ReservationId = r.ReservationId
            });
        }
        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();

        // --- GENERATE REPORTS (DailyFinancialReport) ---
        Console.WriteLine("Generowanie raportów finansowych...");
        var reportsToAdd = new List<DailyFinancialReport>();

        // Group payments by Business and Date
        var paymentsByBusinessAndDate = paymentsToAdd
            .GroupBy(p => new { p.BusinessId, Date = DateOnly.FromDateTime(p.TransactionDate) });

        foreach (var group in paymentsByBusinessAndDate)
        {
            var busId = group.Key.BusinessId;
            var date = group.Key.Date;
            var daysPayments = group.ToList();

            // Get reservations for this day to count appointments
            var daysReservations = reservations.Where(r => r.BusinessId == busId && DateOnly.FromDateTime(r.StartTime) == date).ToList();

            if (!daysReservations.Any()) continue;

            // Plan commission
            decimal planCommissionRate = 0;
            if (businessPlans.TryGetValue(busId, out var subPlan))
            {
                planCommissionRate = subPlan.CommissionPercentage;
            }

            // Calculate metrics
            var totalRev = daysPayments.Sum(p => p.Amount);
            var commissionVal = totalRev * (planCommissionRate / 100m);

            var report = new DailyFinancialReport
            {
                BusinessId = busId,
                ReportDate = date,
                TotalRevenue = totalRev,
                TotalCommission = commissionVal,

                CashRevenue = daysPayments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount),
                CardRevenue = daysPayments.Where(p => p.PaymentMethod == PaymentMethod.Card).Sum(p => p.Amount),
                OnlineRevenue = daysPayments.Where(p => p.PaymentMethod == PaymentMethod.Online).Sum(p => p.Amount),

                TotalAppointments = daysReservations.Count,
                CompletedAppointments = daysReservations.Count(r => r.Status == ReservationStatus.Completed),
                CancelledAppointments = daysReservations.Count(r => r.Status == ReservationStatus.Cancelled),
                NoShowCount = daysReservations.Count(r => r.Status == ReservationStatus.NoShow),

                NewCustomersCount = faker.Random.Number(0, 3), // Simplified
                ReturningCustomersCount = faker.Random.Number(0, 5),
                OccupancyRate = faker.Random.Double(0.6, 0.95),
                AverageTicketValue = daysReservations.Where(r => r.Status == ReservationStatus.Completed).Any()
                    ? totalRev / daysReservations.Count(r => r.Status == ReservationStatus.Completed)
                    : 0,

                // Find top service
                TopSellingServiceName = daysReservations
                    .Where(r => r.Status == ReservationStatus.Completed)
                    .GroupBy(r => context.ServiceVariants.Local.FirstOrDefault(v => v.ServiceVariantId == r.ServiceVariantId)?.Name ?? "Service") // simplifying since tracked
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "Standard"
            };
            reportsToAdd.Add(report);
        }

        await context.DailyFinancialReports.AddRangeAsync(reportsToAdd);
        await context.SaveChangesAsync();
        Console.WriteLine($"Wygenerowano {reportsToAdd.Count} raportów dziennych.");
    }
}