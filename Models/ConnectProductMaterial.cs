using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class ConnectProductMaterial
{
    [Display(Name = "ID связи")]
    public int Id { get; set; }
    [Display(Name = "ID товара")]
    public int IdProduct { get; set; }
    [Display(Name = "ID материала")]
    public int IdMaterial { get; set; }
    [Display(Name = "Вес/Количество")]
    public decimal Quantity { get; set; }
    [Display(Name = "Ед. измерения")]
    public string Unit { get; set; } = null!;
    [Display(Name = "Материла")]
    public virtual CatalogForMaterial IdMaterialNavigation { get; set; } = null!;
    [Display(Name = "Товар")]
    public virtual Product IdProductNavigation { get; set; } = null!;
}
