@rem Use this batch file in case you need to build MSI by Wix commands (not by means of MS BUILD)
@rem The variable PKG should point at an NLedger package folder (you can build it by "package" command in NLedger build console)
@rem The variable WX should point at Wix binaries

set wx="C:\Program Files (x86)\WiX Toolset v3.10\bin\"
set PKG=D:\DEV\DMVS1\NLedger\Build-20180730-140833\package\

del *.wixobj
del *.wixpdb
del *.msi

%wx%candle Product.wxs Components.wxs -ext WixNetFxExtension -dPackageFolder=%PKG%
%wx%light Product.wixobj Components.wixobj -o NLedgerDev.msi -ext WixUIExtension -ext WixNetFxExtension -ext WixUtilExtension -loc Product_en-us.wxl

