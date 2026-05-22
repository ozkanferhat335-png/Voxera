using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public InvoiceType Type { get; private set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public DateTime IssuedAt { get; private set; }
    public DateTime DueAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxRate { get; private set; } = 0.20m;
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public string? Notes { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? PaymentReference { get; private set; }
    public string? PdfPath { get; private set; }
    public SubscriptionPlan? Plan { get; private set; }
    public int? PeriodMonths { get; private set; }
    public DateTime? PeriodStart { get; private set; }
    public DateTime? PeriodEnd { get; private set; }

    // Navigation
    public Company? Company { get; private set; }
    public ICollection<InvoiceItem> Items { get; private set; } = new List<InvoiceItem>();

    protected Invoice() { }

    public static Invoice Create(Guid companyId, string invoiceNumber, InvoiceType type, DateTime dueAt)
    {
        return new Invoice
        {
            CompanyId = companyId,
            InvoiceNumber = invoiceNumber,
            Type = type,
            IssuedAt = DateTime.UtcNow,
            DueAt = dueAt
        };
    }

    public void Calculate()
    {
        SubTotal = Items.Sum(i => i.Total);
        TaxAmount = SubTotal * TaxRate;
        Total = SubTotal + TaxAmount;
        MarkAsUpdated();
    }

    public void MarkPaid(string method, string reference)
    {
        Status = InvoiceStatus.Paid;
        PaidAt = DateTime.UtcNow;
        PaymentMethod = method;
        PaymentReference = reference;
        MarkAsUpdated();
    }
}

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total => Quantity * UnitPrice;

    public Invoice? Invoice { get; private set; }

    protected InvoiceItem() { }

    public static InvoiceItem Create(Guid invoiceId, string description, decimal quantity, decimal unitPrice)
    {
        return new InvoiceItem { InvoiceId = invoiceId, Description = description, Quantity = quantity, UnitPrice = unitPrice };
    }
}
