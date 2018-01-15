@rem this is an entry point to call a quote downloading function. By default, it calls a powershell script that returns a stock quote.
@rem You can try a function to convert currencies according to the notes below or completely customize the batch file and/or the powershell script.
@rem
@rem If you would like to get quotes, uncomment (remove @rem) from the next line (that ends up with -getQuote) and comment out the latter (-convertCurrency)
@rem If you would like to convert currencies, uncomment the latter (that ends up with -convertCurrency) and comment out the next line (-getQuote)
@rem Remember that only one line must be uncommented.
@rem
@PowerShell -NoLogo -NonInteractive -ExecutionPolicy RemoteSigned -File %~dp0getQuote.ps1 %1 -getQuote
@rem PowerShell -NoLogo -NonInteractive -ExecutionPolicy RemoteSigned -File %~dp0getQuote.ps1 %1 %2 -convertCurrency
@rem
@exit %errorlevel%