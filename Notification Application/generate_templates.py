#!/usr/bin/env python3
"""
Template Generator Script for Notification Application
Generates professional popup templates based on industry specifications
"""

import os
import json
from datetime import datetime

# Color schemes
COLOR_SCHEMES = {
    'professional': {
        'primary': '#1e3a8a',
        'secondary': '#3b82f6',
        'accent': '#f59e0b',
        'background': '#ffffff',
        'text': '#1e293b'
    },
    'energetic': {
        'primary': '#dc2626',
        'secondary': '#f97316',
        'accent': '#fbbf24',
        'background': '#fff7ed',
        'text': '#292524'
    },
    'trustworthy': {
        'primary': '#0891b2',
        'secondary': '#10b981',
        'accent': '#06b6d4',
        'background': '#ecfeff',
        'text': '#164e63'
    },
    'healthcare': {
        'primary': '#0e7490',
        'secondary': '#06b6d4',
        'accent': '#67e8f9',
        'background': '#f0f9ff',
        'text': '#164e63'
    }
}

# Template specifications
TEMPLATE_SPECS = [
    {
        'industry': 'Plumbing',
        'category': 'HomeServices',
        'useCase': 'Maintenance',
        'style': 'Modern',
        'headline': 'Schedule Your Annual Plumbing Inspection',
        'subheadline': 'Prevent costly repairs with regular maintenance',
        'ctaText': 'Book Inspection',
        'ctaPhone': 'Call (555) 123-4567',
        'colorScheme': 'trustworthy',
        'formFields': ['name', 'email', 'phone', 'property_type'],
        'benefits': ['Licensed Plumbers', 'Same-Day Service', '100% Satisfaction', 'Upfront Pricing'],
        'icon': '🔧'
    },
    {
        'industry': 'Fashion',
        'category': 'Ecommerce',
        'useCase': 'SeasonalSale',
        'style': 'Vibrant',
        'headline': 'Spring Fashion Sale!',
        'subheadline': 'Up to 60% off new arrivals',
        'ctaText': 'Shop Now',
        'ctaPhone': None,
        'colorScheme': 'energetic',
        'formFields': ['email'],
        'benefits': ['Free Shipping', 'Easy Returns', 'Exclusive Access', 'Style Guarantee'],
        'icon': '👗'
    },
    {
        'industry': 'Legal',
        'category': 'ProfessionalServices',
        'useCase': 'Consultation',
        'style': 'Professional',
        'headline': 'Free Legal Consultation',
        'subheadline': 'Speak with an experienced attorney today',
        'ctaText': 'Schedule Call',
        'ctaPhone': 'Call (555) 234-5678',
        'colorScheme': 'professional',
        'formFields': ['name', 'email', 'phone', 'case_type'],
        'benefits': ['30+ Years Experience', 'No Upfront Costs', 'Confidential', 'Available 24/7'],
        'icon': '⚖️'
    },
    {
        'industry': 'Dental',
        'category': 'Healthcare',
        'useCase': 'NewPatient',
        'style': 'Modern',
        'headline': 'New Patient Special',
        'subheadline': 'Exam, X-Rays & Cleaning - Only $99',
        'ctaText': 'Book Appointment',
        'ctaPhone': 'Call (555) 345-6789',
        'colorScheme': 'healthcare',
        'formFields': ['name', 'email', 'phone', 'insurance'],
        'benefits': ['Most Insurance Accepted', 'Flexible Scheduling', 'Comfortable Environment', 'Family Dentistry'],
        'icon': '🦷'
    },
    {
        'industry': 'Fitness',
        'category': 'Healthcare',
        'useCase': 'FreeTrial',
        'style': 'Vibrant',
        'headline': '7-Day Free Trial Pass',
        'subheadline': 'Experience our state-of-the-art facility',
        'ctaText': 'Start Free Trial',
        'ctaPhone': None,
        'colorScheme': 'energetic',
        'formFields': ['name', 'email', 'phone', 'goals'],
        'benefits': ['Personal Training Included', 'Group Classes', 'No Commitment', 'All Equipment Access'],
        'icon': '💪'
    }
]

