using System.ComponentModel.DataAnnotations;

namespace ERC20RevShare
{
    public class CalculateRevShare
    {
        [Required]
        public string? RevShareInWei { get; set; }
        [Required]
        public string? ThresholdInWei { get; set; }
        public long? BanSalesAfterBlock { get; set; }
        public bool? BanSalesAfterTransfer { get; set; }
    }
}
