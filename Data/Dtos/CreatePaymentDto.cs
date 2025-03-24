namespace MyAppApi.Data.Dtos
{
    public class CreatePaymentDto
    {
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentType PaymentType { get; set; }
        public string PaymentMethod { get; set; } // إضافة PaymentMethod
        public string Reference { get; set; } // إضافة Reference
        public string Note { get; set; } // إضافة Note
        public int BookingId { get; set; }
        public string UserName { get; set; }
        public int RepairId { get; set; } // إضافة RepairId
    }

}

