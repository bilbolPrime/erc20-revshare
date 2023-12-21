# ERC20 RevShare

A small application which calculates revshare from files created by ERC20Snapshot. The application can then distribute that revshare in erc20 tokens or eth.

# App Settings Setup

1. Update ChainInfo data to match local chain or remote chain
1. Update RevShareInfo data

## API 

1. `~/revshare/Calculate` takes 4 parameters. `revShareInWei` is the rev share that will be distributed. `thresholdInWei` is the minimum amount of tokens a wallet must hold to qualify. `banSalesAfterBlock`, if set to any value, would disqualify any wallet selling any of the erc20 token on or after the block was mined. `banSalesAfterTransfer` when set to true would disqualify wallets that sent to another wallet and then had those wallets sell.
This api requires `balances.csv` and `transfers.csv` to be placed inside of the root directory of the API. These files are generated from `ERC20Snapshot`.
After the calculation is done, the data is saved to `revshare.csv`.

```
{
  "revShareInWei": "100000000000000000000",
  "thresholdInWei": "500000000000000000000000",
  "banSalesAfterBlock": 18793577,
  "banSalesAfterTransfer": true
}
```

2. `~/revshare/Send` takes a query param `inEth`. The rev share is distributed in eth should that be true. If not, it is distributed in the configured erc20 token. The api takes the `revshare.csv` created by `Calculate` and creates a new file `revshare_processing.json` which holds the qualifying wallets, their expected rev share, the distribution transaction per wallet and if it were succesful. This is tracked in the console when the api is called.



# Useful Links

You will need [ERC20Snapshot](https://github.com/bilbolPrime/erc20-snapshot)

# Version History

1. 2023-12-21 : Initial release v1.0.0 

# Disclaimer

This implementation was made for educational / training purposes only.

# License

License is [MIT](https://en.wikipedia.org/wiki/MIT_License)

# MISC

Birbia is coming
