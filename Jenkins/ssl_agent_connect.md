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

