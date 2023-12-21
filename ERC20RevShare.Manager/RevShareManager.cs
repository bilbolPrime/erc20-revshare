using BilbolStack.ERC20RevShare.Chain;
using BilbolStack.Erc20Snapshot.Chain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Numerics;
using System.Text;

namespace BilbolStack.ERC20RevShare.Manager
{
    public class RevShareManager : IRevShareManager
    {
        private readonly string _balanceFile = "balances.csv";
        private readonly string _transfers = "transfers.csv";
        private readonly string _revShare = "revshare.csv";
        private readonly string _revShareProcessing = "revshare_processing.json";
        private static object _lock = new object();

        private RevShareSettings _revShareSettings;
        private IERC20Contract _eRC20Contract;
        public RevShareManager(IOptions<RevShareSettings> revShareSettings, IERC20Contract eRC20Contract)
        {
            _revShareSettings = revShareSettings.Value;
            _eRC20Contract = eRC20Contract;
        }

        public void CalculateRevShare(BigInteger totalRevShare, BigInteger threshold, long? banSaleAfterBlock, bool? banSaleAfterTransfer)
        {
            lock (_lock)
            {
                var addressOfSale = _revShareSettings.AddressesOfSale.Select(i => i.ToLower());
                var addressToIgnore = _revShareSettings.AddressToIgnore.ToLower();
                var transfers = File.ReadAllLines(_transfers).Skip(1).Select(i => new ERC20Transfer(i)).ToList();
                var balances = File.ReadAllLines(_balanceFile).Skip(1).Select(i => new ERC20Balance(i)).ToList();
                var revShare = balances.Select(i => new RevShare() { Wallet = i.Owner, TokensInWei = i.Amount }).ToList();
                Dictionary<string, ERC20Transfer> bustedAt = new Dictionary<string, ERC20Transfer>();
                Dictionary<string, string> badTX = new Dictionary<string, string>();
                HashSet<string> badWallets = new HashSet<string>();
                if (banSaleAfterBlock.HasValue)
                {
                    var transfersInEffect = transfers.Where(i => i.BlockNumber >= banSaleAfterBlock.Value).ToList();
                    var allSales = transfersInEffect.Where(i => addressOfSale.Contains(i.To.ToLower())).ToList();
                    bustedAt = allSales.GroupBy(i => i.From)
                                .Select(i => i.OrderBy(j => j.BlockNumber))
                                .ToDictionary(i => i.First().From.ToLower(), j => j.First());
                    var busted = new List<ERC20Transfer>();

                    allSales.ForEach(i => badWallets.Add(i.From.ToLower()));
                    addressOfSale.ToList().ForEach(i => badWallets.Remove(i));
                    badWallets.Remove(addressToIgnore);

                    if (banSaleAfterTransfer.HasValue && banSaleAfterTransfer.Value)
                    {
                        var bustedBadWallets = new HashSet<string>();

                        var badTx = transfersInEffect
                                    .Where(i => i.To.ToLower() == addressToIgnore.ToLower() || i.From.ToLower() == addressToIgnore.ToLower()
                                     || addressOfSale.Contains(i.From.ToLower())).Select(i => i.TX).ToHashSet();
                        do
                        {
                            bustedBadWallets = new HashSet<string>();
                            transfersInEffect
                                    .Where(i => !badTx.Contains(i.TX))
                                    .Where(i => badWallets.Contains(i.To.ToLower()))
                                    .ToList()
                                    .ForEach(i =>
                                    {
                                        bustedBadWallets.Add(i.From.ToLower());
                                        badTx.Add(i.TX);
                                        if (!bustedAt.ContainsKey(i.From.ToLower()))
                                        {
                                            bustedAt[i.From.ToLower()] = i;
                                        }
                                        else if (bustedAt[i.From.ToLower()].From != i.From.ToLower()
                                                    && bustedAt[i.From.ToLower()].BlockNumber > i.BlockNumber)
                                        {
                                            bustedAt[i.From.ToLower()] = i;
                                        }
                                    });

                            addressOfSale.ToList().ForEach(i => bustedBadWallets.Remove(i));
                            bustedBadWallets.Remove(addressToIgnore);
                            bustedBadWallets.ToList().ForEach(i => badWallets.Add(i));
                        } while (bustedBadWallets.Any());
                    }


                    foreach (var kvp in bustedAt)
                    {
                        var bingo = revShare.FirstOrDefault(i => i.Wallet.ToLower() == kvp.Key.ToLower());
                        bingo.BannedFromBlock = kvp.Value.BlockNumber;
                        bingo.BannedFromTransaction = kvp.Value.TX;
                        bingo.BannedFromWallet = addressOfSale.Contains(kvp.Value.To.ToLower()) ? kvp.Value.From : kvp.Value.To;
                    }
                }

                BigInteger totalTokens = 0;
                revShare.Where(i => i.BannedFromBlock == 0 && i.TokensInWei >= threshold && !addressOfSale.Contains(i.Wallet.ToLower()) && i.Wallet.ToLower() != addressToIgnore.ToLower()).ToList().ForEach(j => totalTokens = BigInteger.Add(totalTokens, j.TokensInWei));

                foreach (var rs in revShare.Where(i => i.BannedFromBlock == 0 && i.TokensInWei >= threshold && !addressOfSale.Contains(i.Wallet.ToLower()) && i.Wallet.ToLower() != addressToIgnore.ToLower()))
                {
                    rs.ExpectedRevShareInWei = BigInteger.Divide(BigInteger.Multiply(totalRevShare, rs.TokensInWei), totalTokens);
                }

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("wallet,tokens,tokensEth,bannedFromBlock,bannedFromWallet,bannedFromTransaction,revshare");

                foreach (var rs in revShare)
                {
                    stringBuilder.AppendLine($"{rs.Wallet},{rs.TokensInWei},{BigInteger.Divide(rs.TokensInWei, new BigInteger(1000000000000000000))},{rs.BannedFromBlock},{rs.BannedFromWallet},{rs.BannedFromTransaction},{rs.ExpectedRevShareInWei}");
                }

                File.WriteAllText(_revShare, stringBuilder.ToString());
            }
        }

        
        public void SendRevShare(bool inEth)
        {
            lock (_lock)
            {

                if (File.Exists(_revShareProcessing))
                {
                    Console.WriteLine($"Resuming {_revShareProcessing}");
                    SendRevShare(inEth, _revShareProcessing).Wait();
                    return;
                }

                if (!File.Exists(_revShare))
                {
                    throw new Exception($"{_revShare} not found");
                }


                var data = File.ReadAllLines(_revShare).Skip(1).ToList();
                var revShareData = data
                                    .Select(i => new RevShareDistribution() { Wallet = i.Split(',')[0], RevShare = BigInteger.Parse(i.Split(',')[6]) })
                                    .ToList();
                File.WriteAllText(_revShareProcessing, JsonConvert.SerializeObject(revShareData.Where(i => i.RevShare > 0)));
                SendRevShare(inEth, _revShareProcessing).Wait();
            }
        }