def generate_form_fields(fields, colors):
    """Generate HTML for form fields"""
    html = []
    
    for field in fields:
        if field == 'name':
            html.append(f'''
                <input type="text" 
                       name="name" 
                       placeholder="Your Name *" 
                       required 
                       style="width: 100%; padding: 12px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 1rem; margin-bottom: 15px;">''')
        elif field == 'email':
            html.append(f'''
                <input type="email" 
                       name="email" 
                       placeholder="Email Address *" 
                       required 
                       style="width: 100%; padding: 12px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 1rem; margin-bottom: 15px;">''')
        elif field == 'phone':
            html.append(f'''
                <input type="tel" 
                       name="phone" 
                       placeholder="Phone Number *" 
                       required 
                       style="width: 100%; padding: 12px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 1rem; margin-bottom: 15px;">''')
        else:
            label = field.replace('_', ' ').upper()
            html.append(f'''
                <input type="text" 
                       name="{field}" 
                       placeholder="{label}" 
                       style="width: 100%; padding: 12px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 1rem; margin-bottom: 15px;">''')
    
    return '\n'.join(html)

def generate_benefits(benefits, colors):
    """Generate HTML for benefits list"""
    html = []
    
    for benefit in benefits:
        html.append(f'''
                <div style="display: flex; align-items: center; margin-bottom: 12px;">
                    <div style="width: 8px; height: 8px; background: {colors['accent']}; border-radius: 50%; margin-right: 12px;"></div>
                    <span style="color: {colors['text']}; font-size: 0.95rem;">{benefit}</span>
                </div>''')
    
    return '\n'.join(html)

def generate_phone_section(phone, colors):
    """Generate HTML for phone section"""
    if not phone:
        return ''
    
    phone_clean = phone.replace('(', '').replace(')', '').replace(' ', '').replace('-', '')
    
    return f'''
            <div style="text-align: center; margin-top: 20px; padding-top: 20px; border-top: 2px solid #e2e8f0;">
                <div style="color: {colors['text']}; font-size: 0.9rem; margin-bottom: 8px;">Or call us directly:</div>
                <a href="tel:{phone_clean}" 
                   style="color: {colors['primary']}; font-size: 1.3rem; font-weight: 700; text-decoration: none;">
                    📞 {phone}
                </a>
            </div>'''

