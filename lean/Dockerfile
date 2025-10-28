# Use the latest Ubuntu development version
FROM ubuntu:rolling

# Set environment variables
ENV DEBIAN_FRONTEND=noninteractive

# Update and install required packages
RUN apt-get update && \
    apt-get install -y \
    software-properties-common \
    curl \
    snapd \
    git \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Install Neovim Nightly via Snap
RUN snap install nvim --classic --edge

# Install Lean programming language
RUN curl -L https://leanprover.github.io/lean4/lean4-linux.zip -o lean4.zip && \
    unzip lean4.zip -d /usr/local/ && \
    rm lean4.zip && \
    echo 'export PATH=$PATH:/usr/local/lean4/bin' >> /etc/profile

# Set the working directory
WORKDIR /workspace

# Start the shell
CMD ["/bin/bash"]

