# 🎨 Quick Start: Using Templates in Designer

## One-Click Template to Designer Workflow

```
📚 Template Gallery         →  🎨 Designer Editor        →  💾 Save Campaign
   (Browse & Preview)           (Customize & Edit)           (Activate & Use)
```

---

## 🚀 Step-by-Step Guide

### 1️⃣ Access Templates
Navigate to the template gallery:
```
http://localhost:5117/Template/SelectTemplate
```

Or click **"Templates"** in the navigation menu.

---

### 2️⃣ Browse & Preview

**What you'll see:**
- 7 category tabs (Popup, FloatingBar, Fullscreen, etc.)
- Template cards with **live HTML previews**
- Each template shows actual design (no blank boxes!)

**Available Actions:**
- 👁️ **Preview** - Full-screen view
- ✏️ **Edit** - Code editor
- ✓ **Use This** - **Opens Designer** ← NEW!
- 🗑️ **Delete** - Remove template

---

### 3️⃣ Click "Use This" Button

**What happens:**
1. Template file is loaded from disk
2. Designer opens automatically
3. Template HTML appears in canvas
4. Ready for customization!

**Example:**
```
Click: "Use This" on discount-offer.html
  ↓
Opens: /Popup/Designer?templateFile=discount-offer.html&category=Popup
  ↓
Result: Designer with discount popup loaded
```

---

### 4️⃣ Customize in Designer

#### Left Sidebar: Blocks
Drag & drop elements:
- **Standard**: Heading, Text, Button, Image, Icon, Input, List, etc.
- **Smart**: Countdown, Progress Bar, Rating, Testimonial, Coupon, etc.

#### Center Canvas: Visual Editor
- **Click** any element to select
- **Edit** properties in right panel
- **Drag** blocks to add
- **Delete** with toolbar button

#### Right Sidebar: Properties
Edit selected element:
- Text content
- Colors (background, text)
- Sizes (width, height, padding)
- Fonts & styles
- Links & actions

#### Top Tabs: Additional Settings
- **Design** - Visual editor (default)
- **Display Rules** - When to show popup
- **Integrations** - Connect services
- **Analytics** - View performance

---

### 5️⃣ Configure Display Rules

Switch to **"Display Rules"** tab:

**Trigger Options:**
- On Page Load
- Exit Intent
- On Scroll (%)
- Time Delay
- On Click
- On Idle

**Frequency Options:**
- Every Visit
- Once Per Session
- Once Per Day
- Once Per Week
- Once Per Month
- Once Ever

**Targeting:**
- URL patterns (one per line)
- Device (Desktop/Mobile/Both)

---

### 6️⃣ Save Campaign

Click **"Save"** button:
- Campaign is created
- Design is saved
- Ready to activate
- Returns to popup list

---

## 💡 Common Workflows

### A. Quick Customization (2 minutes)
```
1. Select template closest to goal
2. Click "Use This"
3. Change text & button copy
4. Adjust colors to match brand
5. Save campaign
```

### B. Moderate Editing (5-10 minutes)
```
1. Load template in Designer
2. Edit all text content
3. Replace images
4. Add/remove sections
5. Configure display rules
6. Test responsive views
7. Save campaign
```

### C. Major Redesign (15+ minutes)
```
1. Use template as starting point
2. Remove unwanted blocks
3. Add new blocks from sidebar
4. Restructure layout
5. Customize all properties
6. Set up integrations
7. Configure advanced rules
8. Preview thoroughly
9. Save campaign
```

---

## 🎯 Pro Tips

### 1. Start Simple
- Choose template close to final goal
- Make small changes first
- Test frequently

### 2. Use Preview
- Toggle desktop/mobile views
- Preview button shows full-screen
- Check responsiveness

### 3. Save Often
- Designer auto-saves to drafts
- Click Save to persist changes
- Don't lose your work!

### 4. Learn from Templates
- Open template in Edit mode to see code
- Study structure and styles
- Apply patterns to your designs

### 5. Test Display Rules
- Use "Once Per Session" during testing
- Switch to production rules before launch
- Consider user experience

---

## 🔄 Workflow Comparison

### Old Way ❌
```
1. Browse templates
2. Click "Edit" to see code
3. Copy HTML manually
4. Go to popup creation
5. Paste code
6. Hope it works
7. Fix issues
```

### New Way ✅
```
1. Browse templates
2. Click "Use This"
3. Customize visually
4. Save campaign
✨ Done!
```

---

## 📱 Example: Creating a Discount Popup

**Goal:** 20% off popup for new visitors

**Steps:**
```
1. Go to Templates → Popup category
2. Find "discount-offer.html"
3. Click "Use This"
4. Designer opens with coupon design
5. Edit heading: "Welcome! Get 20% Off"
6. Change coupon code: WELCOME20
7. Update button text: "Claim Discount"
8. Switch to Display Rules tab
9. Set trigger: On Page Load (3 second delay)
10. Set frequency: Once Ever
11. Enable: Desktop & Mobile
12. Click Save
13. Activate in popup list
✅ Live on your site!
```

**Time:** ~3 minutes

---

## 🎨 Available Templates

### Popup (10 templates)
1. Basic Modal - Simple newsletter signup
2. Discount Offer - Coupon code popup
3. Exit Intent - Catch leaving visitors
4. Newsletter Minimal - Clean design
5. Video Popup - YouTube embed
6. Survey Feedback - Rating system
7. Ebook Download - Lead magnet
8. Flash Sale - Countdown timer
9. Webinar Registration - Multi-field form
10. Social Proof - Testimonials
11. Free Trial - Feature list

### Other Categories (6 templates)
- FloatingBar: Announcement, Countdown (2)
- Fullscreen: Welcome Screen (1)
- Inline: Signup Form (1)
- SlideIn: Corner Notification (1)
- Gamified: Spin the Wheel (1)

---

## 🆘 Troubleshooting

### Template doesn't load?
- Check browser console for errors
- Verify template file exists
- Try different template

### Can't edit elements?
- Click element to select
- Properties appear in right panel
- Use component toolbar for actions

### Changes not saving?
- Click Save button in top right
- Wait for "Saved!" confirmation
- Check popup list for campaign

### Preview looks different?
- Clear browser cache
- Check responsive view toggle
- Test in different browser

---

## 🎊 Success!

You now have:
- ✅ Professional template library
- ✅ One-click Designer integration
- ✅ Visual customization tools
- ✅ Quick campaign creation

**Start creating beautiful popups in minutes, not hours!**

---

## 📚 Related Docs

- [TEMPLATE-TO-DESIGNER-GUIDE.md](TEMPLATE-TO-DESIGNER-GUIDE.md) - Detailed technical guide
- [IMPLEMENTATION-STATUS.md](IMPLEMENTATION-STATUS.md) - Current feature status
- [HOW-TO-ACCESS.md](HOW-TO-ACCESS.md) - Access & login info

---

**Login:** info@vertexsofts.com / Workload11  
**URL:** http://localhost:5117
