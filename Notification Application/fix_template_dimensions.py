#!/usr/bin/env python3
"""
Fix template dimensions to match canvas sizes
- Floating Bar templates: 100% × 192px
- Popup (Lightbox) templates: 800px × 600px
- Fullscreen templates: 100% × 100%
- SlideIn templates: 400px × 100%
"""

import os
import re
from pathlib import Path

# Define dimension constants
DIMENSIONS = {
    'FloatingBar': 'width: 100%; height: 192px; min-height: 192px; max-height: 192px;',
    'Popup': 'width: 800px; height: 600px; min-width: 800px; max-width: 800px; min-height: 600px; max-height: 600px;',
    'Fullscreen': 'width: 100%; height: 100%; min-width: 100%; max-width: 100%; min-height: 100%; max-height: 100%;',
    'SlideIn': 'width: 400px; height: 100%; min-width: 400px; max-width: 400px; min-height: 100%; max-height: 100%;',
}

def fix_template_file(file_path, template_type):
    """Fix dimensions in a single template file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Find the first div with style attribute
        pattern = r'(<div[^>]*?class="[^"]*"[^>]*?style=")([^"]*?)(")'
        
        def replace_dimensions(match):
            import re as regex_module
            prefix = match.group(1)
            style = match.group(2)
            suffix = match.group(3)
            
            # Remove existing dimension properties
            style = regex_module.sub(r'max-width:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'width:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'height:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'min-width:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'max-width:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'min-height:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'max-height:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'position:\s*fixed;?\s*', '', style)
            style = regex_module.sub(r'top:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'left:\s*[^;]+;?\s*', '', style)
            style = regex_module.sub(r'z-index:\s*[^;]+;?\s*', '', style)
            
            # Clean up spacing
            style = regex_module.sub(r'\s+', ' ', style).strip()
            style = regex_module.sub(r';\s*;', ';', style)
            
            # Add box-sizing if not present
            if 'box-sizing' not in style:
                style += ' box-sizing: border-box;'
            
            # Insert new dimensions at the beginning
            new_style = DIMENSIONS[template_type] + ' ' + style
            
            return prefix + new_style + suffix
        
        # Replace only the first div (main wrapper)
        content = pattern.sub(replace_dimensions, content, count=1)
        
        # Write back if changed
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return True
        return False
        
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return False

def main():
    """Process all template files"""
    templates_dir = Path('/workspaces/Notification_Update/Notification Application/Templates')
    
    stats = {'processed': 0, 'updated': 0, 'skipped': 0}
    
    for template_type, dimensions in DIMENSIONS.items():
        type_dir = templates_dir / template_type
        if not type_dir.exists():
            continue
            
        print(f"\nProcessing {template_type} templates ({dimensions})...")
        
        for html_file in type_dir.glob('*.html'):
            stats['processed'] += 1
            if fix_template_file(html_file, template_type):
                stats['updated'] += 1
                print(f"  ✓ Updated: {html_file.name}")
            else:
                stats['skipped'] += 1
                print(f"  - Skipped: {html_file.name}")
    
    print(f"\n{'='*60}")
    print(f"Summary: {stats['processed']} processed, {stats['updated']} updated, {stats['skipped']} skipped")
    print(f"{'='*60}")

if __name__ == '__main__':
    main()
