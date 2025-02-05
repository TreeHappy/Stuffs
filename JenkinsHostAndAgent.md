
# Jenkins Host and Agent in docker Compose



## Docker Copose


```yaml
version: '3.8'

services:
  jenkins:
    image: jenkins/jenkins:lts
    ports:
      - "8080:8080"
      - "50000:50000"
    volumes:
      - jenkins_home:/var/jenkins_home
    environment:
      JENKINS_OPTS: --httpPort=8080
      DNS_SERVER: ${DNS_SERVER}  # Use the DNS_SERVER environment variable
    networks:
      - jenkins-network
    restart: unless-stopped  # Restart policy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/login"]  # Check if Jenkins is up
      interval: 30s
      timeout: 10s
      retries: 5
    dns:
      - ${DNS_SERVER}  # Use the DNS_SERVER environment variable

  jenkins-agent:
    image: jenkins/inbound-agent
    environment:
      JENKINS_URL: http://jenkins:8080
      JENKINS_AGENT_NAME: "MyAgent"  # Change this to your desired agent name
      JENKINS_SECRET: "<secret>"      # Replace with the actual secret token
      DNS_SERVER: ${DNS_SERVER}  # Use the DNS_SERVER environment variable
    depends_on:
      jenkins:
        condition: service_healthy  # Wait for Jenkins to be healthy
    networks:
      - jenkins-network
    restart: unless-stopped  # Restart policy
    dns:
      - ${DNS_SERVER}  # Use the DNS_SERVER environment variable
    extra_hosts:
      myhost: 192.168.1.100  # Custom host entry

volumes:
  jenkins_home:

networks:
  jenkins-network:

```

``` ruby
# Ensure the Docker service is installed and running
class { 'docker':
  manage_package => true,
  manage_service => true,
}

# Define the Docker volume
docker::volume { 'jenkins_home':
  ensure => present,
}

# Pull the latest Jenkins image
docker::image { 'jenkins/jenkins':
  tag     => 'latest',
  ensure  => 'latest',  # Ensure the latest image is pulled
  require => Class['docker'],
}

# Run the Jenkins container
docker::run { 'jenkins':
  image   => 'jenkins/jenkins:latest',  # Use the latest tag
  ports   => ['8080:8080', '50000:50000'],
  volumes => ['/var/jenkins_home'],
  env     => {
    'JENKINS_OPTS' => '--httpPort=8080',
  },
  restart => 'unless-stopped',
  require => [
    Docker::Image['jenkins/jenkins'],
    Docker::Volume['jenkins_home'],
  ],
}

```

```powershell
# Define variables
$jenkinsUrl = "http://<jenkins-url>"  # Replace with your Jenkins URL
$agentName = "MyAgent"                  # Replace with your agent name
$username = "<username>"                 # Replace with your Jenkins username
$apiToken = "<api_token>"                # Replace with your Jenkins API token

# Construct the API endpoint
$apiEndpoint = "$jenkinsUrl/computer/$agentName/slave-agent.jnlp"

# Make the API call
$response = Invoke-RestMethod -Uri $apiEndpoint -Method Get -Credential (New-Object System.Management.Automation.PSCredential($username, (ConvertTo-SecureString $apiToken -AsPlainText -Force)))

# Load the response as XML
[xml]$xmlResponse = $response

# Extract the secret token
$secretToken = $xmlResponse.jnlp.security.secret

# Output the secret token
Write-Output "The secret token for agent '$agentName' is: $secretToken"


```

