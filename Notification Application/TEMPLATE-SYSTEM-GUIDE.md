# Template System Documentation

## Overview
The Template System allows you to manage, edit, and organize HTML templates across different categories for your popup notifications.

## Features
✅ **7 Template Categories**: Popup, Select Template, Floating Bar, Fullscreen, Inline, Slide-in, Gamified
✅ **Live Editor**: Edit HTML templates with live preview
✅ **Template Gallery**: Browse all templates with thumbnails
✅ **Easy Management**: Create, edit, delete templates through UI
✅ **Sample Templates**: Pre-built templates for each category

## Folder Structure
```
Templates/
├── Popup/                  # Modal popup templates
│   ├── basic-modal.html
│   └── discount-offer.html
├── SelectTemplate/         # Template selection templates
├── FloatingBar/           # Top/bottom bar templates
│   ├── announcement-bar.html
│   └── countdown-timer.html
├── Fullscreen/            # Full screen overlay templates
│   └── fullscreen-welcome.html
├── Inline/                # Inline form templates
│   └── inline-signup.html
├── SlideIn/               # Slide-in notification templates
│   └── slide-in-notification.html
└── Gamified/              # Interactive templates
    └── spin-the-wheel.html
```

## How to Use

### Accessing the Template Gallery
1. Navigate to `/Template/SelectTemplate` in your application
2. Login with your credentials (authentication required)

### Browsing Templates
1. Click on category tabs to view templates in that category
2. Hover over a template card to see preview/edit options
3. Click "Preview" to see full template rendering
4. Click "Edit" to modify the template

### Editing Templates
1. Click the "Edit" button on any template
2. Modify the HTML code in the left panel
3. See live preview in the right panel
4. Click "Save Template" to persist changes

### Creating New Templates
1. Select a category tab
2. If empty, click "Create New Template"
3. Enter template name (without .html extension)
4. Write your HTML code
5. Save the template

### Using Templates
1. Click "Use This" button on any template
2. Template will be applied to popup creation
3. You'll be redirected to the popup creator

### Deleting Templates
1. Click the trash icon on template card
2. Confirm deletion
3. Template will be removed permanently

## API Endpoints

### Get Templates by Category
```
GET /api/templates/{category}
Returns: Array of template info objects
```

### Get Template Content
```
GET /api/templates/{category}/{templateName}
Returns: { content: "HTML content" }
```

### Save Template
```
POST /api/templates/{category}/{templateName}
Body: { content: "HTML content" }
Returns: { success: true/false, message: "..." }
```

### Delete Template
```
DELETE /api/templates/{category}/{templateName}
Returns: { success: true/false, message: "..." }
```

## Template Categories

### 1. **Popup**
Modal popups that appear centered on screen
- Best for: Email capture, announcements, special offers

### 2. **Select Template**
Custom selection templates
- Best for: Choice-based interactions

### 3. **Floating Bar**
Top or bottom bars that stay visible
- Best for: Announcements, countdowns, promo codes

### 4. **Fullscreen**
Full-screen overlays
- Best for: Welcome screens, major announcements

### 5. **Inline**
Embedded within page content
- Best for: Newsletter signups, content upgrades

### 6. **Slide-in**
Slides from corner of screen
- Best for: Cart reminders, product alerts

### 7. **Gamified**
Interactive, game-like templates
- Best for: Engagement, fun discounts

## Adding Custom Templates

### Manual Method
1. Create a new `.html` file
2. Add your HTML, CSS, and inline JavaScript
3. Place file in appropriate category folder:
   - `/Notification Application/Templates/{CategoryFolder}/your-template.html`

### UI Method
1. Go to Template Gallery
2. Select category
3. Click "Create New Template"
4. Enter name and code
5. Save

## Template Guidelines

### Structure
```html
<div class="your-template">
    <!-- Your HTML content -->
</div>

<style>
/* Your CSS styles */
</style>

<script>
// Your JavaScript (optional)
</script>
```

### Best Practices
- ✅ Use inline styles for portability
- ✅ Keep CSS scoped to your template
- ✅ Include close buttons for popups
- ✅ Make responsive with media queries
- ✅ Use semantic HTML
- ✅ Add animations for engagement
- ❌ Avoid external dependencies
- ❌ Don't use global CSS selectors

## Security

The Template Service includes:
- Path traversal protection
- HTML file validation
- Directory restriction checks
- Authorized access only

## Troubleshooting

### Templates not loading
- Check file exists in correct folder
- Verify file has `.html` extension
- Check folder permissions

### Edit not saving
- Ensure you're logged in
- Check console for errors
- Verify content is not empty

### Preview not working
- Check HTML syntax
- Look for JavaScript errors in console
- Try refreshing the page

## Technical Details

### Technologies
- **Backend**: ASP.NET Core (.NET 9)
- **Frontend**: Bootstrap 5, Vanilla JavaScript
- **Storage**: File system based

### Services
- `ITemplateService`: Interface for template operations
- `TemplateService`: Implementation with file system access
- `TemplateController`: API endpoints and view controller

### Models
- `TemplateInfo`: Template metadata
- `TemplateCategory`: Category information
- `SaveTemplateRequest`: Save operation DTO

## Future Enhancements
- [ ] Template versioning
- [ ] Template marketplace
- [ ] Drag-and-drop editor
- [ ] Template preview thumbnails
- [ ] Import/export templates
- [ ] Template analytics
- [ ] A/B testing support

## Support
For issues or questions, contact your development team or refer to the main application documentation.
