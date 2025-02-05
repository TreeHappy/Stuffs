Here's the corrected tutorial with styling customization (and no, you don't need `dotnet docfx` – it's just `docfx` after installing the .NET tool):

---

### **DocFX Tutorial with .NET Tool & Custom Styling**

---

### **1. Install DocFX as a .NET Tool**
```bash
dotnet tool install --global docfx
```

---

### **2. Initialize Project & Build Docs**
```bash
mkdir my-docs && cd my-docs
docfx init -q              # Creates minimal config
docfx build                # Generates default docs
docfx serve _site          # Preview at http://localhost:8080
```

---

### **3. Configure `docfx.json` for C# Projects**
Edit `docfx.json` to point to your code:
```json
{
  "metadata": [{
    "src": [{
      "files": [ "../src/**/*.csproj" ],  // Path to your C# projects
      "exclude": [ "**/bin/**", "**/obj/**" ]
    }],
    "dest": "api"
  }],
  "build": {
    "content": [
      { "files": "api/**", "dest": "api" },
      { "files": "docs/**/*.md", "dest": "articles" }
    ],
    "output": "_site",
    "template": ["default"],  // We'll customize this later
    "globalMetadata": {
      "_appTitle": "My Fancy Docs",  // Custom title
      "_enableSearch": true
    }
  }
}
```

---

### **4. Add C# XML Documentation**
In your `.csproj` file:
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

---

### **5. Custom Styling: Create a Custom Template**

#### **Step 1: Export Default Template**
```bash
docfx template export default --output custom-template
```
This creates a `custom-template` folder with all default assets.

#### **Step 2: Modify Styles**
1. Edit `custom-template/styles/docfx.less` (or create `custom.css`):
```less
/* Custom colors */
@theme-color: #2c3e50;
@link-color: #3498db;

/* Custom font */
body {
  font-family: "Segoe UI", sans-serif;
}

/* Fancy navbar */
.navbar {
  background: @theme-color;
  border-radius: 0;
}

/* Code blocks */
pre code {
  background: #f8f9fa;
  border: 1px solid #eee;
}
```

#### **Step 3: Add a Logo**
1. Place your logo in `custom-template/logo.svg`
2. Modify `custom-template/partials/header.tmpl.partial`:
```html
<div class="navbar-header">
  <a class="navbar-brand" href="{{_rel}}index.html">
    <img src="{{_rel}}logo.svg" height="30" alt="My Logo">
  </a>
</div>
```

#### **Step 4: Use Custom Template**
Update `docfx.json`:
```json
"build": {
  "template": ["./custom-template", "default"]  // Custom template first
}
```

---

### **6. Rebuild with Custom Styling**
```bash
docfx build
docfx serve _site  # View your styled docs!
```

---

### **Key Customization Points**
| File/Folder               | Purpose                                  |
|---------------------------|------------------------------------------|
| `styles/docfx.less`       | Main LESS/CSS styles                    |
| `partials/header.tmpl.partial` | Navigation bar layout                |
| `partials/footer.tmpl.partial` | Footer content                     |
| `logo.svg`                | Branding image (PNG/JPG also supported)  |
| `fonts/`                  | Custom font files                       |

---

### **Advanced Customization**
1. **Add Google Analytics**:
   In `docfx.json`:
   ```json
   "globalMetadata": {
     "_appFaviconPath": "favicon.ico",
     "_googleAnalytics": "UA-XXXXX-Y"
   }
   ```

2. **Custom Landing Page**:
   Create `docs/index.md` with YAML header:
   ```markdown
   ---
   title: Home
   ---
   # Welcome to My Fancy Docs!
   ```

3. **Dark Theme**:
   Add to `docfx.less`:
   ```less
   body.dark-mode {
     background: #1a1a1a;
     color: #fff;
   }
   ```

---

### **Final Workflow**
```bash
# After making style changes
docfx build
docfx serve _site  # Preview changes
```

Now you have documentation with your brand's look-and-feel! 🎨

