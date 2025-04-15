podman run -p 1234:1234 julia:latest julia -e 'using Pkg; Pkg.add("Pluto"); using Pluto; Pluto.run(host="0.0.0.0", port=1234)'

