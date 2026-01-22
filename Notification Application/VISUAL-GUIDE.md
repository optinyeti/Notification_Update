# 🎨 Template System - Visual Guide

## 📸 What You'll See

### 1. Template Gallery Page
```
┌─────────────────────────────────────────────────────────────┐
│  🎨 Template Gallery                                        │
│  Browse and customize templates for your popups             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Popup] [Select Template] [Floating Bar] [Fullscreen]    │
│  [Inline] [Slide-in] [Gamified]                           │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                │
│  │          │  │          │  │          │                │
│  │ Template │  │ Template │  │ Template │                │
│  │    1     │  │    2     │  │    3     │                │
│  │          │  │          │  │          │                │
│  ├──────────┤  ├──────────┤  ├──────────┤                │
│  │ Title    │  │ Title    │  │ Title    │                │
│  │ Modified │  │ Modified │  │ Modified │                │
│  ├──────────┤  ├──────────┤  ├──────────┤                │
│  │[Use][Del]│  │[Use][Del]│  │[Use][Del]│                │
│  └──────────┘  └──────────┘  └──────────┘                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 2. Template Editor Modal
```
┌─────────────────────────────────────────────────────────────┐
│  📝 Template Editor                                    [X]   │
├──────────────────────┬──────────────────────────────────────┤
│  HTML Code           │  Live Preview                        │
│                      │                                      │
│  <div class="popup"> │  ┌────────────────────────────┐     │
│    <h2>Title</h2>    │  │                            │     │
│    <p>Text</p>       │  │   Your Template            │     │
│    <form>            │  │                            │     │
│      ...             │  │   [Preview Here]           │     │
│    </form>           │  │                            │     │
│  </div>              │  │                            │     │
│                      │  └────────────────────────────┘     │
│  <style>             │                                      │
│    .popup { }        │                                      │
│  </style>            │                                      │
│                      │                                      │
├──────────────────────┴──────────────────────────────────────┤
│                              [Cancel] [Save Template]       │
└─────────────────────────────────────────────────────────────┘
```

## 🎯 User Journey

### Browsing Templates
```
1. Visit /Template/SelectTemplate
   ↓
2. Click category tab (e.g., "Popup")
   ↓
3. See all templates in that category
   ↓
4. Hover over template card
   ↓
5. Options appear: [Preview] [Edit]
```

### Editing Template
```
1. Click "Edit" on template card
   ↓
2. Editor modal opens
   ↓
3. Modify HTML in left pane
   ↓
4. See live preview in right pane
   ↓
5. Click "Save Template"
   ↓
6. Success notification appears
   ↓
7. Modal closes, gallery refreshes
```

### Creating New Template
```
1. Go to category with no templates
   ↓
2. See "No templates found" message
   ↓
3. Click "Create New Template"
   ↓
4. Enter template name
   ↓
5. Editor opens with starter code
   ↓
6. Write your template
   ↓
7. Save
   ↓
8. Template appears in gallery
```

## 🖼️ Sample Templates Preview

### 1. Basic Modal (Popup)
```
┌─────────────────────────────────┐
│              [X]                │
│                                 │
│   Subscribe to Newsletter       │
│                                 │
│   Get latest updates...         │
│                                 │
│   [_____________________]       │
│   [   Subscribe Now   ]         │
│                                 │
│   We respect your privacy       │
└─────────────────────────────────┘
```

### 2. Discount Offer (Popup)
```
┌─────────────────────────────────┐
│              [X]                │
│         ┌─────────┐             │
│         │ 20% OFF │             │
│         └─────────┘             │
│                                 │
│      Special Offer!             │
│   Sign up and get 20% off      │
│                                 │
│   [_____________________]       │
│   [ Claim Your Discount ]       │
└─────────────────────────────────┘
```

### 3. Announcement Bar (Floating)
```
┌──────────────────────────────────────────┐
│ [NEW] Limited Time: Get 50% off! [X]    │
└──────────────────────────────────────────┘
```

### 4. Countdown Timer (Floating)
```
┌──────────────────────────────────────────────┐
│ ⚡ Flash Sale: [00:00:00] [Shop Now] [X]   │
└──────────────────────────────────────────────┘
```

### 5. Fullscreen Welcome
```
┌─────────────────────────────────────────┐
│                    [X]                  │
│                                         │
│              [ICON]                     │
│                                         │
│        Welcome to Our Store!            │
│   Join thousands of happy customers     │
│                                         │
│   ✓ Free Shipping                       │
│   ✓ 30-Day Returns                      │
│   ✓ 24/7 Support                        │
│                                         │
│   [_____________________]               │
│   [    Get Started    ]                 │
│                                         │
└─────────────────────────────────────────┘
```

### 6. Inline Signup
```
┌─────────────────────────────────────────┐
│                                         │
│         Stay in the Loop                │
│   Subscribe to our newsletter           │
│                                         │
│   [____________] [Subscribe]            │
│                                         │
│   📧 Weekly  🎁 Deals  🔒 Private      │
│                                         │
└─────────────────────────────────────────┘
```

### 7. Slide-in Notification
```
                    ┌──────────────────┐
                    │ [X]              │
                    │ [IMG] New Alert! │
                    │                  │
                    │ Check our latest │
                    │ collection!      │
                    │                  │
                    │ [  View Now  ]   │
                    └──────────────────┘
