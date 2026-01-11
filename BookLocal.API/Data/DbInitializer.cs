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
        string[] roleNames = { "owner", "customer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName.ToLower()))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName.ToLower()));
            }
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

        if (await userManager.Users.AnyAsync()) return;

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

        var realisticDataStore = RealisticDataStore.GetData();
        var mainCategoriesFromDb = await context.MainCategories.ToListAsync();
        var availableMainCategories = mainCategoriesFromDb.Where(mc => realisticDataStore.ContainsKey(mc.Name)).ToList();
        var businesses = new List<Business>();

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
                .Generate();
            business.Owner = owner;

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
                    int maxAmountToPick = Math.Min(10, realisticData.Services.Count);
                    int minAmountToPick = Math.Min(2, maxAmountToPick);
                    int amountToPick = faker.Random.Number(minAmountToPick, maxAmountToPick);
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
                            new ServiceVariant
                            {
                                Name = "Standard",
                                Price = basePrice,
                                DurationMinutes = template.DurationMinutes,
                                CleanupTimeMinutes = 15,
                                IsDefault = true,
                                IsActive = true
                            },
                            new ServiceVariant
                            {
                                Name = "Premium / Długie",
                                Price = basePrice * 1.3m,
                                DurationMinutes = template.DurationMinutes + 15,
                                CleanupTimeMinutes = 20,
                                IsDefault = false,
                                IsActive = true
                            }
                        }
                    };
                    services.Add(service);
                }

                subCategory.Services = services;
                businessCategories.Add(subCategory);
            }
            business.Categories = businessCategories;

            var employees = new Faker<Employee>("pl")
                .RuleFor(e => e.FirstName, f => f.Name.FirstName())
                .RuleFor(e => e.LastName, f => f.Name.LastName())
                .RuleFor(e => e.Position, f => f.PickRandom(realisticData.EmployeePositions))
                .RuleFor(e => e.PhotoUrl, f => f.Internet.Avatar())
                .RuleFor(e => e.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Past(40, DateTime.UtcNow.AddYears(-18))))
                .Generate(faker.Random.Number(3, 5));

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
                };

                emp.FinanceSettings = new EmployeeFinanceSettings
                {
                    CommuteType = WorkCommuteType.Local,
                    HasPit2Filed = true,
                    HourlyRate = Math.Round(faker.Random.Decimal(25, 100), 2)
                };
            }

            business.Employees = employees;
            businesses.Add(business);
        }
        await context.Businesses.AddRangeAsync(businesses);
        await context.SaveChangesAsync();

        var businessRatingProfiles = new Dictionary<int, List<int>>();
        foreach (var business in businesses)
        {
            var ratingProfile = faker.PickRandom(RealisticDataStore.RatingProfiles);
            businessRatingProfiles.Add(business.BusinessId, ratingProfile);
        }

        foreach (var business in businesses)
        {
            var allServicesInBusiness = business.Categories.SelectMany(c => c.Services).ToList();

            foreach (var employee in business.Employees)
            {
                if (allServicesInBusiness.Any())
                {
                    int amountToPick = faker.Random.Number(1, Math.Min(5, allServicesInBusiness.Count));
                    var servicesForEmployee = faker.PickRandom(allServicesInBusiness, amountToPick);
                    foreach (var service in servicesForEmployee)
                    {
                        context.EmployeeServices.Add(new EmployeeService
                        {
                            EmployeeId = employee.EmployeeId,
                            ServiceId = service.ServiceId
                        });
                    }
                }

                var dayOff = faker.PickRandom<DayOfWeek>();
                for (int i = 0; i < 7; i++)
                {
                    var day = (DayOfWeek)i;
                    var isDayOff = day == dayOff;
                    context.WorkSchedules.Add(new WorkSchedule
                    {
                        EmployeeId = employee.EmployeeId,
                        DayOfWeek = day,
                        IsDayOff = isDayOff,
                        StartTime = isDayOff ? null : new TimeSpan(faker.Random.ListItem(new List<int> { 8, 9, 10 }), 0, 0),
                        EndTime = isDayOff ? null : new TimeSpan(faker.Random.ListItem(new List<int> { 16, 17, 18, 19 }), 0, 0)
                    });
                }
            }
        }
        await context.SaveChangesAsync();

        // Rezerwacje
        Console.WriteLine("Generowanie rezerwacji...");
        var reservations = new List<Reservation>();

        var allBusinesses = await context.Businesses
            .Include(b => b.Employees)
            .Include(b => b.Categories)
                .ThenInclude(c => c.Services)
                    .ThenInclude(s => s.Variants)
            .ToListAsync();

        var currentYear = DateTime.UtcNow.Year;
        var startDate = DateTime.UtcNow.AddMonths(-6);
        var endDate = DateTime.UtcNow.AddMonths(3);
        var simulationNow = DateTime.UtcNow;

        foreach (var customer in customers)
        {
            int reservationCount = faker.Random.Number(15, 25);

            for (int i = 0; i < reservationCount; i++)
            {
                var randomBusiness = faker.PickRandom(allBusinesses.Where(b => b.Categories.SelectMany(c => c.Services).Any()));
                if (randomBusiness == null) continue;

                var servicesInBusiness = randomBusiness.Categories.SelectMany(c => c.Services).ToList();
                var employeesInBusiness = randomBusiness.Employees.ToList();

                if (!servicesInBusiness.Any() || !employeesInBusiness.Any()) continue;

                var randomService = faker.PickRandom(servicesInBusiness);
                var randomEmployee = faker.PickRandom(employeesInBusiness);

                if (!randomService.Variants.Any()) continue;
                var randomVariant = faker.PickRandom(randomService.Variants);

                var startTime = faker.Date.Between(startDate, endDate);
                var status = startTime < simulationNow
                    ? faker.PickRandom(new[] { ReservationStatus.Completed, ReservationStatus.Completed, ReservationStatus.Cancelled, ReservationStatus.NoShow })
                    : ReservationStatus.Confirmed;

                reservations.Add(new Reservation
                {
                    Customer = customer,
                    BusinessId = randomBusiness.BusinessId,
                    EmployeeId = randomEmployee.EmployeeId,
                    ServiceVariantId = randomVariant.ServiceVariantId,
                    AgreedPrice = randomVariant.Price,
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(randomVariant.DurationMinutes + randomVariant.CleanupTimeMinutes),
                    Status = status
                });
            }
        }
        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
        Console.WriteLine($"Stworzono {reservations.Count} rezerwacji.");

        // Opinie
        var reviews = new List<Review>();

        var completedReservations = await context.Reservations
            .Where(r => r.Status == ReservationStatus.Completed)
            .Include(r => r.Customer)
            .Include(r => r.ServiceVariant)
            .ToListAsync();

        Console.WriteLine($"Tworzenie opinii dla {completedReservations.Count} wizyt.");

        foreach (var reservation in faker.PickRandom(completedReservations, (int)(completedReservations.Count * 0.7)))
        {
            if (reservation.Customer == null) continue;

            var ratingProfile = businessRatingProfiles.ContainsKey(reservation.BusinessId)
                ? businessRatingProfiles[reservation.BusinessId]
                : RealisticDataStore.RatingProfiles[0];

            var review = new Faker<Review>("pl")
                .RuleFor(r => r.Rating, f => f.PickRandom(ratingProfile))
                .RuleFor(r => r.Comment, f => f.Rant.Review())
                .RuleFor(r => r.CreatedAt, f => reservation.EndTime.AddDays(f.Random.Int(1, 14)))
                .RuleFor(r => r.ReviewerName, f => $"{reservation.Customer.FirstName} {reservation.Customer.LastName}")
                .RuleFor(r => r.BusinessId, f => reservation.BusinessId)
                .RuleFor(r => r.UserId, f => reservation.CustomerId)
                .RuleFor(r => r.ReservationId, f => reservation.ReservationId)
                .Generate();
            reviews.Add(review);
        }
        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();
        Console.WriteLine($"Stworzono {reviews.Count} opinii.");

        // CRM: Generowanie Profili Klientów
        Console.WriteLine("Generowanie profili klientów CRM...");
        var profiles = reservations
            .Where(r => r.CustomerId != null)
            .GroupBy(r => new { r.BusinessId, CustomerId = r.CustomerId! })
            .Select(g => new CustomerBusinessProfile
            {
                BusinessId = g.Key.BusinessId,
                CustomerId = g.Key.CustomerId,
                LastVisitDate = g.Where(r => r.Status == ReservationStatus.Completed).Any()
                    ? g.Where(r => r.Status == ReservationStatus.Completed).Max(r => r.StartTime)
                    : DateTime.MinValue,
                TotalSpent = g.Where(r => r.Status == ReservationStatus.Completed).Sum(r => r.AgreedPrice),
                NoShowCount = g.Count(r => r.Status == ReservationStatus.NoShow),
                PrivateNotes = faker.Random.Bool(0.2f) ? faker.Lorem.Sentence() : null,
                IsVIP = faker.Random.Bool(0.05f),
                IsBanned = faker.Random.Bool(0.01f)
            })
            .ToList();

        // Fix duplicate property assignment in Select if happened
        // Actually Select above uses initializer, duplicate NoShowCount assignment is harmless but messy.
        // I will clean it up in the actual string.

        await context.CustomerBusinessProfiles.AddRangeAsync(profiles);
        await context.SaveChangesAsync();
        Console.WriteLine($"Stworzono {profiles.Count} profili CRM.");
    }
}