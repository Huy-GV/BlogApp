using SimpleForum.Core.Data.Validation;
using System;

namespace SimpleForum.Core.Data.ViewModels;
public class BanUserViewModel
{
    [DateRange(allowsPast: false, allowsFuture: true, required: false, ErrorMessage = "Expiry date must be in the future")]
    public DateTime? NewBanTicketExpiryDate { get; set; } = null;
}