```

### 8. Spin the Wheel (Gamified)
```
┌─────────────────────────────────┐
│              [X]                │
│                                 │
│       Spin to Win!              │
│   Try your luck and win         │
│                                 │
│        ▼                        │
│      ┌───┐                      │
│      │ O │ (wheel graphic)      │
│      └───┘                      │
│                                 │
│   [ Spin the Wheel ]            │
│                                 │
│   [_____________________]       │
│   [   Claim Prize    ]          │
└─────────────────────────────────┘
```

## 🎨 Template Structure

### Every Template Contains:
```html
<!-- 1. HTML Structure -->
<div class="template-wrapper">
    <!-- Your content -->
</div>

<!-- 2. Inline CSS -->
<style>
    .template-wrapper {
        /* Your styles */
    }
</style>

<!-- 3. Optional JavaScript -->
<script>
    // Your interactions
</script>
```

## 🎭 Interactions

### Hover Effects
- Template cards lift on hover
- Overlay buttons appear
- Smooth transitions

### Click Actions
| Button | Action |
|--------|--------|
| **Preview** | Opens preview modal |
| **Edit** | Opens editor modal |
| **Use This** | Applies to popup creator |
| **Delete** | Confirms and removes |

### Keyboard Shortcuts
- `Esc` - Close modal
- `Ctrl/Cmd + S` - Save template (in editor)

## 🎨 Color Scheme

### Template Cards
- Background: White
- Border: Light gray
- Shadow: Soft on hover

### Category Tabs
- Inactive: Gray text
- Hover: Light blue background
- Active: Gradient purple/blue

### Buttons
- Primary: Blue gradient
- Success: Green
- Danger: Red
- Secondary: Gray

## 📱 Responsive Design

### Desktop (> 992px)
```
┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐
│ Tmpl │ │ Tmpl │ │ Tmpl │ │ Tmpl │
└──────┘ └──────┘ └──────┘ └──────┘
```

### Tablet (768px - 991px)
```
┌──────┐ ┌──────┐ ┌──────┐
│ Tmpl │ │ Tmpl │ │ Tmpl │
└──────┘ └──────┘ └──────┘
```

### Mobile (< 768px)
```
┌──────────┐
│ Template │
└──────────┘
┌──────────┐
│ Template │
└──────────┘
```

## 🔔 Notifications

### Success
```
┌────────────────────────────┐
│ ✅ Template saved!        │
└────────────────────────────┘
```

### Error
```
┌────────────────────────────┐
│ ❌ Error saving template  │
└────────────────────────────┘
```

### Info
```
┌────────────────────────────┐
│ ℹ️ Redirecting...         │
└────────────────────────────┘
```

## 🚀 Getting Started

### Step 1: Access
Navigate to: `http://localhost:5000/Template/SelectTemplate`

### Step 2: Explore
Click through category tabs to see templates

### Step 3: Customize
Click "Edit" on any template to modify

### Step 4: Create
Click "Create New Template" in any category

### Step 5: Use
Click "Use This" to apply template to popup

---

**Enjoy your new template system! 🎉**
