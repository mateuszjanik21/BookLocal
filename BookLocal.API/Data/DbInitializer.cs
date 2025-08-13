using Bogus;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Data;

public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Krok 1: Stwórz Role, jeśli nie istnieją
        string[] roleNames = { "owner", "customer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName.ToLower()))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName.ToLower()));
            }
        }

        // Krok 2: Stwórz Kategorie Główne, jeśli nie istnieją
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

        // --- Użytkownicy ---
        var owners = new List<User>();
        var ownerFaker = new Faker<User>("pl").RuleFor(u => u.UserName, f => f.Internet.Email(f.Person.FirstName)).RuleFor(u => u.Email, (f, u) => u.UserName).RuleFor(u => u.FirstName, f => f.Name.FirstName()).RuleFor(u => u.LastName, f => f.Name.LastName()).RuleFor(u => u.PhotoUrl, f => f.Internet.Avatar());
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
        var customerFaker = new Faker<User>("pl").RuleFor(u => u.UserName, f => f.Internet.Email()).RuleFor(u => u.Email, (f, u) => u.UserName).RuleFor(u => u.FirstName, f => f.Name.FirstName()).RuleFor(u => u.LastName, f => f.Name.LastName()).RuleFor(u => u.PhotoUrl, f => f.Internet.Avatar());
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


        // --- Firmy i ich Kategorie/Usługi/Pracownicy ---

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

                var services = servicesForSubCategory.Select(template => new Service
                {
                    Name = template.Name,
                    Price = Math.Round(faker.Random.Decimal(template.MinPrice, template.MaxPrice) / 5) * 5,
                    DurationMinutes = template.DurationMinutes,
                    Business = business,
                    ServiceCategory = subCategory
                }).ToList();

                subCategory.Services = services;
                businessCategories.Add(subCategory);
            }
            business.Categories = businessCategories;

            var employees = new Faker<Employee>("pl")
                .RuleFor(e => e.FirstName, f => f.Name.FirstName())
                .RuleFor(e => e.LastName, f => f.Name.LastName())
                .RuleFor(e => e.Position, f => f.PickRandom(realisticData.EmployeePositions))
                .RuleFor(e => e.PhotoUrl, f => f.Internet.Avatar())
                .Generate(faker.Random.Number(3, 5));

            foreach (var employee in employees) { employee.Business = business; }
            employees.Add(new Employee { FirstName = owner.FirstName, LastName = owner.LastName, Position = "Właściciel", PhotoUrl = owner.PhotoUrl, Business = business });
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

        // --- Grafiki i przypisania usług ---
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
                        context.EmployeeServices.Add(new EmployeeService { EmployeeId = employee.EmployeeId, ServiceId = service.ServiceId });
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

        // --- Rezerwacje ---
        Console.WriteLine("Rozpoczynanie generowania rezerwacji dla każdego klienta...");
        var reservations = new List<Reservation>();
        var allBusinesses = await context.Businesses
            .Include(b => b.Employees)
            .Include(b => b.Categories)
            .ThenInclude(c => c.Services)
            .ToListAsync();

        var currentYear = DateTime.UtcNow.Year;
        var startDate = new DateTime(currentYear, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(currentYear, 9, 30, 23, 59, 59, DateTimeKind.Utc);
        var simulationNow = DateTime.UtcNow;

        foreach (var customer in customers)
        {
            int reservationCount = faker.Random.Number(25, 35);

            for (int i = 0; i < reservationCount; i++)
            {
                var randomBusiness = faker.PickRandom(allBusinesses.Where(b => b.Categories.SelectMany(c => c.Services).Any()));
                if (randomBusiness == null) continue;

                var servicesInBusiness = randomBusiness.Categories.SelectMany(c => c.Services).ToList();
                var employeesInBusiness = randomBusiness.Employees.ToList();

                if (!servicesInBusiness.Any() || !employeesInBusiness.Any()) continue;

                var randomService = faker.PickRandom(servicesInBusiness);
                var randomEmployee = faker.PickRandom(employeesInBusiness);

                var startTime = faker.Date.Between(startDate, endDate);
                var status = startTime < simulationNow
                    ? faker.PickRandom(new[] { ReservationStatus.Completed, ReservationStatus.Completed, ReservationStatus.Cancelled })
                    : ReservationStatus.Confirmed;

                reservations.Add(new Reservation
                {
                    Customer = customer,
                    BusinessId = randomBusiness.BusinessId,
                    EmployeeId = randomEmployee.EmployeeId,
                    ServiceId = randomService.ServiceId,
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(randomService.DurationMinutes),
                    Status = status
                });
            }
        }
        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
        Console.WriteLine($"Stworzono {reservations.Count} rezerwacji.");

        // --- Opinie ---
        var reviews = new List<Review>();
        var completedReservations = await context.Reservations
            .Where(r => r.Status == ReservationStatus.Completed)
            .Include(r => r.Customer)
            .ToListAsync();

        Console.WriteLine($"Znaleziono {completedReservations.Count} rezerwacji do stworzenia opinii.");

        foreach (var reservation in faker.PickRandom(completedReservations, (int)(completedReservations.Count * 0.8)))
        {
            if (reservation.Customer == null) continue;
            var ratingProfileForThisBusiness = businessRatingProfiles[reservation.BusinessId];

            var review = new Faker<Review>("pl")
                .RuleFor(r => r.Rating, f => f.PickRandom(ratingProfileForThisBusiness))
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
    }
}