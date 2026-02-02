#!/usr/bin/env pwsh
# Template Generator Script for Notification Application
# Generates professional popup templates based on industry specifications

param(
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "./Templates",
    
    [Parameter(Mandatory=$false)]
    [int]$BatchSize = 50
)

# Color schemes
$ColorSchemes = @{
    professional = @{
        primary = "#1e3a8a"
        secondary = "#3b82f6"
        accent = "#f59e0b"
        background = "#ffffff"
        text = "#1e293b"
    }
    energetic = @{
        primary = "#dc2626"
        secondary = "#f97316"
        accent = "#fbbf24"
        background = "#fff7ed"
        text = "#292524"
    }
    trustworthy = @{
        primary = "#0891b2"
        secondary = "#10b981"
        accent = "#06b6d4"
        background = "#ecfeff"
        text = "#164e63"
    }
    luxury = @{
        primary = "#78350f"
        secondary = "#eab308"
        accent = "#fef3c7"
        background = "#1c1917"
        text = "#fafaf9"
    }
    healthcare = @{
        primary = "#0e7490"
        secondary = "#06b6d4"
        accent = "#67e8f9"
        background = "#f0f9ff"
        text = "#164e63"
    }
}

# Icon mapping
$Icons = @{
    home_services = "🏠"
    plumbing = "🔧"
    hvac = "❄️"
    electrical = "⚡"
    landscaping = "🌳"
    ecommerce = "🛒"
    fashion = "👗"
    electronics = "📱"
    beauty = "💄"
    legal = "⚖️"
    accounting = "📊"
    real_estate = "🏘️"
    insurance = "🛡️"
    medical = "🏥"
    dental = "🦷"
    fitness = "💪"
    education = "📚"
    saas = "💻"
    hospitality = "🏨"
    automotive = "🚗"
    nonprofit = "❤️"
}

# Template specifications
$TemplateSpecs = @(
    @{
        industry = "Plumbing"
        category = "HomeServices"
        useCase = "Maintenance"
        style = "Modern"
        headline = "Schedule Your Annual Plumbing Inspection"
        subheadline = "Prevent costly repairs with regular maintenance"
        ctaText = "Book Inspection"
        ctaPhone = "Call (555) 123-4567"
        colorScheme = "trustworthy"
        formFields = @("name", "email", "phone", "property_type")
        benefits = @("Licensed Plumbers", "Same-Day Service", "100% Satisfaction", "Upfront Pricing")
        icon = "🔧"
    },
    @{
        industry = "Fashion"
        category = "Ecommerce"
        useCase = "SeasonalSale"
        style = "Vibrant"
        headline = "Spring Fashion Sale!"
        subheadline = "Up to 60% off new arrivals"
        ctaText = "Shop Now"
        ctaPhone = $null
        colorScheme = "energetic"
        formFields = @("email")
        benefits = @("Free Shipping", "Easy Returns", "Exclusive Access", "Style Guarantee")
        icon = "👗"
    },
    @{
        industry = "Legal"
        category = "ProfessionalServices"
        useCase = "Consultation"
        style = "Professional"
        headline = "Free Legal Consultation"
        subheadline = "Speak with an experienced attorney today"
        ctaText = "Schedule Call"
        ctaPhone = "Call (555) 234-5678"
        colorScheme = "professional"
        formFields = @("name", "email", "phone", "case_type")
        benefits = @("30+ Years Experience", "No Upfront Costs", "Confidential", "Available 24/7")
        icon = "⚖️"
    },
    @{
        industry = "Dental"
        category = "Healthcare"
        useCase = "NewPatient"
        style = "Modern"
        headline = "New Patient Special"
        subheadline = "Exam, X-Rays & Cleaning - Only $99"
        ctaText = "Book Appointment"
        ctaPhone = "Call (555) 345-6789"
        colorScheme = "healthcare"
        formFields = @("name", "email", "phone", "insurance")
        benefits = @("Most Insurance Accepted", "Flexible Scheduling", "Comfortable Environment", "Family Dentistry")
        icon = "🦷"
    },
    @{
        industry = "Fitness"
        category = "Healthcare"
        useCase = "FreeTrial"
        style = "Vibrant"
        headline = "7-Day Free Trial Pass"
        subheadline = "Experience our state-of-the-art facility"
        ctaText = "Start Free Trial"
        ctaPhone = $null
        colorScheme = "energetic"
        formFields = @("name", "email", "phone", "goals")
        benefits = @("Personal Training Included", "Group Classes", "No Commitment", "All Equipment Access")
        icon = "💪"
    }
)

