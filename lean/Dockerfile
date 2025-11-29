# Use the latest Ubuntu development version
FROM ubuntu:rolling

# Set environment variables
ENV DEBIAN_FRONTEND=noninteractive

# Update and install required packages
RUN apt-get update && \
    apt-get install -y \
    software-properties-common \
    curl \
    git \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Install Neovim directly from the package repository
RUN add-apt-repository ppa:neovim-ppa/unstable && \
    apt-get update && \
    apt-get install -y neovim

RUN curl https://elan.lean-lang.org/elan-init.sh -sSf | sh
RUN echo 'source $HOME/.elan/env' >> $HOME/.bashrc

# Create a new command that runs the Snap installation
CMD ["/bin/bash"]

