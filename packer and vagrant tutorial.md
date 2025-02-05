Below is a **step-by-step tutorial** with complete file contents and references to specific lines. All files are provided in full, with annotations explaining key sections.

---

### **Tutorial: Build a Windows 11 Evaluation VM with SSH and Automated Script Execution**

#### **Prerequisites**
1. **Tools**:
   - [Packer](https://www.packer.io/) (v1.10+)
   - [Vagrant](https://www.vagrantup.com/) (v2.3+)
   - Hyper-V (Windows) or libvirt/QEMU (Linux)
2. **Windows 11 Evaluation ISO**:  
   Download from [Microsoft Evaluation Center](https://www.microsoft.com/en-us/evalcenter/evaluate-windows-11-enterprise).

---

### **Step 1: Project Structure**
Create the following files and folders:  
```
windows-11-vagrant/
├── answer_files/
│   └── Autounattend.xml       # Unattended Windows setup
├── scripts/
│   └── install.ps1            # Custom PowerShell script
├── windows-11.pkr.hcl         # Packer template
└── Vagrantfile                # Vagrant configuration
```

---

### **Step 2: Configure `Autounattend.xml`**
Create `answer_files/Autounattend.xml` with the following content:  
```xml
<?xml version="1.0" encoding="utf-8"?>
<unattend xmlns="urn:schemas-microsoft-com:unattend">
  <settings pass="windowsPE">
    <component name="Microsoft-Windows-International-Core-WinPE" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS" xmlns:wcm="http://schemas.microsoft.com/WMIConfig/2002/State" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <SetupUILanguage>
        <UILanguage>en-US</UILanguage>
      </SetupUILanguage>
      <InputLocale>en-US</InputLocale>
      <SystemLocale>en-US</SystemLocale>
      <UILanguage>en-US</UILanguage>
      <UserLocale>en-US</UserLocale>
    </component>
    <component name="Microsoft-Windows-Setup" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS" xmlns:wcm="http://schemas.microsoft.com/WMIConfig/2002/State" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <UserData>
        <AcceptEula>true</AcceptEula>
      </UserData>
    </component>
  </settings>
  <settings pass="oobeSystem">
    <component name="Microsoft-Windows-Shell-Setup" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS" xmlns:wcm="http://schemas.microsoft.com/WMIConfig/2002/State">
      <OOBE>
        <HideEULAPage>true</HideEULAPage>
        <HideOEMRegistrationScreen>true</HideOEMRegistrationScreen>
        <HideOnlineAccountScreens>true</HideOnlineAccountScreens>
        <HideWirelessSetupInOOBE>true</HideWirelessSetupInOOBE>
      </OOBE>
      <FirstLogonCommands>
        <!-- Enable SSH (Lines 21-30) -->
        <SynchronousCommand wcm:action="add">
          <Order>1</Order>
          <CommandLine>powershell -Command "Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0"</CommandLine>
        </SynchronousCommand>
        <SynchronousCommand wcm:action="add">
          <Order>2</Order>
          <CommandLine>powershell -Command "Start-Service sshd; Set-Service -Name sshd -StartupType Automatic"</CommandLine>
        </SynchronousCommand>
        <SynchronousCommand wcm:action="add">
          <Order>3</Order>
          <CommandLine>powershell -Command "New-NetFirewallRule -Name 'SSH' -DisplayName 'SSH' -Enabled True -Protocol TCP -Action Allow -LocalPort 22"</CommandLine>
        </SynchronousCommand>
        <!-- Set Execution Policy (Line 31) -->
        <SynchronousCommand wcm:action="add">
          <Order>4</Order>
          <CommandLine>powershell -Command "Set-ExecutionPolicy RemoteSigned -Force"</CommandLine>
        </SynchronousCommand>
      </FirstLogonCommands>
    </component>
  </settings>
</unattend>
```

**Key Sections**:  
- **Lines 21-30**: Enable OpenSSH Server, start the service, and open port 22.  
- **Line 31**: Allow execution of local PowerShell scripts.  

---

### **Step 3: Create the Packer Template**
Create `windows-11.pkr.hcl`:  
```hcl
variable "iso_url" {
  type    = string
  default = "path/to/windows-11-eval.iso"  # Update this path!
}

variable "iso_checksum" {
  type    = string
  default = "none"
}

source "libvirt" "windows-11" {
  // Hypervisor configuration (Lines 9-20)
  memory      = 4096
  vcpu        = 2
  disk_size   = 50 * 1024  # 50GB
  iso_url     = var.iso_url
  iso_checksum = var.iso_checksum
  http_directory = "answer_files"
  shutdown_command = "shutdown /s /t 10 /f /d p:4:1"
  qemuargs = [
    ["-cpu", "host"],
  ]
}

build {
  sources = ["source.libvirt.windows-11"]

  // Copy Autounattend.xml (Lines 25-30)
  provisioner "file" {
    source      = "answer_files/Autounattend.xml"
    destination = "C:/Windows/Panther/Unattend/Autounattend.xml"
  }

  // Copy and run the install.ps1 script (Lines 32-42)
  provisioner "file" {
    source      = "scripts/install.ps1"
    destination = "C:/install.ps1"
  }

  provisioner "powershell" {
    inline = [
      "C:/install.ps1"
    ]
  }
}
```

**Key Sections**:  
- **Lines 9-20**: VM hardware settings (adjust `iso_url` to your ISO path).  
- **Lines 25-30**: Copy the unattended setup file.  
- **Lines 32-42**: Copy and execute the PowerShell script.  

---

### **Step 4: Add the PowerShell Script**
Create `scripts/install.ps1`:  
```powershell
# Install Chocolatey (Lines 1-3)
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Install Git (Line 5)
choco install -y git
```

**Key Sections**:  
- **Lines 1-3**: Install Chocolatey (a package manager).  
- **Line 5**: Install Git using Chocolatey.  

---

### **Step 5: Create the Vagrantfile**
Create `Vagrantfile`:  
```ruby
Vagrant.configure("2") do |config|
  config.vm.box = "windows-11-amd64"  # Name from `vagrant box list`
  config.vm.guest = :windows
  config.vm.communicator = "ssh"
  config.vm.network "forwarded_port", guest: 22, host: 2222, id: "ssh"

  # Provider-specific settings (Lines 6-9)
  config.vm.provider "libvirt" do |libvirt|
    libvirt.memory = 4096
    libvirt.cpus = 2
  end
end
```

**Key Sections**:  
- **Lines 6-9**: VM resource allocation (adjust as needed).  

---

### **Step 6: Build the VM**
1. **Run Packer**:  
   ```bash
   packer build windows-11.pkr.hcl
   ```
   This creates a Vagrant box named `windows-11-amd64`.

2. **Add the Box to Vagrant**:  
   ```bash
   vagrant box add windows-11-amd64 windows-11-amd64-libvirt.box
   ```

3. **Start the VM**:  
   ```bash
   vagrant up
   ```

4. **Connect via SSH**:  
   ```bash
   vagrant ssh
   ```

---

### **Verification**
1. **Check SSH Access**:  
   ```bash
   ssh vagrant@localhost -p 2222
   ```

2. **Confirm Software Installation**:  
   ```powershell
   choco --version  # Should show Chocolatey v2.2.2+
   git --version    # Should show Git v2.45.1+
   ```

---

### **Troubleshooting**
- **Packer Build Fails**:  
  - Ensure `iso_url` points to a valid Windows 11 evaluation ISO.  
  - Use `packer build -debug windows-11.pkr.hcl` for detailed logs.  
- **SSH Connection Issues**:  
  - Verify port `2222` is not blocked by your firewall.  
  - Check if the SSH service is running in the VM:  
    ```powershell
    Get-Service sshd
    ```

---

### **Final Notes**
- **Evaluation Period**: The VM will work for 90 days without activation.  
- **Customization**: Modify `install.ps1` to add more software (e.g., Docker, VS Code).  
- **Full Files**: All files are provided above. Adjust paths/versions as needed.

