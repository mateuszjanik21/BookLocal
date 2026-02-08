using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    public partial class AddProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PROC 1: GetDashboardStats
            var proc1 = @"
            CREATE OR ALTER PROCEDURE [dbo].[GetDashboardStats] @OwnerId NVARCHAR(450)
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @BusinessId INT;
                SELECT @BusinessId = BusinessId FROM Businesses WHERE OwnerId = @OwnerId;

                SELECT 
                    (SELECT COUNT(*) FROM Reservations r
                     INNER JOIN ServiceVariants sv ON r.ServiceVariantId = sv.ServiceVariantId
                     INNER JOIN Services s ON sv.ServiceId = s.ServiceId
                     INNER JOIN ServiceCategories sc ON s.ServiceCategoryId = sc.ServiceCategoryId
                     WHERE sc.BusinessId = @BusinessId 
                       AND r.StartTime >= GETDATE() 
                       AND r.Status = 'Confirmed') AS UpcomingReservationsCount,
        
                    (SELECT COUNT(*) FROM Employees WHERE BusinessId = @BusinessId AND IsArchived = 0) AS EmployeeCount,
        
                    (SELECT COUNT(*) FROM Services s
                     INNER JOIN ServiceCategories sc ON s.ServiceCategoryId = sc.ServiceCategoryId
                     WHERE sc.BusinessId = @BusinessId AND s.IsArchived = 0) AS ServiceCount,
        
                    (SELECT COUNT(DISTINCT COALESCE(r.CustomerId, r.GuestName)) FROM Reservations r
                     INNER JOIN ServiceVariants sv ON r.ServiceVariantId = sv.ServiceVariantId
                     INNER JOIN Services s ON sv.ServiceId = s.ServiceId
                     INNER JOIN ServiceCategories sc ON s.ServiceCategoryId = sc.ServiceCategoryId
                     WHERE sc.BusinessId = @BusinessId) AS ClientCount,
        
                    (SELECT CAST(CASE WHEN EXISTS (
                        SELECT 1 FROM ServiceVariants sv
                        INNER JOIN Services s ON sv.ServiceId = s.ServiceId
                        INNER JOIN ServiceCategories sc ON s.ServiceCategoryId = sc.ServiceCategoryId
                        WHERE sc.BusinessId = @BusinessId
                    ) THEN 1 ELSE 0 END AS BIT)) AS HasVariants
            END
            ";
            migrationBuilder.Sql(proc1);

            // PROC 2: GetFinanceAppointments
            var proc2 = @"
            CREATE OR ALTER PROCEDURE [dbo].[GetFinanceAppointments]
                @BusinessId INT,
                @StartDate DATE,
                @EndDate DATE
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT 
                    CAST(r.StartTime AS DATE) AS ReportDate,
                    COUNT(*) AS TotalAppointments,
                    SUM(CASE WHEN r.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedAppointments,
                    SUM(CASE WHEN r.Status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledAppointments,
                    SUM(CASE WHEN r.Status = 'NoShow' THEN 1 ELSE 0 END) AS NoShowCount
                FROM Reservations r
                WHERE r.BusinessId = @BusinessId
                  AND CAST(r.StartTime AS DATE) BETWEEN @StartDate AND @EndDate
                GROUP BY CAST(r.StartTime AS DATE)
                ORDER BY ReportDate DESC;
            END
            ";
            migrationBuilder.Sql(proc2);

            // PROC 3: GetFinanceCommission
            var proc3 = @"
            CREATE OR ALTER PROCEDURE [dbo].[GetFinanceCommission]
                @BusinessId INT,
                @StartDate DATE,
                @EndDate DATE
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @CommissionRate DECIMAL(5,2);
                DECLARE @TotalRevenue DECIMAL(10,2);
                
                SELECT TOP 1 @CommissionRate = ISNULL(sp.CommissionPercentage, 0)
                FROM BusinessSubscriptions bs
                INNER JOIN SubscriptionPlans sp ON sp.PlanId = bs.PlanId
                WHERE bs.BusinessId = @BusinessId
                  AND bs.StartDate <= CAST(@EndDate AS DATETIME)
                  AND bs.EndDate >= CAST(@StartDate AS DATETIME)
                  AND bs.IsActive = 1
                ORDER BY bs.SubscriptionId DESC;
                
                IF @CommissionRate IS NULL SET @CommissionRate = 0;
                
                SELECT @TotalRevenue = ISNULL(SUM(p.Amount), 0)
                FROM Payments p
                INNER JOIN Reservations r ON r.ReservationId = p.ReservationId
                WHERE r.BusinessId = @BusinessId
                  AND CAST(r.StartTime AS DATE) BETWEEN @StartDate AND @EndDate
                  AND r.Status = 'Completed' 
                  AND p.Status = 'Completed'; 
                  
                SELECT 
                    @TotalRevenue * (@CommissionRate / 100.0) AS TotalCommission,
                    @CommissionRate AS CommissionRate;
            END
            ";
            migrationBuilder.Sql(proc3);

            // PROC 4: GetFinanceNewCustomers
            var proc4 = @"
            CREATE OR ALTER PROCEDURE [dbo].[GetFinanceNewCustomers]
                @BusinessId INT,
                @StartDate DATE,
                @EndDate DATE
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @TotalCustomers INT;
                DECLARE @ReturningCustomers INT;
                
                SELECT @TotalCustomers = COUNT(DISTINCT r.CustomerId)
                FROM Reservations r
                WHERE r.BusinessId = @BusinessId
                  AND CAST(r.StartTime AS DATE) BETWEEN @StartDate AND @EndDate
                  AND r.Status = 'Completed'
                  AND r.CustomerId IS NOT NULL;
                  
                SELECT @ReturningCustomers = COUNT(DISTINCT r.CustomerId)
                FROM Reservations r
                WHERE r.BusinessId = @BusinessId
                  AND CAST(r.StartTime AS DATE) BETWEEN @StartDate AND @EndDate
                  AND r.Status = 'Completed'
                  AND r.CustomerId IS NOT NULL
                  AND r.CustomerId IN (
                      SELECT DISTINCT r2.CustomerId
                      FROM Reservations r2
                      WHERE r2.BusinessId = @BusinessId
                        AND r2.Status = 'Completed'
                        AND r2.StartTime < CAST(@StartDate AS DATETIME)
                        AND r2.CustomerId IS NOT NULL
                  );
                  
                SELECT 
                    @TotalCustomers - @ReturningCustomers AS NewCustomersCount,
                    @ReturningCustomers AS ReturningCustomersCount;
            END
            ";
            migrationBuilder.Sql(proc4);

            // PROC 5: GetFinanceReport
            var proc5 = @"
            CREATE OR ALTER PROCEDURE [dbo].[GetFinanceReport]
                @BusinessId INT,
                @StartDate DATE,
                @EndDate DATE
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @CommissionRate DECIMAL(5,2);
                SELECT TOP 1 @CommissionRate = ISNULL(sp.CommissionPercentage, 0)
                FROM BusinessSubscriptions bs
                INNER JOIN SubscriptionPlans sp ON sp.PlanId = bs.PlanId
                WHERE bs.BusinessId = @BusinessId
                  AND bs.StartDate <= CAST(@EndDate AS DATETIME)
                  AND bs.EndDate >= CAST(@StartDate AS DATETIME)
                  AND bs.IsActive = 1
                ORDER BY bs.SubscriptionId DESC;
                
                IF @CommissionRate IS NULL SET @CommissionRate = 0;
                
                SELECT 
                    CAST(r.StartTime AS DATE) AS ReportDate,
                    ISNULL(SUM(CASE WHEN p.Status = 'Completed' THEN p.Amount ELSE 0 END), 0) AS TotalRevenue,
                    ISNULL(SUM(CASE WHEN p.Status = 'Completed' AND TRY_CAST(p.PaymentMethod AS INT) = 0 THEN p.Amount ELSE 0 END), 0) AS CashRevenue,
                    ISNULL(SUM(CASE WHEN p.Status = 'Completed' AND TRY_CAST(p.PaymentMethod AS INT) = 1 THEN p.Amount ELSE 0 END), 0) AS CardRevenue,
                    ISNULL(SUM(CASE WHEN p.Status = 'Completed' AND TRY_CAST(p.PaymentMethod AS INT) = 2 THEN p.Amount ELSE 0 END), 0) AS OnlineRevenue,
                    
                    COUNT(DISTINCT r.ReservationId) AS TotalAppointments,
                    COUNT(DISTINCT CASE WHEN r.Status = 'Completed' THEN r.ReservationId END) AS CompletedAppointments,
                    COUNT(DISTINCT CASE WHEN r.Status = 'Cancelled' THEN r.ReservationId END) AS CancelledAppointments,
                    COUNT(DISTINCT CASE WHEN r.Status = 'NoShow' THEN r.ReservationId END) AS NoShowCount,
                    
                    ISNULL(SUM(CASE WHEN p.Status = 'Completed' THEN p.Amount ELSE 0 END), 0) * (@CommissionRate / 100.0) AS TotalCommission,
                    0 AS NewCustomersCount,
                    0 AS ReturningCustomersCount,
                    
                    CASE 
                        WHEN COUNT(DISTINCT CASE WHEN r.Status = 'Completed' THEN r.ReservationId END) > 0 
                        THEN ISNULL(SUM(CASE WHEN p.Status = 'Completed' THEN p.Amount ELSE 0 END), 0) / 
                             COUNT(DISTINCT CASE WHEN r.Status = 'Completed' THEN r.ReservationId END)
                        ELSE 0 
                    END AS AverageTicketValue,
        
                    CAST(NULL AS NVARCHAR(200)) AS TopSellingServiceName
                FROM Reservations r
                LEFT JOIN Payments p ON p.ReservationId = r.ReservationId
                WHERE r.BusinessId = @BusinessId
                  AND CAST(r.StartTime AS DATE) BETWEEN @StartDate AND @EndDate
                GROUP BY CAST(r.StartTime AS DATE)
                HAVING COUNT(*) > 0
                ORDER BY ReportDate DESC;
            END
            ";
            migrationBuilder.Sql(proc5);

            // PROC 6: GetLatestReviews (Bez zmian - tutaj nie ma statusu)
            var proc6 = @"
            CREATE OR ALTER PROCEDURE [dbo].[GetLatestReviews] @OwnerId NVARCHAR(450)
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @BusinessId INT;
                SELECT @BusinessId = BusinessId FROM Businesses WHERE OwnerId = @OwnerId;
                SELECT TOP 3
                    r.ReviewId, r.Rating, r.Comment,
                    COALESCE(u.FirstName, 'Anonim') AS ReviewerName, r.CreatedAt
                FROM Reviews r
                LEFT JOIN AspNetUsers u ON r.UserId = u.Id
                WHERE r.BusinessId = @BusinessId
                ORDER BY r.CreatedAt DESC
            END
            ";
            migrationBuilder.Sql(proc6);

            // PROC 7: GetOwnerCalendarEvents
            var proc7 = @"
            CREATE OR ALTER PROCEDURE [dbo].[GetOwnerCalendarEvents]
                @OwnerId NVARCHAR(450),
                @StartDate DATETIME,
                @EndDate DATETIME
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT 
                    r.ReservationId,
                    r.StartTime,
                    r.EndTime,
                    r.Status,
                    r.AgreedPrice,
                    r.PaymentMethod,
                    r.ServiceVariantId,
                    sv.Name AS VariantName,
                    s.Name AS ServiceName,
                    r.BusinessId,
                    b.Name AS BusinessName,
                    r.EmployeeId,
                    e.FirstName + ' ' + e.LastName AS EmployeeFullName,
                    r.CustomerId,
                    COALESCE(u.FirstName + ' ' + u.LastName, r.GuestName) AS CustomerFullName,
                    r.GuestName,
                    CAST(CASE WHEN rev.ReviewId IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS HasReview
                FROM Reservations r
                INNER JOIN Businesses b ON r.BusinessId = b.BusinessId
                INNER JOIN ServiceVariants sv ON r.ServiceVariantId = sv.ServiceVariantId
                INNER JOIN Services s ON sv.ServiceId = s.ServiceId
                INNER JOIN Employees e ON r.EmployeeId = e.EmployeeId
                LEFT JOIN AspNetUsers u ON r.CustomerId = u.Id
                LEFT JOIN Reviews rev ON r.ReservationId = rev.ReservationId
                WHERE b.OwnerId = @OwnerId
                  AND r.StartTime < @EndDate
                  AND r.EndTime > @StartDate
            END
            ";
            migrationBuilder.Sql(proc7);

            // PROC 8: GetTodaysReservations
            // TUTAJ BYŁ GŁÓWNY BŁĄD. Zmieniamy 'Status != 1' z powrotem na 'Status != ''Cancelled'''
            var proc8 = @"
            CREATE OR ALTER PROCEDURE [dbo].[GetTodaysReservations] @OwnerId NVARCHAR(450)
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @BusinessId INT, @TodayStart DATETIME, @TodayEnd DATETIME;
                SELECT @BusinessId = BusinessId FROM Businesses WHERE OwnerId = @OwnerId;
                SET @TodayStart = CAST(GETDATE() AS DATE);
                SET @TodayEnd = DATEADD(DAY, 1, @TodayStart);
                
                SELECT 
                    r.ReservationId, r.StartTime, r.EndTime, r.Status, r.AgreedPrice, r.PaymentMethod,
                    r.ServiceVariantId, sv.Name AS VariantName, s.Name AS ServiceName,
                    r.BusinessId, b.Name AS BusinessName, r.EmployeeId,
                    e.FirstName + ' ' + e.LastName AS EmployeeFullName,
                    r.CustomerId,
                    COALESCE(u.FirstName + ' ' + u.LastName, r.GuestName) AS CustomerFullName,
                    r.GuestName, s.IsArchived AS IsServiceArchived,
                    CAST(CASE WHEN rev.ReviewId IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS HasReview
                FROM Reservations r
                INNER JOIN Businesses b ON r.BusinessId = b.BusinessId
                INNER JOIN ServiceVariants sv ON r.ServiceVariantId = sv.ServiceVariantId
                INNER JOIN Services s ON sv.ServiceId = s.ServiceId
                INNER JOIN Employees e ON r.EmployeeId = e.EmployeeId
                LEFT JOIN AspNetUsers u ON r.CustomerId = u.Id
                LEFT JOIN Reviews rev ON r.ReservationId = rev.ReservationId
                WHERE b.BusinessId = @BusinessId
                  AND r.StartTime >= @TodayStart AND r.StartTime < @TodayEnd
                  AND r.Status != 'Cancelled'
                ORDER BY r.StartTime
            END
            ";
            migrationBuilder.Sql(proc8);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetDashboardStats]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetFinanceAppointments]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetFinanceCommission]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetFinanceNewCustomers]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetFinanceReport]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetLatestReviews]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetOwnerCalendarEvents]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[GetTodaysReservations]");
        }
    }
}