$baseUrl = "http://localhost:8080"
$masterRealm = "master"
$targetRealm = "WeatherShop"
$username = "admin"
$password = "admin"
$clientId = "admin-cli"

# 1. Get Token
Write-Host "1. Getting Admin Token..."
$tokenUrl = "$baseUrl/realms/$masterRealm/protocol/openid-connect/token"
$body = @{
    client_id  = $clientId
    grant_type = "password"
    username   = $username
    password   = $password
}
try {
    $tokenResponse = Invoke-RestMethod -Uri $tokenUrl -Method Post -Body $body
    $token = $tokenResponse.access_token
    Write-Host "   SUCCESS! Token acquired." -ForegroundColor Green
}
catch {
    Write-Host "   FAILED to get token." -ForegroundColor Red
    Write-Host $_.Exception.Message
    exit
}

# 2. Create User
Write-Host "`n2. Creating User 'debuguser'..."
$createUserUrl = "$baseUrl/admin/realms/$targetRealm/users"
$headers = @{
    Authorization  = "Bearer $token"
    "Content-Type" = "application/json"
}

$newUser = @{
    username      = "debuguser_csharp"
    email         = "debuguser_csharp@test.com"
    firstName     = "Debug"
    lastName      = "User"
    enabled       = $true
    emailVerified = $false
    credentials   = @(
        @{
            type      = "password"
            value     = "password123"
            temporary = $false
        }
    )
} | ConvertTo-Json -Depth 5

$newUser = [System.Text.Encoding]::UTF8.GetBytes($newUser)

try {
    Invoke-RestMethod -Uri $createUserUrl -Method Post -Headers $headers -Body $newUser
    Write-Host "   SUCCESS! User created." -ForegroundColor Green
}
catch {
    Write-Host "   FAILED to create user." -ForegroundColor Red
    Write-Host "   Status: $($_.Exception.Response.StatusCode.value__)"
    
    # Read Error Body
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $errorBody = $reader.ReadToEnd()
    Write-Host "   Error Body: $errorBody" -ForegroundColor Red
}
