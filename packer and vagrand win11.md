Here's a **Vagrantfile** that builds a Windows 11 VM using your evaluation ISO and runs a PowerShell setup script. This requires **Packer** to first create a Vagrant box from the ISO, then Vagrant to manage the VM:

---

### **Step 1: Create a Vagrant Box with Packer**
#### **Packer Template (`windows11-eval.pkr.hcl`)**  
Replace `C:\path\to\windows11-eval.iso` with your ISO path.  
```hcl
variable "iso_url" {
  type    = string
  default = "C:\\path\\to\\windows11-eval.iso"
}

variable "iso_checksum" {
  type    = string
  default = "sha256:YOUR_ISO_SHA256_CHECKSUM"  # Use `Get-FileHash` in PowerShell to get this
}

source "hyperv-iso" "windows11" {
  iso_url           = var.iso_url
  iso_checksum      = var.iso_checksum
  disk_size         = 65536  # 64GB
  memory            = 4096
  cpus              = 2
  generation        = 2
  differencing_disk = true
  secure_boot       = false  # Disable for evaluation ISO

  # Unattended install settings (local admin account)
  http_directory = "http"  # Create an "http" folder with Autounattend.xml
  boot_command = [
    "a<wait>a<wait>a<wait>a"  # Trigger automated install
  ]
  shutdown_command = "shutdown /s /t 10 /f /d p:4:1"
}

build {
  sources = ["source.hyperv-iso.windows11"]

  # Post-process to create a Vagrant box
  post-processor "vagrant" {
    output = "windows11-eval.box"
  }
}
```

#### **Autounattend.xml** (Place in `http/Autounattend.xml`)  
```xml
<?xml version="1.0" encoding="utf-8"?>
<unattend xmlns="urn:schemas-microsoft-com:unattend">
  <settings pass="windowsPE">
    <component name="Microsoft-Windows-International-Core-WinPE" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS">
      <SetupUILanguage><UILanguage>en-US</UILanguage></SetupUILanguage>
      <InputLocale>en-US</InputLocale>
      <SystemLocale>en-US</SystemLocale>
      <UILanguage>en-US</UILanguage>
      <UserLocale>en-US</UserLocale>
    </component>
    <component name="Microsoft-Windows-Setup" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS">
      <UserData>
        <AcceptEula>true</AcceptEula>
        <FullName>Vagrant</FullName>
        <Organization>Dev</Organization>
      </UserData>
    </component>
  </settings>
  <settings pass="oobeSystem">
    <component name="Microsoft-Windows-Shell-Setup" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS">
      <UserAccounts>
        <LocalAccounts>
          <LocalAccount wcm:action="add">
            <Password><Value>vagrant</Value></Password>
            <Name>vagrant</Name>
            <Group>Administrators</Group>
          </LocalAccount>
        </LocalAccounts>
      </UserAccounts>
      <OOBE>
        <HideEULAPage>true</HideEULAPage>
        <SkipMachineOOBE>true</SkipMachineOOBE>
        <SkipUserOOBE>true</SkipUserOOBE>
      </OOBE>
    </component>
  </settings>
</unattend>
```

#### **Build the Box**  
```powershell
packer build windows11-eval.pkr.hcl
```

---

### **Step 2: Vagrantfile to Use the Box and Run Setup**
```ruby
Vagrant.configure("2") do |config|
  config.vm.box = "windows11-eval"
  config.vm.box_url = "./windows11-eval.box"  # Path to the Packer-generated box
  config.vm.guest = :windows
  config.vm.communicator = "winrm"  # Required for Windows

  # Provider settings (Hyper-V)
  config.vm.provider "hyperv" do |hv|
    hv.memory = 4096
    hv.cpus = 2
    hv.vmname = "win11-eval-vm"
  end

  # Run your PowerShell setup script
  config.vm.provision "shell", path: "./setup.ps1"
end
```

---

### **Step 3: Example Setup Script (`setup.ps1`)**  
```powershell
# Install Chocolatey
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Install packages with Chocolatey
choco install -y git python vscode

# Enable SSH (optional)
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0
Start-Service sshd
Set-Service -Name sshd -StartupType Automatic

# Configure WinRM for Vagrant
winrm quickconfig -q
winrm set winrm/config/winrs '@{MaxMemoryPerShellMB="1024"}'
```

---

### **Step 4: Start the VM**  
```bash
vagrant up --provider=hyperv
```

---

### **Key Notes**  
1. **Packer Requirement**: You must build the Vagrant box first (Step 1).  
2. **WinRM**: Vagrant uses WinRM to communicate with Windows VMs. The Packer template configures this automatically.  
3. **Evaluation Period**: The Windows 11 evaluation ISO expires after 90 days. Reset it with:  
   ```powershell
   slmgr /rearm
   ```
4. **ISO Checksum**: Use `Get-FileHash -Algorithm SHA256 C:\path\to\iso` in PowerShell to get the checksum.  

---

### **Troubleshooting**  
- **WinRM Errors**: Ensure the VM’s firewall allows WinRM (port 5985).  
- **Packer Failures**: Verify the `Autounattend.xml` matches your ISO version.  
- **Vagrant SSH**: Use `vagrant winrm -c PowerShell` to access the VM.  

This workflow lets you automate the entire process from ISO to a fully configured Windows 11 VM! 🚀

