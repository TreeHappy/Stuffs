
# Jenkins Host and Agent in docker Compose



## Docker Copose

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

  jenkins-agent:
    image: jenkins/inbound-agent
    environment:
      JENKINS_URL: http://jenkins:8080
      JENKINS_AGENT_NAME: "MyAgent"  # Change this to your desired agent name
      JENKINS_SECRET: "<secret>"      # Replace with the actual secret token
    depends_on:
      - jenkins

volumes:
  jenkins_home:


