podman run -p 1234:1234 julia:latest julia -e 'using Pkg; Pkg.add("Pluto"); using Pluto; Pluto.run(host="0.0.0.0", port=1234)'

# Create a named volume (only needed once)
podman volume create julia_pkgs

# Run container with virtual package storage
podman run -it --rm \
  -v "$(pwd)":/app \
  -v julia_pkgs:/root/.julia \
  docker.io/library/julia:latest \
  julia -e 'using Pkg; Pkg.activate("/app"); include("/app/YOUR_SCRIPT.jl")'


