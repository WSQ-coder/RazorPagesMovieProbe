using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class ItemPurchase
{
    [Display(Name = "ID позиции заказа")]
    public int IdItemPurchase { get; set; }
    [Display(Name = "ID заказа")]
    public int IdPurchase { get; set; }
    [Display(Name = "Id товара")]
    public int IdProduct { get; set; }
    [Display(Name = "Количество в звказе")]
    public int QuantityInPurchase { get; set; }
    [Display(Name = "Цена заказа")]
    public decimal PriceInPurchase { get; set; }
    [Display(Name = "Статус заказа")]
    public string StatusItem { get; set; } = null!;
    [Display(Name = "Товар")]
    public virtual Product IdProductNavigation { get; set; } = null!;
    [Display(Name = "Заказ")]
    public virtual Purchase IdPurchaseNavigation { get; set; } = null!;
}
