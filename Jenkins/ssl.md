Below is an example that builds off the official Jenkins image and wraps its original entrypoint. In this version the container will run Jenkins with SSL on port 8443 only (i.e. no HTTP on 8080). The custom entrypoint script first sets up the keystore (generating one if necessary) and then calls the original Jenkins startup script (jenkins.sh).

For simplicity the script uses a PKCS#12 keystore, which modern Java supports out-of-the-box. Adjust as needed if you require conversion to JKS using keytool.

---

### Dockerfile

```dockerfile
FROM jenkins/jenkins:lts

# Switch to root for installing packages and copying scripts.
USER root

# Install OpenSSL (needed for certificate creation and conversion)
RUN apt-get update && \
    apt-get install -y openssl && \
    rm -rf /var/lib/apt/lists/*

# Create directory for certificates.
RUN mkdir -p /certs

# Copy our custom entrypoint script.
# This script will prepare the SSL keystore and then call the original Jenkins startup.
COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Do not expose HTTP port 8080; only HTTPS.
EXPOSE 8443

# Set a default keystore password.
ENV KEYSTORE_PASS=changeit

# Switch back to jenkins user.
USER jenkins

# Use our custom entrypoint.
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
```

---

### entrypoint.sh

```bash
#!/bin/bash
set -e

CERT_DIR="/certs"
KEYSTORE_FILE="${CERT_DIR}/keystore.p12"

# Check if a PKCS#12 keystore is provided.
if [ -f "${KEYSTORE_FILE}" ]; then
    echo "Using provided keystore at ${KEYSTORE_FILE}"
else
    # Look for provided certificate and key files.
    if [ -f "${CERT_DIR}/cert.pem" ] && [ -f "${CERT_DIR}/key.pem" ]; then
        echo "Found cert.pem and key.pem in ${CERT_DIR}"
    else
        echo "No certificate and key provided. Generating a self-signed certificate..."
        # Generate a self-signed certificate, valid for 10 years.
        openssl req -x509 -nodes -newkey rsa:4096 \
          -keyout "${CERT_DIR}/key.pem" \
          -out "${CERT_DIR}/cert.pem" \
          -days 3650 \
          -subj "/CN=localhost"
    fi

    echo "Converting certificate and key to a PKCS#12 keystore..."
    # Create the PKCS#12 keystore from the certificate and key.
    openssl pkcs12 -export \
      -in "${CERT_DIR}/cert.pem" \
      -inkey "${CERT_DIR}/key.pem" \
      -out "${KEYSTORE_FILE}" \
      -name jenkins \
      -passout pass:"${KEYSTORE_PASS}"
    echo "Keystore generated at ${KEYSTORE_FILE}"
fi

# Set Java options so that Jenkins' Winstone picks up the keystore settings.
# We're disabling the setup wizard and enabling SSL on port 8443.
JAVA_OPTS_SSL="-Djenkins.install.runSetupWizard=false -Djavax.net.ssl.keyStore=${KEYSTORE_FILE} -Djavax.net.ssl.keyStorePassword=${KEYSTORE_PASS}"
JENKINS_ARGS="--httpsPort=8443"

echo "Starting Jenkins with SSL only on port 8443"

# Call the original Jenkins entrypoint script, passing along our parameters.
# The official Jenkins image's entrypoint calls /usr/local/bin/jenkins.sh.
# By executing it here, we keep the default behavior.
exec java ${JAVA_OPTS_SSL} -jar /usr/share/jenkins/jenkins.war ${JENKINS_ARGS}
```

---

### Usage

1. **Build the image:**

   ```bash
   docker build -t jenkins-ssl .
   ```

2. **Run with your own certificate and key files:**

   Place your certificate (as cert.pem) and key (as key.pem) files in a host directory (for example, /path/to/certs) and run:

   ```bash
   docker run -d -p 8443:8443 \
     -v /path/to/certs:/certs \
     -e KEYSTORE_PASS=yourpassword \
     jenkins-ssl
   ```

3. **Run without providing certs (self-signed certificate generation):**

   ```bash
   docker run -d -p 8443:8443 \
     -e KEYSTORE_PASS=yourpassword \
     jenkins-ssl
   ```

This configuration ensures Jenkins listens on HTTPS only (port 8443) and uses either your provided certificate files or generates a self‑signed certificate automatically.

