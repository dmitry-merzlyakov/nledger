# Tests for NLedger Setup module (NLSetup.ps1)
# Based on Pester unit testing framework (https://github.com/pester/Pester)
# Pestel 4 or higher needs to be installed by the command: Install-Module -Name Pester -Force -SkipPublisherCheck
# If you need to enable execution of PS files - execute: set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

[string]$Script:ScriptPath = Split-Path $MyInvocation.MyCommand.Path
Import-Module $Script:ScriptPath\..\NLSetup.psm1 -Force

describe 'UpdateConfigXml' {

    it 'should check xml structure' {
        { UpdateConfigXml -xml $null } | Should Throw
        { UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8" ?><unknown></unknown>') } | Should Throw
        { UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8" ?><configuration></configuration>') } | Should Not Throw
    }

    it 'should add appSettings' {
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8" ?><configuration></configuration>')).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings /></configuration>'
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8"?><configuration><appSettings /></configuration>')).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings /></configuration>'
    }

    it 'should add element' {
        $addOrUpdate = @{ "key1"="value1" }
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8" ?><configuration></configuration>') -addOrUpdate $addOrUpdate).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /></appSettings></configuration>'
        $addOrUpdate = @{ "key1"="value1" }
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8"?><configuration><appSettings /></configuration>') -addOrUpdate $addOrUpdate).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /></appSettings></configuration>'
        $addOrUpdate = @{ "key2"="value2" }
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /></appSettings></configuration>') -addOrUpdate $addOrUpdate).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /><add key="key2" value="value2" /></appSettings></configuration>'
    }

    it 'should update element' {
        $addOrUpdate = @{ "key1"="value2" }
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /></appSettings></configuration>') -addOrUpdate $addOrUpdate).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value2" /></appSettings></configuration>'
        $addOrUpdate = @{ "key2"="value3" }
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /><add key="key2" value="value2" /></appSettings></configuration>') -addOrUpdate $addOrUpdate).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /><add key="key2" value="value3" /></appSettings></configuration>'
    }

    it 'should remove element' {
        $remove = @( "key1" )
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8"?><configuration><appSettings /></configuration>') -remove $remove).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings /></configuration>'
        $remove = @( "key1" )
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /></appSettings></configuration>') -remove $remove).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings></appSettings></configuration>'
        $remove = @( "key2" )
        (UpdateConfigXml -xml ([xml]'<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /><add key="key2" value="value2" /></appSettings></configuration>') -remove $remove).OuterXml | Should Be '<?xml version="1.0" encoding="utf-8"?><configuration><appSettings><add key="key1" value="value1" /></appSettings></configuration>'
    }
}



