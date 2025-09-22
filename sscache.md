## Complete PowerShell-Based Example

**File structure:**
```
project/
├── CMakeLists.txt
├── build.ps1
├── clean.ps1
└── main.cpp
```

### 1. CMakeLists.txt
```cmake
cmake_minimum_required(VERSION 3.15)
project(MyProject)

# Enable sccache for compiler caching
find_program(SCCACHE sccache)
if(SCCACHE)
    message(STATUS "Using sccache for compilation")
    set(CMAKE_C_COMPILER_LAUNCHER ${SCCACHE})
    set(CMAKE_CXX_COMPILER_LAUNCHER ${SCCACHE})
else()
    message(STATUS "sccache not found, compiling without cache")
endif()

# Create executable from main.cpp
add_executable(main main.cpp)

# Set C++ standard
set_target_properties(main PROPERTIES
    CXX_STANDARD 17
    CXX_STANDARD_REQUIRED ON
)

# Use /Z7 for cacheable debug info (important for sccache + MSVC)
if(MSVC)
    target_compile_options(main PRIVATE /Z7)
    target_link_options(main PRIVATE /DEBUG /INCREMENTAL:NO)
endif()
```

### 2. build.ps1
```powershell
param(
    [string]$BuildType = "Release",
    [string]$BuildDir = "build",
    [switch]$Clean,
    [switch]$Run,
    [switch]$Stats,
    [switch]$DebugCMake
)

# Delete build directory if clean flag is set
if ($Clean -or (Test-Path $BuildDir)) {
    Write-Host "Cleaning build directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $BuildDir -ErrorAction SilentlyContinue
}

# Create build directory
if (!(Test-Path $BuildDir)) {
    New-Item -ItemType Directory -Path $BuildDir | Out-Null
}

# CMake configure
$CmakeArgs = @(
    "-B", $BuildDir
    "-G", "Ninja"
    "-DCMAKE_BUILD_TYPE=$BuildType"
)

if ($DebugCMake) {
    $CmakeArgs += "--debug-output"
}

Write-Host "Configuring CMake with Ninja generator..." -ForegroundColor Green
cmake @CmakeArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "CMake configuration failed!" -ForegroundColor Red
    exit 1
}

# Build
Write-Host "Building project..." -ForegroundColor Green
cmake --build $BuildDir --config $BuildType

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Run the executable if requested
if ($Run) {
    $exePath = Join-Path $BuildDir "main.exe"
    if (Test-Path $exePath) {
        Write-Host "Running executable..." -ForegroundColor Cyan
        & $exePath
    } else {
        Write-Host "Executable not found: $exePath" -ForegroundColor Red
    }
}

# Show sccache stats if requested
if ($Stats) {
    Write-Host "Sccache statistics:" -ForegroundColor Cyan
    sccache --show-stats
}
```

### 3. clean.ps1
```powershell
param([string]$BuildDir = "build")

Write-Host "Cleaning build directory..." -ForegroundColor Yellow

if (Test-Path $BuildDir) {
    Remove-Item -Recurse -Force $BuildDir
    Write-Host "Clean complete!" -ForegroundColor Green
} else {
    Write-Host "Build directory does not exist." -ForegroundColor Yellow
}

# Also clear sccache if requested
if ($args[0] -eq "--full") {
    Write-Host "Clearing sccache..." -ForegroundColor Yellow
    sccache --zero-stats
    sccache --clear
    Write-Host "Sccache cleared!" -ForegroundColor Green
}
```

### 4. main.cpp (example)
```cpp
#include <iostream>

int main() {
    std::cout << "Hello from sccache + MSVC + Ninja + PowerShell!\n";
    std::cout << "This build uses /Z7 for cacheable debug info.\n";
    return 0;
}
```

## Usage Examples:

```powershell
# Basic build (Release)
.\build.ps1

# Debug build
.\build.ps1 -BuildType Debug

# Clean and build
.\build.ps1 -Clean

# Build and run
.\build.ps1 -Run

# Build with sccache stats
.\build.ps1 -Stats

# Debug CMake configuration
.\build.ps1 -DebugCMake

# Just clean
.\clean.ps1

# Clean everything including sccache cache
.\clean.ps1 --full
```

## About `/Z7` vs `/Zi`

### What `/Z7` does:
- **Embeds debug information directly in .obj files**
- **No separate .pdb file is created**
- **Makes compilations cacheable** because no unique PDB timestamps

### Tradeoffs:
```
/Z7 Advantages:
✅ Cacheable builds (works with sccache)
✅ Simpler build setup (no PDB management)
✅ Better for distributed builds

/Z7 Disadvantages:
❌ Larger object files (debug info duplicated in each .obj)
❌ Slower linking (linker processes debug info from each .obj)
❌ Less efficient debug storage

/Zi Advantages:
✅ Smaller object files
✅ Faster linking (single PDB file)
✅ Better for large projects

/Zi Disadvantages:
❌ Non-cacheable builds (unique PDB timestamps)
❌ PDB file management complexity
```

### Recommendation for your project:
**Use `/Z7`** since you only have one source file. The disadvantages are negligible for small projects, and you get full sccache caching benefits.

## Prerequisites:
```powershell
# Install ninja
pip install ninja

# Verify tools are available
sccache --version
cmake --version
ninja --version
```

This PowerShell setup gives you a clean, Windows-native build system with full sccache support!