        async Task SendRevShare(bool inEth, string file)
        {
            var data = JsonConvert.DeserializeObject<List<RevShareDistribution>>(File.ReadAllText(_revShareProcessing));
            Console.WriteLine($"Processing {file} with {data.Where(i => string.IsNullOrEmpty(i.RevShareTransaction)).Count()} rev share wallets");
            foreach (RevShareDistribution d in data.Where(i => string.IsNullOrEmpty(i.RevShareTransaction)))
            {
                Console.WriteLine($"Processing {d.Wallet} for {d.RevShare} wei, inEth {inEth}");
                var tx = inEth ? await _eRC20Contract.SendGas(d.Wallet, ((decimal) BigInteger.Divide(d.RevShare, new BigInteger(100000000000000))) / 10000m) : await _eRC20Contract.Transfer(d.Wallet, d.RevShare);
                d.RevShareTransaction = tx.TX;
                File.WriteAllText(file, JsonConvert.SerializeObject(data));
                Console.WriteLine($"Processing {d.Wallet} for {d.RevShare} wei, inEth {inEth}; TX is {d.RevShareTransaction}, validating");
                await _eRC20Contract.ValidateTransaction(tx.TX);
                d.Success = true;
                File.WriteAllText(file, JsonConvert.SerializeObject(data));
                Console.WriteLine($"Processing {d.Wallet} for {d.RevShare} wei, inEth {inEth}; TX is {d.RevShareTransaction}, validated");
            }

            Console.WriteLine("Finished");
        }

        public class ERC20Transfer
        {
            public long BlockNumber { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public BigInteger Amount { get; set; }
            public string TX { get; set; }

            public ERC20Transfer(string data)
            {
                var dataSplit = data.Split(',');
                BlockNumber = long.Parse(dataSplit[0]);
                From = dataSplit[1];
                To = dataSplit[2];
                Amount = BigInteger.Parse(dataSplit[3]);
                TX = dataSplit[4];
            }
        }

        public class ERC20Balance
        {
            public string Owner { get; set; }
            public BigInteger Amount { get; set; }

            public ERC20Balance(string data)
            {
                var dataSplit = data.Split(',');
                Owner = dataSplit[0];
                Amount = BigInteger.Parse(dataSplit[1]);
            }
        }

        public class Busted
        {
            public string Owner { get; set; }
            public string Other { get; set; }
            public long BlockNumber { get; set; }
            public string TX { get; set; }
        }
    }
}
