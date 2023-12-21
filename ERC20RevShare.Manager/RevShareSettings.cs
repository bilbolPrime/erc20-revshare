namespace BilbolStack.Erc20Snapshot.Chain
{
    public class RevShareSettings
    {
        public const string ConfigKey = "RevShareInfo";
        public string AddressToIgnore { get; set; }
        public List<string>  AddressesOfSale { get; set; }
    }
}
