# Quick Start Guide - Template System

## 🚀 How to Access

1. **Start the Application**
   ```bash
   cd "Notification Application"
   dotnet run
   ```

2. **Login to Your Account**
   - Navigate to: `http://localhost:5000/Account/Login`
   - Use your credentials or register a new account

3. **Access Template Gallery**
   - Navigate to: `http://localhost:5000/Template/SelectTemplate`
   - Or add a link to your navigation menu

## 📁 What's Been Created

### Folder Structure
```
Notification Application/
├── Templates/                          # ✅ Template storage folders
│   ├── Popup/                         # 2 sample templates
│   ├── SelectTemplate/                # Ready for your templates
│   ├── FloatingBar/                   # 2 sample templates
│   ├── Fullscreen/                    # 1 sample template
│   ├── Inline/                        # 1 sample template
│   ├── SlideIn/                       # 1 sample template
│   └── Gamified/                      # 1 sample template
│
├── Controllers/
│   └── TemplateController.cs          # ✅ API endpoints & views
│
├── Models/
│   └── Template.cs                    # ✅ Template models
│
├── Services/
│   ├── IServices.cs                   # ✅ ITemplateService interface
│   └── TemplateService.cs             # ✅ Template management logic
│
├── Views/
│   └── Template/
│       └── SelectTemplate.cshtml      # ✅ Template gallery UI
│
├── wwwroot/
│   ├── js/
│   │   └── template-manager.js        # ✅ Frontend logic
│   └── css/
│       └── template-gallery.css       # ✅ Styling
│
└── TEMPLATE-SYSTEM-GUIDE.md          # ✅ Full documentation
```

## 🎯 Features Implemented

### ✅ Template Categories (7 types)
- **Popup**: Modal dialogs (basic-modal, discount-offer)
- **Select Template**: Custom selection templates
- **Floating Bar**: Top/bottom bars (announcement, countdown)
- **Fullscreen**: Full-screen overlays (welcome screen)
- **Inline**: Embedded forms (newsletter signup)
- **Slide-in**: Corner notifications
- **Gamified**: Interactive templates (spin-the-wheel)

### ✅ Template Management
- Browse templates by category
- Live HTML editor with preview
- Create new templates from UI
- Edit existing templates
- Delete templates
- Use templates in popup creation

### ✅ API Endpoints
- `GET /api/templates/{category}` - List templates
- `GET /api/templates/{category}/{name}` - Get template content
- `POST /api/templates/{category}/{name}` - Save template
- `DELETE /api/templates/{category}/{name}` - Delete template

## 🎨 Sample Templates Included

1. **Popup/basic-modal.html** - Simple newsletter signup modal
2. **Popup/discount-offer.html** - Gradient discount popup with badge
3. **FloatingBar/announcement-bar.html** - Green promo bar
4. **FloatingBar/countdown-timer.html** - Flash sale countdown
5. **Fullscreen/fullscreen-welcome.html** - Welcome screen overlay
6. **Inline/inline-signup.html** - Newsletter signup form
7. **SlideIn/slide-in-notification.html** - Bottom-right notification
8. **Gamified/spin-the-wheel.html** - Interactive wheel game

## 🔧 How to Add More Templates

### Option 1: Through UI (Recommended)
1. Go to `/Template/SelectTemplate`
2. Select a category
3. Click "Create New Template" (or edit existing)
4. Write your HTML/CSS/JS
5. Save

### Option 2: Manually
1. Create `my-template.html` in appropriate folder
2. Add your HTML code with inline styles
3. Refresh the gallery - it will appear automatically

## 🎬 How to Use Templates

### In Popup Creation
```csharp
// When creating a popup, you can load a template:
var templateContent = await _templateService
    .GetTemplateContentAsync("popup", "basic-modal.html");
```

### Through the UI
1. Browse to template gallery
2. Click "Use This" on any template
3. Get redirected to popup creator with template loaded

## 📝 Template Format

```html
<div class="my-template">
    <!-- Your HTML -->
    <h2>Title</h2>
    <form>
        <input type="email" placeholder="Email">
        <button>Submit</button>
    </form>
</div>

<style>
/* Your CSS - scoped to .my-template */
.my-template {
    padding: 20px;
    background: white;
}
</style>

<script>
// Optional JavaScript
document.querySelector('form').addEventListener('submit', (e) => {
    e.preventDefault();
    // Handle form submission
});
</script>
```

## 🔐 Security Features

- ✅ Requires authentication
- ✅ Path traversal protection
- ✅ File type validation (.html only)
- ✅ Directory restriction
- ✅ Sanitized file paths

## 🐛 Troubleshooting

### Templates not showing?
- Check files are in correct folder
- Verify .html extension
- Refresh the page

### Can't save?
- Make sure you're logged in
- Check browser console for errors
- Verify template name is valid

### Preview not working?
- Check HTML syntax
- Look for JavaScript errors
- Try a different browser

## 📚 Next Steps

1. **Add Navigation Link**: Add link to main menu
2. **Create More Templates**: Build templates for your use cases
3. **Integrate with Popups**: Use templates in popup creation workflow
4. **Customize**: Modify styles and add features
5. **Test**: Try editing and creating templates

## 🎓 Learn More

- Full documentation: `TEMPLATE-SYSTEM-GUIDE.md`
- View sample templates in `Templates/` folders
- Check API: Try endpoints with Postman/curl

---

## ✨ You're All Set!

The template system is fully functional. Just run the app and navigate to:
**`/Template/SelectTemplate`**

Happy templating! 🎉
