$baseUrl = "http://localhost:8080"
$masterRealm = "master"
$targetRealm = "WeatherShop"
$username = "admin"
$password = "admin"
$clientId = "admin-cli"

Write-Host "1. Getting Admin Token from MASTER realm..."
$tokenUrl = "$baseUrl/realms/$masterRealm/protocol/openid-connect/token"
$body = @{
    client_id = $clientId
    grant_type = "password"
    username = $username
    password = $password
}

try {
    $tokenResponse = Invoke-RestMethod -Uri $tokenUrl -Method Post -Body $body
    $token = $tokenResponse.access_token
    Write-Host "   SUCCESS! Token acquired." -ForegroundColor Green
}
catch {
    Write-Host "   FAILED to get token from master realm." -ForegroundColor Red
    Write-Host $_.Exception.Message
    exit
}

Write-Host "`n2. Listing Users in TARGET realm ($targetRealm)..."
$usersUrl = "$baseUrl/admin/realms/$targetRealm/users"
$headers = @{
    Authorization = "Bearer $token"
}

try {
    $users = Invoke-RestMethod -Uri $usersUrl -Method Get -Headers $headers
    Write-Host "   SUCCESS! Users retrieved:" -ForegroundColor Green
    if ($users.Count -eq 0) {
        Write-Host "   (No users found, list is empty)" -ForegroundColor Yellow
    }
    foreach ($user in $users) {
        Write-Host "   - $($user.username) (ID: $($user.id))"
    }
}
catch {
    Write-Host "   FAILED to list users in $targetRealm." -ForegroundColor Red
    Write-Host "   Status Code: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "   Message: $($_.Exception.Message)"
}
