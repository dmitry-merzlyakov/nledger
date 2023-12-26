$Script:shutdownRequest = $false

function Start-HttpListener {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][hashtable]$actions,
        [Parameter(Mandatory)][string]$prefix,
        [Parameter(Mandatory)][scriptblock]$onListenerStart
    )
    try
    {
        Write-Verbose "Starting listener..."
        $Script:shutdownRequest = $false
        $listener = New-Object System.Net.HttpListener
        $null = $listener.Prefixes.Add($prefix)
        $null = $listener.Start()
        Write-Verbose "Listener is started [$prefix]"
        if($onListenerStart) { $null = $onListenerStart.Invoke() }
        while(!$shutdownRequest) {            
            Write-Verbose "Listening"
            $task = $private:listener.GetContextAsync();
            while(!$task.Wait(200)) { }
            Write-Verbose "Processing a request"
            [System.Net.HttpListenerContext]$context = $task.Result
            $null = Invoke-RequestProcessing $actions $context
        }
    }
    finally
    {
        Write-Verbose "Stopping listener..."
        $null = $listener.Stop()
        Write-Verbose "Listener is stopped"
    }
}

function Invoke-RequestProcessing {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][Hashtable]$actions,
        [Parameter(Mandatory)][System.Net.HttpListenerContext]$context
    )

    [string]$url = $context.Request.Url.LocalPath
    Write-Verbose "Request URL: $url"

    foreach($pattern in $actions.Keys) {
        if ($url -match $pattern) {
            Write-Verbose "Pattern '$pattern' matches given URL"
            try {
                $response = $actions[$pattern].InvokeWithContext($null, [psvariable]::new("_", $Matches))[0]
                Write-Verbose "Invoke response: $response"
                $null = Write-HttpResponse $context $response
                Write-Verbose "Request is properly handled"
            }
            catch {
                Write-Error "Http Request Processing Error: $($_.ToString())"
                $null = Write-HttpResponse $context "Server Error" 500
            }
            return
        }
    }

    Write-Verbose "No matched actions found; resource not found"
    $null = Write-HttpResponse $context "Resource Not Found" 404
}

function Write-HttpResponse {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory)][System.Net.HttpListenerContext]$context,
        [Parameter()][string]$response = "OK",
        [Parameter()][int]$statusCode = 200
    )

    $buffer = [System.Text.Encoding]::UTF8.GetBytes($response)
    $context.Response.StatusCode = $statusCode
    $context.Response.ContentLength64 = $buffer.Length
    $Context.Response.ContentType = "text/html"
    $null = $context.Response.OutputStream.Write($buffer,0,$buffer.Length)
    $null = $context.Response.OutputStream.Close()            
}

function Invoke-HttpListenerShutdown() {
    [CmdletBinding()]
    Param()
    Write-Verbose "Requested shutdown"
    $Script:shutdownRequest = $True
    return "OK"
}
