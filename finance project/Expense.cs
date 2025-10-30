using System;

namespace finance_project
{
    public class Expense
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.Today;
        public string Category { get; set; } = "General";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
    }
}
