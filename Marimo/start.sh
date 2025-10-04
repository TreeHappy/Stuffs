podman run -p 8080:8080 -v $(pwd):/app:Z --userns=keep-id -it marimo_image:latest

