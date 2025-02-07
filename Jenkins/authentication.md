Here's a Jenkins Configuration as Code (JCasC) YAML snippet for Entra ID (Azure AD) integration with the specified permissions:

```yaml
jenkins:
  securityRealm:
    azureActiveDirectory:
      tenantID: "YOUR_TENANT_ID"                  # Entra ID Directory (Tenant) ID
      clientID: "YOUR_CLIENT_ID"                  # Application (Client) ID
      clientSecret:
        $class: "AzureClientSecret"
        secret: "YOUR_CREDENTIALS_ID"             # Jenkins credential ID storing client secret
      cacheDuration: 600                          # Optional token cache duration

  authorizationStrategy:
    fullControlOnceLoggedIn:
      allowAnonymousRead: false                   # Block all unauthorized access

  security:
    queueItemAuthenticator:
      authenticators: []
```

**Setup Instructions:**
1. Create a credential in Jenkins:
   - Type: **Microsoft Azure Service Principal**
   - Credential ID: `YOUR_CREDENTIALS_ID` (match with clientSecret.secret)
   - Input your Client Secret in the credential

2. Required Azure AD App Registration Permissions:
   ```plaintext
   Microsoft Graph:
   - openid
   - profile
   - User.Read
   - email
   ```

3. Features:
   - ✅ Authenticated users get full administrative privileges
   - ❌ Anonymous users get zero access
   - Uses Entra ID for identity management
   - Includes token caching for performance

**Important Security Note:** This configuration gives complete control to *all authenticated users*. Consider more granular permissions (e.g., using groups) for production environments.
