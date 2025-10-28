# Base image
FROM ubuntu:latest

# Install necessary dependencies
RUN apt-get update && apt-get install -y \
    curl \
    git \
    sudo \
    && rm -rf /var/lib/apt/lists/*

# Install Nix
RUN curl -L https://nixos.org/nix/install | sh

# Load Nix environment for the current session
ENV USER=root
RUN . /home/root/.nix-profile/etc/profile.d/nix.sh

# Install Home Manager
RUN nix-env -iA nixpkgs.home-manager

# Set up Home Manager for the user (optional, can be customized)
COPY home.nix /etc/nix/hosts/
RUN home-manager switch

# Set default command
CMD [ "/bin/bash" ]