function Generate-TemplateHTML {
    param(
        [hashtable]$spec
    )
    
    $colors = $ColorSchemes[$spec.colorScheme]
    $fileName = "$($spec.industry)_$($spec.useCase)_$($spec.style).html"
    $filePath = Join-Path $OutputPath $spec.category
    
    # Create directory if it doesn't exist
    if (-not (Test-Path $filePath)) {
        New-Item -ItemType Directory -Path $filePath -Force | Out-Null
    }
    
    $fullPath = Join-Path $filePath $fileName
    
    # Build form fields HTML
    $formFieldsHTML = ""
    foreach ($field in $spec.formFields) {
        switch ($field) {
            "name" {
                $formFieldsHTML += @"
                        <input type="text" 
                               name="name" 
                               placeholder="Your Name *" 
                               required 
                               style="width: 100%; padding: 12px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 1rem; margin-bottom: 15px;">
"@
            }
            "email" {
                $formFieldsHTML += @"
                        <input type="email" 
                               name="email" 
                               placeholder="Email Address *" 
                               required 
                               style="width: 100%; padding: 12px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 1rem; margin-bottom: 15px;">
"@
            }
            "phone" {
                $formFieldsHTML += @"
                        <input type="tel" 
                               name="phone" 
                               placeholder="Phone Number *" 
                               required 
                               style="width: 100%; padding: 12px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 1rem; margin-bottom: 15px;">
"@
            }
            default {
                $label = ($field -replace '_', ' ').ToUpper()
                $formFieldsHTML += @"
                        <input type="text" 
                               name="$field" 
                               placeholder="$label" 
                               style="width: 100%; padding: 12px; border: 2px solid #e2e8f0; border-radius: 8px; font-size: 1rem; margin-bottom: 15px;">
"@
            }
        }
    }
    
    # Build benefits HTML
    $benefitsHTML = ""
    foreach ($benefit in $spec.benefits) {
        $benefitsHTML += @"
                    <div style="display: flex; align-items: center; margin-bottom: 12px;">
                        <div style="width: 8px; height: 8px; background: $($colors.accent); border-radius: 50%; margin-right: 12px;"></div>
                        <span style="color: $($colors.text); font-size: 0.95rem;">$benefit</span>
                    </div>
"@
    }
    
    # Phone section (if applicable)
    $phoneSection = ""
    if ($spec.ctaPhone) {
        $phoneSection = @"
                <div style="text-align: center; margin-top: 20px; padding-top: 20px; border-top: 2px solid #e2e8f0;">
                    <div style="color: $($colors.text); font-size: 0.9rem; margin-bottom: 8px;">Or call us directly:</div>
                    <a href="tel:$($spec.ctaPhone.Replace('(', '').Replace(')', '').Replace(' ', '').Replace('-', ''))" 
                       style="color: $($colors.primary); font-size: 1.3rem; font-weight: 700; text-decoration: none;">
                        📞 $($spec.ctaPhone)
                    </a>
                </div>
"@
    }
    
    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>$($spec.headline) - $($spec.industry)</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        }
        .popup-container {
            background: $($colors.background);
            border-radius: 16px;
            max-width: 500px;
            width: 90%;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            position: relative;
            overflow: hidden;
        }
        .close-btn {
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
            color: $($colors.text);
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s ease;
            z-index: 10;
        }
        .close-btn:hover {
            background: rgba(0, 0, 0, 0.2);
            transform: rotate(90deg);
        }
        @media (max-width: 768px) {
            .popup-container {
                max-width: 95%;
                border-radius: 12px;
            }
        }
    </style>
