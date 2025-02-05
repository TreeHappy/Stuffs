
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

# Pull the Jenkins image
docker::image { 'jenkins/jenkins:lts':
  tag     => 'latest',
  require => Class['docker'],
}

# Run the Jenkins container
docker::run { 'jenkins':
  image   => 'jenkins/jenkins:lts',
  ports   => ['8080:8080', '50000:50000'],
  volumes => ['/var/jenkins_home'],
  env     => {
    'JENKINS_OPTS' => '--httpPort=8080',
  },
  restart => 'unless-stopped',
  require => [
    Docker::Image['jenkins/jenkins:lts'],
    Docker::Volume['jenkins_home'],
  ],
}
```

