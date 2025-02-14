```markdown
# Debugging a Chiseled Container with PID/IPC Sharing and Filesystem Access

This guide combines **namespace sharing** (PID, IPC) and **filesystem mounting** to debug an Ubuntu Chiseled container using a secondary tools container.

---

## **Step 1: Start the Ubuntu Chiseled Container**
```bash
docker run -d --name my-chiseled-container ubuntu:chiseled
```

---

## **Step 2: Start the Debug Container**
Run a debug container (e.g., `ubuntu:latest` or `nicolaka/netshoot`) with:
- Shared **PID** and **IPC** namespaces (to inspect processes and shared memory).
- Mounted filesystem from the Chiseled container (read-only for safety).

```bash
docker run -it --rm \
  --pid container:my-chiseled-container \   # Share PID namespace
  --ipc container:my-chiseled-container \   # Share IPC namespace
  -v "$(docker inspect my-chiseled-container --format '{{ .GraphDriver.Data.MergedDir }}'):/chiseled-fs:ro" \  # Mount Chiseled filesystem
  ubuntu:latest \
  bash
```

---

## **Inside the Debug Container**
### **1. Inspect Processes (PID Sharing)**
```bash
ps aux  # View processes from the Chiseled container
```

### **2. Access the Chiseled Filesystem**
```bash
ls /chiseled-fs        # Browse the Chiseled container's root filesystem
cat /chiseled-fs/etc/os-release  # Example: Check OS details
```

### **3. Test IPC (Optional)**
If the Chiseled container uses shared memory (e.g., `/dev/shm`), you can inspect it via the shared IPC namespace.

---

## **Alternative: Option 2 (Bind-Mount Specific Host Directories)**
If you only need specific directories from the **host** (not the Chiseled container), use a bind mount:
```bash
docker run -it --rm \
  --pid container:my-chiseled-container \
  --ipc container:my-chiseled-container \
  -v /path/on/host:/data \  # Mount a host directory to `/data`
  ubuntu:latest \
  bash
```
Now `/data` in the debug container mirrors `/path/on/host` on your host machine.

---

## **Key Notes**
- **PID/IPC Sharing ≠ Filesystem Access**: Namespace sharing alone does not expose the filesystem.
- **Read-Only Mounts**: Use `:ro` to prevent accidental writes to the Chiseled container.
- **No Modification**: The Chiseled container remains untouched and minimal.

---

## **Why This Works**
- **PID Namespace**: Lets you see the Chiseled container’s processes.
- **IPC Namespace**: Allows interaction with shared memory.
- **Filesystem Mount**: Directly access the Chiseled container’s filesystem via Docker’s storage driver path.

Use this approach to debug while keeping your Chiseled container secure and lightweight! 🐳
```