</head>
<body>
    <div class="popup-container">
        <button class="close-btn" onclick="window.parent.postMessage('closePopup', '*')">×</button>
        
        <div style="background: linear-gradient(135deg, $($colors.primary) 0%, $($colors.secondary) 100%); padding: 40px 30px; text-align: center;">
            <div style="font-size: 3.5rem; margin-bottom: 15px;">$($spec.icon)</div>
            <h1 style="color: white; font-size: 1.8rem; font-weight: 800; margin-bottom: 10px; line-height: 1.2;">
                $($spec.headline)
            </h1>
            <p style="color: rgba(255, 255, 255, 0.95); font-size: 1.1rem; font-weight: 500;">
                $($spec.subheadline)
            </p>
        </div>
        
        <div style="padding: 30px;">
            <div style="margin-bottom: 25px;">
                $benefitsHTML
            </div>
            
            <form onsubmit="handleSubmit(event)" style="margin-top: 25px;">
                $formFieldsHTML
                
                <button type="submit" 
                        style="width: 100%; padding: 16px; background: linear-gradient(135deg, $($colors.primary) 0%, $($colors.secondary) 100%); color: white; border: none; border-radius: 10px; font-size: 1.1rem; font-weight: 700; cursor: pointer; transition: all 0.3s ease; box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);">
                    $($spec.ctaText) →
                </button>
            </form>
            
            $phoneSection
            
            <div style="text-align: center; margin-top: 20px; padding-top: 15px;">
                <p style="color: #64748b; font-size: 0.8rem;">
                    🔒 Your information is secure and will never be shared.
                </p>
            </div>
        </div>
    </div>
    
    <script>
        function handleSubmit(event) {
            event.preventDefault();
            const formData = new FormData(event.target);
            const data = Object.fromEntries(formData.entries());
            
            // Send to parent window
            window.parent.postMessage({
                type: 'formSubmit',
                template: '$fileName',
                data: data
            }, '*');
            
            // Show success message
            event.target.innerHTML = '<div style="text-align: center; padding: 40px;"><h2 style="color: $($colors.primary); margin-bottom: 15px;">✓ Thank You!</h2><p style="color: $($colors.text);">We\'ll be in touch shortly.</p></div>';
        }
    </script>
</body>
</html>
"@
    
    # Write to file
    Set-Content -Path $fullPath -Value $html -Encoding UTF8
    
    return @{
        FileName = $fileName
        Category = $spec.category
        FilePath = $fullPath
        Spec = $spec
    }
}

# Main execution
Write-Host "🚀 Template Generator Starting..." -ForegroundColor Cyan
Write-Host ""

$generated = @()
$totalSpecs = $TemplateSpecs.Count

for ($i = 0; $i -lt $totalSpecs; $i++) {
    $spec = $TemplateSpecs[$i]
    $progress = [math]::Round(($i + 1) / $totalSpecs * 100)
    
    Write-Host "[$progress%] Generating: $($spec.industry) - $($spec.useCase) ($($spec.style))" -ForegroundColor Green
    
    try {
        $result = Generate-TemplateHTML -spec $spec
        $generated += $result
        Write-Host "  ✓ Created: $($result.FilePath)" -ForegroundColor Gray
    }
    catch {
        Write-Host "  ✗ Error: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "✅ Template Generation Complete!" -ForegroundColor Green
Write-Host "   Total Generated: $($generated.Count)" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Breakdown by Category:" -ForegroundColor Yellow
$generated | Group-Object -Property Category | ForEach-Object {
    Write-Host "   $($_.Name): $($_.Count) templates" -ForegroundColor White
}
Write-Host ""
Write-Host "💡 Next Steps:" -ForegroundColor Magenta
Write-Host "   1. Review generated templates in ./Templates folders"
Write-Host "   2. Update DatabaseSeeder.cs with new templates"
Write-Host "   3. Restart application to see templates in gallery"
Write-Host "   4. Test each template in the designer"
Write-Host ""
