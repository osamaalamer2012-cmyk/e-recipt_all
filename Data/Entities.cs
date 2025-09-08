using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EReceiptAllInOne.Data;

[Table("Receipts")]
public class ReceiptEntity
{
    [Key]
    [MaxLength(64)]
    public string ReceiptId { get; set; } = default!;

    [MaxLength(64)]
    public string TxnId { get; set; } = default!;

    [MaxLength(32)]
    public string Msisdn { get; set; } = default!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(8)]
    public string Currency { get; set; } = "USD";

    public string ItemsJson { get; set; } = "[]";

    public DateTimeOffset ExpiresAt { get; set; }

    public int MaxUses { get; set; } = 2;

    public int Uses { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; }
}

[Table("ShortLinks")]
public class ShortLinkEntity
{
    [Key]
    [MaxLength(32)]
    public string Code { get; set; } = default!;

    public string LongUrl { get; set; } = default!;

    public int Usage { get; set; }

    public int UsageMax { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
