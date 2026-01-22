# 🎨 Template to Designer Integration Guide

## ✨ What's New

When you click **"Use This"** on any template, it now opens the **drag-and-drop Designer** with the template HTML pre-loaded, allowing you to customize it before creating your popup!

## 🚀 How It Works

### Step 1: Browse Templates
1. Login at `http://localhost:5117`
2. Click **"Templates"** in the navigation menu
3. Browse through the 7 categories:
   - Popup
   - Floating Bar
   - Fullscreen
   - Inline
   - Slide-in
   - Gamified
   - Select Template

### Step 2: Select a Template
- **Preview**: 👁️ View full-screen preview
- **Edit**: ✏️ Edit the template code
- **Use This**: ✓ **NEW!** Opens in Designer for customization
- **Delete**: 🗑️ Remove template

### Step 3: Customize in Designer
When you click **"Use This"**:
1. Designer opens with template HTML loaded
2. The template appears in the canvas area
3. You can now:
   - **Drag & Drop** new blocks
   - **Edit** existing content by clicking elements
   - **Style** with the properties panel
   - **Preview** desktop/mobile views
   - **Save** as a new popup campaign

## 🎯 Designer Features

### Visual Editor
- **Drag & Drop Interface**: Add blocks from the left sidebar
- **Component Selection**: Click any element to edit
- **Live Preview**: See changes in real-time
- **Device Toggle**: Switch between desktop and mobile views

### Available Blocks

#### Standard Blocks
- Heading
- Text
- Button
- Image
- Icon
- Input Field
- List
- Columns
- Divider
- Spacer
- Video
- Alert

#### Smart Blocks
- Countdown Timer
- Progress Bar
- Star Rating
- Testimonial
- Badge
- Card
- Social Share
- Coupon
- Form
- Accordion
- Map
- HTML

### Properties Panel
- Edit text content
- Change colors
- Adjust sizes
- Set padding/margins
- Configure links
- Upload images

### Display Rules
- **Triggers**: Page load, time delay, exit intent, scroll depth
- **Frequency**: Once per session, always show, once per day
- **Targeting**: Specific URLs
- **Device**: Desktop, mobile, or both

### Integrations
- Email service providers
- Zapier webhooks
- Custom webhooks

## 🔄 Workflow Example

### Using the "Discount Offer" Template

1. **Select Template**
   ```
   Templates → Popup → discount-offer.html → Click "Use This"
   ```

2. **Opens in Designer**
   - Template loads with gradient design and "SAVE25" coupon
   - All HTML/CSS is pre-loaded and editable

3. **Customize**
   - Click heading to change "Special Offer!"
   - Edit coupon code from "SAVE25" to your code
   - Change colors in properties panel
   - Drag new blocks if needed

4. **Configure**
   - Switch to "Display Rules" tab
   - Set trigger: Exit Intent
   - Set frequency: Once per session
   - Target specific URLs

5. **Save**
   - Click "Save" button
   - Campaign is created and ready to activate

## 🎨 Template Categories

### Popup Templates (10)
- Basic Modal
- Discount Offer
- Exit Intent
- Newsletter Minimal
- Video Popup
- Survey Feedback
- Ebook Download
- Flash Sale
- Webinar Registration
- Social Proof
- Free Trial

### Floating Bar Templates (2)
- Announcement Bar
- Countdown Timer

### Fullscreen Templates (1)
- Welcome Screen

### Inline Templates (1)
- Inline Signup

### Slide-in Templates (1)
- Corner Notification

### Gamified Templates (1)
- Spin the Wheel

## 💡 Pro Tips

### Editing Templates
1. **Template Library**: Templates are stored in `/Templates/{Category}/` folder
2. **File Format**: HTML files with inline CSS and JavaScript
3. **Edit Options**:
   - Use "Edit" button for code editing
   - Use "Use This" button for visual editing

### Customization Best Practices
1. **Start with a template** - Faster than building from scratch
2. **Use the Designer** - Visual editing is easier
3. **Test responsive** - Toggle desktop/mobile views
4. **Save frequently** - Don't lose your work
5. **Preview before publishing** - Use the preview button

### Designer Shortcuts
- **Drag & Drop**: Add blocks quickly
- **Click to Select**: Edit any element
- **Properties Panel**: Fine-tune styling
- **Device Toggle**: Check responsiveness
- **Tab Navigation**: Switch between Design/Rules/Integrations

## 🔧 Technical Details

### How Templates Load
```javascript
// User clicks "Use This" on template
useTemplate(category, templateName) {
    // Redirects to Designer with template parameters
    window.location.href = `/Popup/Designer?templateFile=${fileName}&category=${category}`;
}
```

### Backend Processing
```csharp
// Designer action loads template HTML
public async Task<IActionResult> Designer(string? templateFile, string? category)
{
    if (!string.IsNullOrEmpty(templateFile) && !string.IsNullOrEmpty(category))
    {
        var templatePath = Path.Combine("Templates", category, templateFile);
        popup.Content = await File.ReadAllTextAsync(templatePath);
        // Template HTML is now in popup.Content
    }
}
```

### Designer Loading
```javascript
// Designer checks if content is HTML or JSON
if (contentStr.includes('<') && contentStr.includes('>')) {
    // Plain HTML from template - load directly
    document.getElementById('dropzone').innerHTML = contentStr;
} else {
    // JSON from saved popup - parse first
    const parsed = JSON.parse(contentStr);
    document.getElementById('dropzone').innerHTML = parsed.html;
}
```

## 🎯 Use Cases

### 1. Quick Customization
- Select template closest to your goal
- Change text and colors
- Save immediately

### 2. Template as Starting Point
- Load template in Designer
- Add/remove blocks
- Rebuild layout
- Save as new design

### 3. Template Learning
- Open template in Designer
- Study structure
- Modify elements
- Learn best practices

### 4. A/B Testing
- Use same template multiple times
- Make small variations
- Test different versions
- Compare analytics

## 📋 Checklist

Before publishing your customized popup:

- [ ] Tested desktop view
- [ ] Tested mobile view
- [ ] Set appropriate trigger
- [ ] Configured frequency
- [ ] Added URL targeting (if needed)
- [ ] Connected integrations (if needed)
- [ ] Preview looks good
- [ ] Text is error-free
- [ ] Links work correctly
- [ ] Form fields are correct
- [ ] Call-to-action is clear

## 🚀 Getting Started

1. **Login**: info@vertexsofts.com / Workload11
2. **Navigate**: Click "Templates" in menu
3. **Browse**: Explore the template gallery
4. **Select**: Find a template you like
5. **Click**: "Use This" button
6. **Customize**: In the Designer
7. **Save**: Create your popup
8. **Activate**: Start collecting leads!

## 🔗 Related Documentation

- [TEMPLATE-SYSTEM-GUIDE.md](TEMPLATE-SYSTEM-GUIDE.md) - Template management
- [HOW-TO-ACCESS.md](HOW-TO-ACCESS.md) - Access instructions
- [IMPLEMENTATION-STATUS.md](IMPLEMENTATION-STATUS.md) - Current status

---

**Need Help?** All templates come with clean, documented code. Use the "Edit" button to see the source!
