Here's an explanation of the `curl` flags and their PowerShell (pwsh) equivalent:

**Original cURL Commands Explained:**
1. **First Command (Get Jenkins Crumb):**
   ```bash
   -u $JENKINS_USER:$JENKINS_USER_PASS  # Basic authentication
   -s                                   # Silent mode (no progress/errors)
   -c /tmp/cookies                      # Save cookies to file
   $JENKINS_URL/crumbIssuer/api/json    # Endpoint URL
   ```

2. **Second Command (Generate Access Token):**
   ```bash
   -H "Jenkins-Crumb:$JENKINS_CRUMB"    # Add custom header
   -b /tmp/cookies                      # Load cookies from file
   --data 'newTokenName=GlobalToken'    # POST data payload
   ```

**PowerShell Equivalent:**
```powershell
# Encode credentials for Basic Auth
$encodedAuth = [Convert]::ToBase64String(
    [Text.Encoding]::ASCII.GetBytes("${JENKINS_USER}:${JENKINS_USER_PASS}")
)

# Get Jenkins Crumb with cookie session (replaces -c /tmp/cookies)
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$crumbResponse = Invoke-RestMethod -Uri "$JENKINS_URL/crumbIssuer/api/json" `
    -Headers @{Authorization = "Basic $encodedAuth"} `
    -WebSession $session
$JENKINS_CRUMB = $crumbResponse.crumb

# Generate Access Token (replaces -b /tmp/cookies)
$tokenParams = @{
    Uri = "$JENKINS_URL/me/descriptorByName/jenkins.security.ApiTokenProperty/generateNewToken"
    Headers = @{
        Authorization = "Basic $encodedAuth"
        "Jenkins-Crumb" = $JENKINS_CRUMB
    }
    WebSession = $session  # Reuse cookie session
    Method = "POST"
    Body = @{newTokenName = "GlobalToken"}
}
$tokenResult = Invoke-RestMethod @tokenParams
$ACCESS_TOKEN = $tokenResult.data.tokenValue
```

**Key Differences:**
1. **Cookie Management:**
   - Instead of `-c` (save cookies) and `-b` (load cookies), pwsh uses a `WebRequestSession` object.
   - The session object automatically handles cookies between requests (no temp files needed).

2. **Authentication:**
   - Basic Auth is explicitly encoded in PowerShell.
   - The original `-u` flag is replaced with an `Authorization` header.

3. **Response Parsing:**
   - `Invoke-RestMethod` automatically parses JSON responses (replaces `jq -r`).
   - Access properties directly (e.g., `$crumbResponse.crumb`).

4. **POST Data:**
   - PowerShell uses a `Body` parameter with a hashtable instead of `--data`.
   - Form-encoding is handled automatically.

This PowerShell version eliminates the need for temporary cookie files and provides better native type handling.


Here's the C# equivalent using `HttpClient` with cookie handling and JSON parsing:

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class JenkinsTokenGenerator
{
    public static async Task<string> GetJenkinsAccessToken(string jenkinsUrl, string user, string password)
    {
        // Create HTTP client with cookie container
        var cookieContainer = new System.Net.CookieContainer();
        using var handler = new HttpClientHandler { CookieContainer = cookieContainer };
        using var client = new HttpClient(handler);
        
        // Set basic authentication
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        // Get Jenkins Crumb
        var crumbResponse = await client.GetAsync($"{jenkinsUrl}/crumbIssuer/api/json");
        crumbResponse.EnsureSuccessStatusCode();
        
        var crumbJson = await crumbResponse.Content.ReadAsStringAsync();
        var crumbDoc = JsonDocument.Parse(crumbJson);
        var jenkinsCrumb = crumbDoc.RootElement.GetProperty("crumb").GetString();

        // Generate Access Token
        var request = new HttpRequestMessage(HttpMethod.Post, 
            $"{jenkinsUrl}/me/descriptorByName/jenkins.security.ApiTokenProperty/generateNewToken");
        
        request.Headers.Add("Jenkins-Crumb", jenkinsCrumb);
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("newTokenName", "GlobalToken")
        });

        var tokenResponse = await client.SendAsync(request);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        var tokenDoc = JsonDocument.Parse(tokenJson);
        return tokenDoc.RootElement
            .GetProperty("data")
            .GetProperty("tokenValue")
            .GetString();
    }
}
```

**Key Components:**
1. **Cookie Handling:**
   - `HttpClientHandler.CookieContainer` automatically manages cookies between requests
   - No need for temporary files like in the original curl command

2. **Authentication:**
   - Basic auth is set in the `Authorization` header using Base64 encoding
   - Credentials are combined in the format "username:password"

3. **JSON Handling:**
   - Uses `System.Text.Json` for parsing JSON responses
   - Direct property access instead of using jq

4. **Request Flow:**
   - First GET request gets the crumb and stores cookies automatically
   - Second POST request reuses the same cookie container and adds the crumb header
   - Form data is sent using `FormUrlEncodedContent`

**Usage Example:**
```csharp
var accessToken = await JenkinsTokenGenerator.GetJenkinsAccessToken(
    "http://your-jenkins-server",
    "your-username",
    "your-password"
);
Console.WriteLine($"Generated token: {accessToken}");
```

**Important Notes:**
1. Error handling (try/catch) should be added for production use
2. The HTTP client is properly disposed with `using` statements
3. Ensure your Jenkins server URL is correct and accessible
4. The JSON property names ("crumb", "data", "tokenValue") must match your Jenkins API response

This implementation maintains the same security flow as the original curl commands while using C#'s native HTTP and JSON handling capabilities.

https://community.jenkins.io/t/jenkins-rest-api-to-create-an-agent/24730
https://github.com/jenkins-zh/jenkins-cli/blob/c926d60cb6f97f9e28f9e1bee689ceaa48a6622c/client/user.go#L102
https://stackoverflow.com/questions/45466090/how-to-get-the-api-token-for-jenkins

```bash
#!/bin/bash

FILE=/var/lib/jenkins/api_token.txt

if [ -f "$FILE" ]; then
    echo "$FILE exists."
else 
    echo "$FILE does not exist."
    JENKINS_URL=http://127.0.0.1:8080
    JENKINS_USER=admin
    JENKINS_USER_PASS={{ jenkins_admin_password }}

    JENKINS_CRUMB=$(curl -u $JENKINS_USER:$JENKINS_USER_PASS -s -c /tmp/cookies $JENKINS_URL'/crumbIssuer/api/json' | jq -r '.crumb')
    ACCESS_TOKEN=$(curl -u $JENKINS_USER:$JENKINS_USER_PASS -H "Jenkins-Crumb:$JENKINS_CRUMB" -s \
                        -b /tmp/cookies $JENKINS_URL'/me/descriptorByName/jenkins.security.ApiTokenProperty/generateNewToken' \
                        --data 'newTokenName=GlobalToken' | jq -r '.data.tokenValue')
    echo $ACCESS_TOKEN > /var/lib/jenkins/api_token.txt
fi
```
