Yes, it is definitely possible to run a Jenkins server in a Docker container using SSL with self‐signed certificates and have a separate Jenkins agent (or multiple agents) connect to it. However, there are a few important considerations to keep in mind:

1. SSL Configuration in Jenkins Container:
   - You’ll need to configure your Jenkins Docker container to serve HTTPS. This typically involves either configuring the embedded servlet container (like Jetty) with your keystore containing the self‑signed certificate or fronting Jenkins with a reverse proxy (NGINX, Apache, Traefik, etc.) that handles SSL termination.
   - In the case of using Jenkins’ own configuration, you may need to pass additional Java options (for example, –Djavax.net.ssl.keyStore and –Djavax.net.ssl.keyStorePassword) when starting Jenkins.

2. Trusting the Self‑Signed Certificate on the Jenkins Agent:
   - When using self‑signed certificates, the Jenkins agent (which might be running in another Docker container or on another machine) will not trust the certificate by default.
   - To resolve this, you must configure the agent’s Java runtime to trust your self‑signed certificate. This can be done by importing your certificate into the Java truststore on the agent machine or container.
   - For Dockerized agents, you can either bake the certificate into the Docker image or mount the certificate file and configure Java accordingly.

3. Connection between Jenkins Master and Agent:
   - Ensure that the agent’s connection settings (URL, port, etc.) match the Jenkins master’s configuration.
   - When using self‑signed certificates, you might have to add JVM options such as –Djavax.net.ssl.trustStore along with the related truststore password on the agent side.
   - Depending on the plugin you are using for agent connectivity (e.g., JNLP inbound agents), you might also have to handle any certificate verification issues by configuring the necessary Java system properties or adjusting the agent launch command.

4. Practical Steps:
   - Generate your self‑signed certificate and create a keystore.
   - Configure your Jenkins Docker container (or your reverse proxy if you’re using one) to use this certificate.
   - Ensure that when the Jenkins master starts (e.g., by setting up the Dockerfile or Docker Compose file), it is bound to the correct host/port and is serving HTTPS.
   - On the agent side, pre-import the self‑signed certificate into the Java truststore (using keytool) or provide your truststore as part of your agent image.
   - Test the connectivity to ensure that when the agent tries to connect back to the master (using JNLP or other protocols), the SSL handshake completes successfully.

5. Troubleshooting:
   - If agents are failing to connect due to SSL errors, check the logs for certificate path errors or untrusted certificate exceptions.
   - You might also verify the certificate using tools like openssl to ensure it’s correctly served by the Jenkins endpoint.

In summary, running Jenkins with self‑signed certificates requires some additional effort to ensure that all connected systems trust your certificate. Once properly set up, both the Jenkins server and its agents should be able to communicate securely over SSL.

To connect an inbound Jenkins agent container to a Jenkins master with a self-signed SSL certificate, you need to ensure the agent trusts the certificate. Here's a step-by-step solution:

### 1. **Export the Jenkins Master's Certificate**
   - **From the Jenkins Master Host**:
     ```bash
     openssl s_client -connect jenkins-master:443 </dev/null | openssl x509 -outform PEM > jenkins-master.crt
     ```
   - Replace `jenkins-master` with the hostname or IP of your Jenkins master.

### 2. **Create a Custom Agent Docker Image**
   - **Dockerfile**:
     ```dockerfile
     FROM jenkins/inbound-agent:latest

     # Switch to root to modify system files
     USER root

     # Copy the certificate into the image
     COPY jenkins-master.crt /tmp/jenkins-master.crt

     # Import the certificate into the Java truststore
     RUN keytool -import -noprompt \
         -alias jenkins-master \
         -file /tmp/jenkins-master.crt \
         -keystore $JAVA_HOME/lib/security/cacerts \
         -storepass changeit

     # Switch back to the jenkins user
     USER jenkins
     ```
   - **Replace `$JAVA_HOME`** with the correct path (e.g., `/usr/local/openjdk-11/lib/security/cacerts` for JDK 11).

### 3. **Build the Custom Agent Image**
   ```bash
   docker build -t custom-jenkins-agent .
   ```

### 4. **Run the Agent Container**
   - Use the custom image when starting the agent:
     ```bash
     docker run -d --name jenkins-agent \
       -e JENKINS_URL=https://jenkins-master:443 \
       -e JENKINS_SECRET=<AGENT_SECRET> \
       -e JENKINS_AGENT_NAME=<AGENT_NAME> \
       custom-jenkins-agent
     ```
   - Replace `<AGENT_SECRET>` and `<AGENT_NAME>` with values from your Jenkins master.

### Alternative: Disable Certificate Verification (Not Recommended)
   - Add `-noCertificateCheck` to the agent's JNLP arguments:
     ```bash
     java -jar agent.jar -jnlpUrl https://jenkins-master:443/... -noCertificateCheck -secret <SECRET>
     ```

### Verify the Connection
   - Check the agent logs to ensure it connects without SSL errors.

### Notes
- **Java Truststore Path**: Confirm the correct `cacerts` path in your base image.
- **Hostname Mismatch**: Ensure the certificate's SAN includes the hostname used by the agent to connect.
- **Reverse Proxy**: If using a proxy, export its certificate instead.

By importing the self-signed certificate into the agent's truststore, the agent will trust the Jenkins master's SSL certificate, resolving the validation error.

* https://community.jenkins.io/t/how-to-connect-agent-jnlp-over-https/7294

