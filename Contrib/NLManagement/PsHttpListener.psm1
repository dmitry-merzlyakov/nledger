# If you run this script in ISE, you may want to configure Execution Policy by this command:
# set-executionpolicy -ExecutionPolicy RemoteSigned -Scope Process

###Add-Type -AssemblyName System.Web

$Global:ActionParams = $null
$Script:shutdownRequest = $false

function RunListener {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$true)]$actions,
        [Parameter(Mandatory=$true)][string]$prefix,
        [Parameter(Mandatory=$false)][scriptblock]$onListenerStart
    )
    try
    {
        Write-Verbose "Starting listener..."
        $Script:shutdownRequest = $false
        $private:listener = New-Object System.Net.HttpListener
        $private:listener.Prefixes.Add($prefix) | Out-Null
        $private:listener.Start() | Out-Null
        Write-Verbose "Listener is started [$prefix]"
        if($onListenerStart) { $onListenerStart.Invoke() | Out-Null }
        [bool]$private:cancellation = $false
        while(!$Script:shutdownRequest) {            
            Write-Verbose "Listening"
            $private:task = $private:listener.GetContextAsync();
            while(!$private:task.Wait(200)) { }
            Write-Verbose "Processing a request"
            [System.Net.HttpListenerContext]$private:context = $private:task.Result
            ProcessRequest $actions $private:context | Out-Null
        }
    }
    finally
    {
        Write-Verbose "Stopping listener..."
        $private:listener.Stop()
        Write-Verbose "Listener is stopped"
    }
}

function ProcessRequest {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$true)]$actions,
        [Parameter(Mandatory=$True)] [System.Net.HttpListenerContext]$context
    )

    [string]$private:url = $context.Request.Url.LocalPath
    Write-Verbose "Request URL: $private:url"

    foreach($private:act in $actions) {
        if ($private:url -match $private:act.pattern) {
            Write-Verbose "Pattern '$($private:act.pattern)' matches given URL"
            $Global:ActionParams = $Matches
            $private:response = $private:act.action.Invoke()
            WriteResponse $context $private:response | Out-Null
            Write-Verbose "Request is properly handled"
            return
        }
    }

    Write-Verbose "No matched actions found; resource not found"
    WriteResponse $context "Resource Not Found" 404 | Out-Null
}

function WriteResponse {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)] [System.Net.HttpListenerContext]$context,
        [Parameter(Mandatory=$false)][string]$response = "OK",
        [Parameter(Mandatory=$false)][int]$statusCode = 200
    )

    $private:buffer = [System.Text.Encoding]::UTF8.GetBytes($response)
    $context.Response.StatusCode = $statusCode
    $context.Response.ContentLength64 = $private:buffer.Length
    $Context.Response.ContentType = "text/html" ##[System.Web.MimeMapping]::GetMimeMapping(".html")
    $context.Response.OutputStream.Write($private:buffer,0,$private:buffer.Length)
    $context.Response.OutputStream.Close()            
}

function getShutdown() {
    [CmdletBinding()]
    Param()
    Write-Verbose "Requested shutdown"
    $Script:shutdownRequest = $True
    return "OK"
}

Export-ModuleMember -function * -alias *