def generate_template_html(spec):
    """Generate complete HTML template"""
    colors = COLOR_SCHEMES[spec['colorScheme']]
    file_name = f"{spec['industry']}_{spec['useCase']}_{spec['style']}.html"
    
    form_fields_html = generate_form_fields(spec['formFields'], colors)
    benefits_html = generate_benefits(spec['benefits'], colors)
    phone_section = generate_phone_section(spec.get('ctaPhone'), colors)
    
    html = f'''<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{spec['headline']} - {spec['industry']}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        }}
        .popup-container {{
            background: {colors['background']};
            border-radius: 16px;
            max-width: 500px;
            width: 90%;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            position: relative;
            overflow: hidden;
        }}
        .close-btn {{
            position: absolute;
            top: 15px;
            right: 15px;
            background: rgba(0, 0, 0, 0.1);
            border: none;
            border-radius: 50%;
            width: 32px;
            height: 32px;
            font-size: 1.2rem;
            cursor: pointer;
            color: {colors['text']};
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s ease;
            z-index: 10;
        }}
        .close-btn:hover {{
            background: rgba(0, 0, 0, 0.2);
            transform: rotate(90deg);
        }}
        @media (max-width: 768px) {{
            .popup-container {{
                max-width: 95%;
                border-radius: 12px;
            }}
        }}
    </style>
</head>
<body>
    <div class="popup-container">
        <button class="close-btn" onclick="window.parent.postMessage('closePopup', '*')">×</button>
        
        <div style="background: linear-gradient(135deg, {colors['primary']} 0%, {colors['secondary']} 100%); padding: 40px 30px; text-align: center;">
            <div style="font-size: 3.5rem; margin-bottom: 15px;">{spec['icon']}</div>
            <h1 style="color: white; font-size: 1.8rem; font-weight: 800; margin-bottom: 10px; line-height: 1.2;">
                {spec['headline']}
            </h1>
            <p style="color: rgba(255, 255, 255, 0.95); font-size: 1.1rem; font-weight: 500;">
                {spec['subheadline']}
            </p>
        </div>
        
        <div style="padding: 30px;">
            <div style="margin-bottom: 25px;">
{benefits_html}
            </div>
            
            <form onsubmit="handleSubmit(event)" style="margin-top: 25px;">
{form_fields_html}
                
                <button type="submit" 
                        style="width: 100%; padding: 16px; background: linear-gradient(135deg, {colors['primary']} 0%, {colors['secondary']} 100%); color: white; border: none; border-radius: 10px; font-size: 1.1rem; font-weight: 700; cursor: pointer; transition: all 0.3s ease; box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);">
                    {spec['ctaText']} →
                </button>
            </form>
            {phone_section}
            
            <div style="text-align: center; margin-top: 20px; padding-top: 15px;">
                <p style="color: #64748b; font-size: 0.8rem;">
                    🔒 Your information is secure and will never be shared.
                </p>
            </div>
        </div>
    </div>
    
    <script>
        function handleSubmit(event) {{
            event.preventDefault();
            const formData = new FormData(event.target);
            const data = Object.fromEntries(formData.entries());
            
            // Send to parent window
            window.parent.postMessage({{
                type: 'formSubmit',
                template: '{file_name}',
                data: data
            }}, '*');
            
            // Show success message
            event.target.innerHTML = '<div style="text-align: center; padding: 40px;"><h2 style="color: {colors['primary']}; margin-bottom: 15px;">✓ Thank You!</h2><p style="color: {colors['text']};">We\\'ll be in touch shortly.</p></div>';
        }}
    </script>
</body>
</html>'''
    
    return file_name, html

def main():
    """Main execution"""
    print('🚀 Template Generator Starting...')
    print()
    
    output_path = './Templates'
    generated = []
    
    for i, spec in enumerate(TEMPLATE_SPECS):
        progress = int((i + 1) / len(TEMPLATE_SPECS) * 100)
        print(f"[{progress}%] Generating: {spec['industry']} - {spec['useCase']} ({spec['style']})")
        
        try:
            file_name, html = generate_template_html(spec)
            category_path = os.path.join(output_path, spec['category'])
            
            # Create directory if it doesn't exist
            os.makedirs(category_path, exist_ok=True)
            
            file_path = os.path.join(category_path, file_name)
            
            # Write file
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(html)
            
            generated.append({
                'fileName': file_name,
                'category': spec['category'],
                'filePath': file_path
            })
            
            print(f"  ✓ Created: {file_path}")
        except Exception as e:
            print(f"  ✗ Error: {e}")
    
    print()
    print(f'✅ Template Generation Complete!')
    print(f'   Total Generated: {len(generated)}')
    print()
    print('📊 Breakdown by Category:')
    
    # Group by category
    categories = {}
    for item in generated:
        cat = item['category']
        categories[cat] = categories.get(cat, 0) + 1
    
    for cat, count in categories.items():
        print(f'   {cat}: {count} templates')
    
    print()
    print('💡 Next Steps:')
    print('   1. Review generated templates in ./Templates folders')
    print('   2. Update DatabaseSeeder.cs with new templates')
    print('   3. Restart application to see templates in gallery')
    print('   4. Test each template in the designer')
    print()

if __name__ == '__main__':
    main()
