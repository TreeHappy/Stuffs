Yes, you can list Docker image packages in a Gitea server using its REST API. Below is a step-by-step guide:

---

### **1. Gitea API Endpoints for Packages**
Gitea provides endpoints to list packages under a **user**, **organization**, or **repository**. Use the `type=docker` query parameter to filter Docker images.

#### **Endpoints**:
- **List packages in a repository**:
  ```
  GET /api/v1/repos/{owner}/{repo}/packages?type=docker
  ```
- **List packages under a user**:
  ```
  GET /api/v1/users/{owner}/packages?type=docker
  ```
- **List packages under an organization**:
  ```
  GET /api/v1/orgs/{org}/packages?type=docker
  ```

---

### **2. Authentication**
You need an **access token** with appropriate permissions (e.g., `read:package`). Include it in the request header:
```bash
-H "Authorization: token YOUR_ACCESS_TOKEN"
```

---

### **3. Example: List Docker Packages in a Repository**
#### **Using `curl`**:
```bash
curl -X GET "https://your-gitea.com/api/v1/repos/myuser/myrepo/packages?type=docker" \
  -H "Authorization: token YOUR_ACCESS_TOKEN"
```

#### **Sample Response**:
```json
[
  {
    "id": 1,
    "type": "docker",
    "name": "myimage",
    "version": "latest",
    "creator": { ... },
    "created_at": "2023-01-01T00:00:00Z"
  }
]
```

---

### **4. List All Docker Images Across the Server**
To list **all Docker images** across the server:
1. **List all users/orgs/repos** using their respective APIs.
2. **Iterate through them** and query the package endpoints for each.

#### Example Workflow:
1. List all repositories:
   ```bash
   curl -X GET "https://your-gitea.com/api/v1/user/repos" \
     -H "Authorization: token YOUR_ACCESS_TOKEN"
   ```
2. For each repository, call `/repos/{owner}/{repo}/packages?type=docker`.

---

### **5. Using Docker Registry API (Alternative)**
If Docker images are stored in Gitea's **container registry** (not the package registry), use the Docker Registry API:
```bash
curl -X GET "https://your-gitea.com/v2/_catalog" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```
This requires a token with registry read access.

---

### **Notes**:
- Replace `{owner}`, `{repo}`, and URLs with your Gitea instance details.
- Admin users may have broader access to list packages server-wide.

For more details, refer to the [Gitea API Documentation](https://docs.gitea.com/api/next/).

## Delete the package using its ID:

curl -X DELETE "https://your-gitea.com/api/v1/packages/{owner}/123" \
  -H "Authorization: token YOUR_ACCESS_TOKEN"
