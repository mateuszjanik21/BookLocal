using Bogus;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                new MainCategory { Name = "Fryzjer" },
                new MainCategory { Name = "Barber" },
                new MainCategory { Name = "Masaż" },
                new MainCategory { Name = "Fizjoterapia" },
                new MainCategory { Name = "Psycholog" },
                new MainCategory { Name = "Pilates" },
                new MainCategory { Name = "Joga" },
                new MainCategory { Name = "Kosmetyczka" },
                new MainCategory { Name = "Groomer" },
                new MainCategory { Name = "Tatuaż" },
                new MainCategory { Name = "Medycyna Estetyczna" },
                new MainCategory { Name = "Trening Personalny" }
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
        for (int i = 0; i < 39; i++)
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
        for (int i = 0; i < 29; i++)
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
        var allMainCategories = await context.MainCategories.ToListAsync();
        var businesses = new List<Business>();

        foreach (var owner in owners)
        {
            var business = new Faker<Business>("pl").RuleFor(b => b.Name, f => f.Company.CompanyName()).RuleFor(b => b.Description, f => f.Lorem.Sentence(25)).RuleFor(b => b.City, f => f.Address.City()).RuleFor(b => b.Address, f => f.Address.StreetAddress()).RuleFor(b => b.NIP, f => f.Random.Replace("##########")).RuleFor(b => b.PhotoUrl, f => f.Image.PicsumUrl(640, 480)).Generate();
            business.Owner = owner;

            var businessCategories = new List<ServiceCategory>();
            var pickedMainCategories = faker.PickRandom(allMainCategories, faker.Random.Number(1, 3)).ToList();

            foreach (var mainCat in pickedMainCategories)
            {
                var subCategories = new Faker<ServiceCategory>("pl")
                    .RuleFor(sc => sc.Name, f => f.Commerce.ProductName())
                    .RuleFor(sc => sc.PhotoUrl, f => f.Image.PicsumUrl(200, 200))
                    .Generate(faker.Random.Number(1, 3));

                foreach (var subCategory in subCategories)
                {
                    subCategory.Business = business;
                    subCategory.MainCategory = mainCat;

                    var services = new Faker<Service>("pl").RuleFor(s => s.Name, f => f.Commerce.ProductName()).RuleFor(s => s.Price, f => Math.Round(f.Random.Decimal(50, 450) / 5) * 5).RuleFor(s => s.DurationMinutes, f => f.Random.ListItem(new List<int> { 30, 45, 60, 90, 120 })).Generate(faker.Random.Number(3, 8));
                    foreach (var service in services) { service.Business = business; service.ServiceCategory = subCategory; }
                    subCategory.Services = services;

                    businessCategories.Add(subCategory);
                }
            }
            business.Categories = businessCategories;

            var employees = new Faker<Employee>("pl").RuleFor(e => e.FirstName, f => f.Name.FirstName()).RuleFor(e => e.LastName, f => f.Name.LastName()).RuleFor(e => e.Position, f => f.Name.JobTitle()).RuleFor(e => e.PhotoUrl, f => f.Internet.Avatar()).Generate(faker.Random.Number(4, 8));
            foreach (var employee in employees) { employee.Business = business; }
            employees.Add(new Employee { FirstName = owner.FirstName, LastName = owner.LastName, Position = "Właściciel", PhotoUrl = owner.PhotoUrl, Business = business });
            business.Employees = employees;

            businesses.Add(business);
        }
        await context.Businesses.AddRangeAsync(businesses);
        await context.SaveChangesAsync();

        // --- Grafiki i przypisania usług ---
        foreach (var business in businesses)
        {
            var allServicesInBusiness = business.Categories.SelectMany(c => c.Services).ToList();
            foreach (var employee in business.Employees)
            {
                if (allServicesInBusiness.Any())
                {
                    int maxAmountToPick = Math.Min(5, allServicesInBusiness.Count);
                    int amountToPick = faker.Random.Number(1, maxAmountToPick);

                    var servicesForEmployee = faker.PickRandom(allServicesInBusiness, amountToPick);
                    foreach (var service in servicesForEmployee)
                    {
                        context.EmployeeServices.Add(new EmployeeService { EmployeeId = employee.EmployeeId, ServiceId = service.ServiceId });
                    }
                }

                var dayOff = faker.PickRandom<DayOfWeek>();
                for (int i = 0; i < 7; i++) { var day = (DayOfWeek)i; var isDayOff = day == dayOff; context.WorkSchedules.Add(new WorkSchedule { EmployeeId = employee.EmployeeId, DayOfWeek = day, IsDayOff = isDayOff, StartTime = isDayOff ? null : new TimeSpan(faker.Random.ListItem(new List<int> { 8, 9, 10 }), 0, 0), EndTime = isDayOff ? null : new TimeSpan(faker.Random.ListItem(new List<int> { 16, 17, 18, 19 }), 0, 0) }); }
            }
        }
        await context.SaveChangesAsync();

        // --- Rezerwacje ---
        Console.WriteLine("Rozpoczynanie generowania dużej liczby rezerwacji...");
        var reservations = new List<Reservation>();
        var startDate = new DateTime(2025, 8, 4, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 8, 30, 23, 59, 59, DateTimeKind.Utc);
        var simulationNow = new DateTime(2025, 8, 12, 0, 0, 0, DateTimeKind.Utc);

        foreach (var business in businesses)
        {
            var employeesInBusiness = business.Employees.ToList();
            if (!employeesInBusiness.Any()) continue;
            foreach (var category in business.Categories)
            {
                var servicesInCategory = category.Services.ToList();
                if (!servicesInCategory.Any()) continue;
                for (int i = 0; i < 60; i++)
                {
                    var randomCustomer = faker.PickRandom(customers);
                    var randomService = faker.PickRandom(servicesInCategory);
                    var randomEmployee = faker.PickRandom(employeesInBusiness);
                    var startTime = faker.Date.Between(startDate, endDate);
                    var status = startTime < simulationNow ? faker.PickRandom(new[] { ReservationStatus.Completed, ReservationStatus.Completed, ReservationStatus.Cancelled }) : ReservationStatus.Confirmed;
                    reservations.Add(new Reservation { Customer = randomCustomer, BusinessId = business.BusinessId, EmployeeId = randomEmployee.EmployeeId, ServiceId = randomService.ServiceId, StartTime = startTime, EndTime = startTime.AddMinutes(randomService.DurationMinutes), Status = status });
                }
            }
        }
        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
        Console.WriteLine($"Stworzono {reservations.Count} rezerwacji.");

        // --- Opinie ---
        var reviews = new List<Review>();
        var completedReservations = await context.Reservations.Where(r => r.Status == ReservationStatus.Completed).Include(r => r.Customer).ToListAsync();
        Console.WriteLine($"Znaleziono {completedReservations.Count} rezerwacji do stworzenia opinii.");
        foreach (var reservation in faker.PickRandom(completedReservations, (int)(completedReservations.Count * 0.8)))
        {
            if (reservation.Customer == null) continue;
            var review = new Faker<Review>("pl").RuleFor(r => r.Rating, f => f.Random.Int(3, 5)).RuleFor(r => r.Comment, f => f.Rant.Review()).RuleFor(r => r.CreatedAt, f => reservation.EndTime.AddDays(f.Random.Int(1, 14))).RuleFor(r => r.ReviewerName, f => $"{reservation.Customer.FirstName} {reservation.Customer.LastName}").RuleFor(r => r.BusinessId, f => reservation.BusinessId).RuleFor(r => r.UserId, f => reservation.CustomerId).RuleFor(r => r.ReservationId, f => reservation.ReservationId).Generate();
            reviews.Add(review);
        }
        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();
        Console.WriteLine($"Stworzono {reviews.Count} opinii.");
    }
}