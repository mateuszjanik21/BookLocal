namespace BookLocal.API.DTOs
{
    public class LoyaltyConfigDto
    {
        public bool IsActive { get; set; }
        public decimal SpendAmountForOnePoint { get; set; }
    }

    public class LoyaltyBalanceDto
    {
        public int PointsBalance { get; set; }
        public int TotalPointsEarned { get; set; }
    }

    public class LoyaltyTransactionDto
    {
        public int TransactionId { get; set; }
        public int PointsAmount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
