(Due to technical issues, the search service is temporarily unavailable.)

Here are some popular repositories and tools for detecting security issues (like leaked passwords/keys) in Git hooks:

---

### 1. **GitGuardian ggshield**  
- **GitHub**: [GitGuardian/ggshield](https://github.com/GitGuardian/ggshield)  
- A CLI tool to detect secrets in code, with **pre-commit**, **pre-push**, and **pre-receive** hooks.  
- Example pre-commit config:  
  ```yaml
  - repo: https://github.com/GitGuardian/ggshield
    rev: main
    hooks:
      - id: ggshield
  ```

---

### 2. **TruffleHog**  
- **GitHub**: [trufflesecurity/trufflehog](https://github.com/trufflesecurity/trufflehog)  
- Scans Git history/repos for high-entropy strings (e.g., API keys, passwords).  
- Pre-commit example:  
  ```yaml
  - repo: https://github.com/trufflesecurity/trufflehog
    rev: main
    hooks:
      - id: trufflehog
        args: [--regex, --entropy=False]
  ```

---

### 3. **git-secrets (AWS Labs)**  
- **GitHub**: [awslabs/git-secrets](https://github.com/awslabs/git-secrets) 
- Focuses on AWS keys but customizable for other secrets.  
- Install and enable hooks:  
  ```bash
  git secrets --install
  git secrets --register-aws
  ```

---

### 4. **detect-secrets (Yelp)**  
- **GitHub**: [Yelp/detect-secrets](https://github.com/Yelp/detect-secrets)  
- Proactively checks for secrets using entropy and keyword scanning.  
- Pre-commit setup:  
  ```yaml
  - repo: https://github.com/Yelp/detect-secrets
    rev: master
    hooks:
      - id: detect-secrets
        args: [--baseline, .secrets.baseline]
  ```

---

### 5. **pre-commit Hooks Framework**  
- **GitHub**: [pre-commit/pre-commit](https://github.com/pre-commit/pre-commit)  
- Use community hooks like `secretlint` or `forbid-secrets`:  
  ```yaml
  - repo: https://github.com/pre-commit/pre-commit-hooks
    rev: v4.4.0
    hooks:
      - id: forbid-secrets
        args: ['--keys=password,api_key']
  ```

---

### 6. **Talisman (ThoughtWorks)**  
- **GitHub**: [thoughtworks/talisman](https://github.com/thoughtworks/talisman)  
- Scans commits/pushes for secrets, binaries, or suspicious files.  
- Install:  
  ```bash
  curl --silent https://raw.githubusercontent.com/thoughtworks/talisman/master/global_install_scripts/install.bash > /tmp/install_talisman.bash && /bin/bash /tmp/install_talisman.bash
  ```

---

### 7. **Gitleaks**  
- **GitHub**: [gitleaks/gitleaks](https://github.com/gitleaks/gitleaks)  
- Lightweight secret scanning with pre-commit support.  
- Example:  
  ```yaml
  - repo: https://github.com/gitleaks/gitleaks
    rev: v8.18.0
    hooks:
      - id: gitleaks
  ```

---

### 8. **Infisical CLI**  
- **GitHub**: [infisical/infisical](https://github.com/Infisical/infisical)  
- Scans for secrets and integrates with pre-commit.  

---

### 9. **GitLab Secret Detection**  
- Built-in secret scanning in GitLab CI/CD pipelines (no hooks needed).  
- Docs: [GitLab Secret Detection](https://docs.gitlab.com/ee/user/application_security/secret_detection/)

---

### 10. **GitHub Secret Scanning**  
- GitHub automatically scans repos for known secret formats (enable in repo settings).  
- Docs: [GitHub Secret Scanning](https://docs.github.com/en/code-security/secret-scanning)

---

### Example Setup (Pre-Commit + Pre-Receive):
1. **Client-Side (pre-commit)**: Use `ggshield` or `gitleaks` to block commits locally.  
2. **Server-Side (pre-receive)**: Use `Talisman` or `gitleaks` on the Git server to enforce policies.

---

### Tips:
- Combine hooks with a **.gitignore** to exclude sensitive files (e.g., `.env`).  
- Use **environment variables** for secrets instead of hardcoding them.  
- Rotate keys immediately if a leak is detected.

