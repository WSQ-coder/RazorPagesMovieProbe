using System;
using System.Collections.Generic;

namespace RazorPagesMovie.Models;

public partial class ProductionPurchase
{
    public int IdProductionPurchase { get; set; }

    public int IdSeller { get; set; }

    public int IdBuyer { get; set; }

    public bool DirectionFromSeller { get; set; }

    public string? TextAccounts { get; set; }

    public int? IdProduct { get; set; }

    public virtual Account IdBuyerNavigation { get; set; } = null!;

    public virtual Product? IdProductNavigation { get; set; }

    public virtual Account IdSellerNavigation { get; set; } = null!;
}
