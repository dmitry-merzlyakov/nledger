# Example of a quote downloading script for NLedger.
# There are two available functions: "get stock quotes" (that provides the same functionality as getquote.pl) 
# and "currency conversion". See further information to function descriptions.
#
# If you do not want to enable PS scripts globally on your machine,
# execute the following command in PS console:
# set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)][string]$symbol,
    [Parameter(Mandatory=$False)][AllowEmptyString()][string]$exchangeSymbol,
    [Switch][bool]$getQuote = $False,
    [Switch][bool]$convertCurrency = $False
)

trap 
{ 
  write-error $_ 
  exit 1 
} 


<#
.SYNOPSIS
    Currency Converter based on GeoPlugin web service (http://www.geoplugin.com/)
.DESCRIPTION
    Converts a specifies amount of money from one currency to another
    by means of GeoPlugin Currency Calculator service.
    GeoPlugin supports a big range of currencies and actual rates with free usage limit
    120 requests per minute (see geoPlugin Acceptable Use Policy http://www.geoplugin.com/aup).
.PARAMETER currencyFrom
    The currency that you need to convert from. Must be a valid three-letter currency ISO code.
.PARAMETER currencyTo
    The currency that you need to convert to. Must be a valid three-letter currency ISO code.
.PARAMETER amount
    Amount of money you want to convert. Default value is "1"
.EXAMPLE
    C:\PS>gpConvertCurrency -currencyFrom "USD" -currencyTo "CAD"
    1.25
    Converts 1 United States dollar to Canadian dollars
.EXAMPLE
    C:\PS>gpConvertCurrency "USD" "CAD"
    1.25
    Short form of the same conversion
.EXAMPLE
    C:\PS> gpConvertCurrency -currencyFrom "CZK" -currencyTo "EUR" -amount 100
    3.91
    Converts 100 Czech crowns to Euros
.LINK
    http://www.geoplugin.com/
.NOTES
    Author: Dmitry Merzlyakov
    Date:   January 10, 2018
    Based on geoPlugin web service (http://www.geoplugin.com/)
    -
    If you do not want to enable PS scripts globally on your machine,
    execute the following command in PS console:
    set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
#>    
function gpConvertCurrency {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)] [string]$currencyFrom,
    [Parameter(Mandatory=$True)] [string]$currencyTo,
    [Parameter(Mandatory=$False)] [decimal]$amount = 1
  )

  [int]$private:jsonCallbackReq = Get-Random
  [int]$private:jsonCallbackRes = Get-Random

  $private:headers = @{ "Referer"="https://github.com/dmitry-merzlyakov/nledger" }

  [string]$private:url = "http://www.geoplugin.net/currency_converter.gp?jsoncallback=jsonp$private:jsonCallbackReq&_=$private:jsonCallbackRes&from=$($currencyFrom.ToUpper())&to=$($currencyTo.ToUpper())&amount=$amount"
  Write-Verbose "Getting url content: $private:url"
  $private:result = Invoke-WebRequest $private:url -UseBasicParsing -Headers $private:headers

  if ($private:result.StatusCode -eq 200) {
    Write-Verbose "Response content is: $($private:result.Content)"
    if ($private:result.Content -match "jsonp$private:jsonCallbackReq\(([^\)]+)\)") {
       $private:response = $Matches[1] | ConvertFrom-Json
       $private:rate = $private:response.to.amount
       Write-Verbose "Found rate is: $private:rate"
       return $private:rate
    } else {
       throw "Not expected response content; cannot parse json"
    }
  } else {
    throw "Response status code not OK: $($private:result.StatusCode)"
  }
}


<#
.SYNOPSIS
    Returns a stock quote by a ticker symbol
.DESCRIPTION
    Executes a query to Yahoo Finance services
    to get an actual stock quote for a stock specified by a ticker.
.PARAMETER stock
    The ticket symbol for a stock.
.EXAMPLE
    C:\PS> yahooGetStockQuote "ibm"
    164.18
    Get a stock quote for IBM.
.LINK
    http://boards.fool.com/MessagePrint.aspx?mid=32889925
.NOTES
    IMPORTANT: Yahoo Queries are not affected by discontinued Download
    Finance Yahoo services and still function after Nov 2017.
    -
    Author: Dmitry Merzlyakov
    Date:   January 10, 2018
    -
    If you do not want to enable PS scripts globally on your machine,
    execute the following command in PS console:
    set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process
#>
function yahooGetStockQuote {
  [CmdletBinding()]
  Param(
    [Parameter(Mandatory=$True)] [string]$stock
  )

  Write-Verbose "Getting quote for: $stock"
  [string]$private:url = "https://query1.finance.yahoo.com/v7/finance/quote?formatted=false&symbols=$stock"
  Write-Verbose "Getting url content: $private:url"
  $private:result = Invoke-WebRequest $private:url -UseBasicParsing

  if ($private:result.StatusCode -eq 200) {
    Write-Verbose "Response content is: $($private:result.Content)"
    $private:response = $private:result.Content | ConvertFrom-Json
    if ($private:response) {
        if (!($private:response.quoteResponse.error)) {
            $private:quote = $private:response.quoteResponse.result.regularMarketPrice
            Write-Verbose "Found quote is: $private:quote"
            return $private:quote
        } else {
            throw "Returned ERROR: $private:response.quoteResponse.error"
        }
    } else {
        throw "ERROR: cannot parse json content"
    }
  } else {
    throw "Response status code not OK: $($private:result.StatusCode)"
  }
}

# Generate output

[string]$date = (get-date -f "yyyy/MM/dd HH:mm:ss")
if ($getQuote) { Write-Output "$date $symbol `$$(yahooGetStockQuote $symbol)" }
if ($convertCurrency) { Write-Output "$date $exchangeSymbol `$$(gpConvertCurrency $symbol $exchangeSymbol)" }
