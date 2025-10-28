{ config, pkgs, ... }:

{
  # Basic Home Manager configurations
  home.username = "nibu"; # Substitute with your username
  home.homeDirectory = "/home/nibu"; # Substitute with your home directory

  # Shell configuration
  programs.zsh.enable = true;
  programs.zsh.promptInit = true;

  # Package management
  home.packages = [
    pkgs.git
    pkgs.nvim
  ];

  # Environment variables
  environment.variables = {
    EDITOR = "nvim";
    VISUAL = "nvim";
  };

  # Set up some common configurations
  fonts = {
    enable = true;
    defaultFont = "Noto Sans";
    # Define additional font options here
  };

  # Additional services like SSH
  services.ssh.enable = true;

  # Define your own custom configurations here
}

