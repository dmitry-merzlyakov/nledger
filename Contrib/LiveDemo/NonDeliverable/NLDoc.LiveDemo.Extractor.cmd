@REM This script updates the content of Live Demo files (NLDoc.LiveDemo.config.xml, NLDoc.LiveDemo.ledger3.html, /test/nledger/sandbox/)
@REM in case you update ledger3.texi and/or ledger3.html
@PowerShell -NoLogo -ExecutionPolicy RemoteSigned -File %~dp0NLDoc.LiveDemo.Extractor.ps1