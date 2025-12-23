using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Models;

namespace Notification_Application.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed default tenant and admin user
        await SeedDefaultDataAsync(context, userManager);

        // Seed sample blog posts and content
        await SeedSampleContentAsync(context);

        // Seed popup templates
        await SeedPopupTemplatesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "Admin", "User" };

        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedDefaultDataAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        // Check if default tenant exists
        var defaultTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Domain == "default");
        if (defaultTenant == null)
        {
            defaultTenant = new Tenant
            {
                Name = "Default Company",
                Domain = "default",
                Description = "Default tenant for demo purposes",
                SubscriptionPlanId = 2 // Professional plan
            };
            context.Tenants.Add(defaultTenant);
            await context.SaveChangesAsync();
        }

        // Create admin user
        var adminUser = await userManager.FindByEmailAsync("admin@popupmanager.com");
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = "admin@popupmanager.com",
                Email = "admin@popupmanager.com",
                FirstName = "Admin",
                LastName = "User",
                TenantId = defaultTenant.Id,
                Role = UserRole.Admin,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Create regular user
        var regularUser = await userManager.FindByEmailAsync("user@popupmanager.com");
        if (regularUser == null)
        {
            regularUser = new User
            {
                UserName = "user@popupmanager.com",
                Email = "user@popupmanager.com",
                FirstName = "Regular",
                LastName = "User",
                TenantId = defaultTenant.Id,
                Role = UserRole.User,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(regularUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }
    }

    private static async Task SeedSampleContentAsync(ApplicationDbContext context)
    {
        // Seed sample blog categories
        if (!await context.BlogCategories.AnyAsync())
        {
            var categories = new[]
            {
                new BlogCategory { Name = "Tutorials", Slug = "tutorials", Description = "Step-by-step guides", TenantId = 1 },
                new BlogCategory { Name = "Best Practices", Slug = "best-practices", Description = "Industry best practices", TenantId = 1 },
                new BlogCategory { Name = "Case Studies", Slug = "case-studies", Description = "Real-world examples", TenantId = 1 },
                new BlogCategory { Name = "Product Updates", Slug = "product-updates", Description = "Latest features and improvements", TenantId = 1 }
            };

            context.BlogCategories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed sample blog posts
        if (!await context.BlogPosts.AnyAsync())
        {
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@popupmanager.com");
            if (adminUser != null)
            {
                var blogPosts = new[]
                {
                    new BlogPost
                    {
                        Title = "Getting Started with Popup Manager",
                        Slug = "getting-started-popup-manager",
                        Content = "<p>Welcome to Popup Manager! This guide will help you create your first popup...</p>",
                        Excerpt = "Learn how to create your first popup in minutes",
                        Status = BlogPostStatus.Published,
                        TenantId = 1,
                        AuthorId = adminUser.Id,
                        PublishedAt = DateTime.UtcNow.AddDays(-7)
                    },
                    new BlogPost
                    {
                        Title = "Best Practices for Email Collection",
                        Slug = "best-practices-email-collection",
                        Content = "<p>Email collection is crucial for building your audience. Here are the best practices...</p>",
                        Excerpt = "Proven strategies to increase your email conversion rates",
                        Status = BlogPostStatus.Published,
                        TenantId = 1,
                        AuthorId = adminUser.Id,
                        PublishedAt = DateTime.UtcNow.AddDays(-14)
                    }
                };

                context.BlogPosts.AddRange(blogPosts);
                await context.SaveChangesAsync();
            }
        }

        // Seed popup templates
        await SeedPopupTemplatesAsync(context);
    }

    public static async Task SeedPopupTemplatesAsync(ApplicationDbContext context)
    {
        if (await context.PopupTemplates.AnyAsync())
            return;

        await SeedTemplates(context);
    }

    public static async Task SeedTemplates(ApplicationDbContext context)
    {
        var templates = new List<PopupTemplate>();

        // Email Collector Templates
        templates.Add(new PopupTemplate
        {
            Name = "Newsletter Signup - Classic",
            Description = "A clean and simple newsletter signup form",
            Category = "Email Collection",
            Type = PopupType.EmailCollector,
            Content = "{\"heading\":\"Join Our Newsletter\",\"subheading\":\"Get weekly updates delivered to your inbox\",\"buttonText\":\"Subscribe Now\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new EmailCollectorOptions
            {
                Heading = "Join Our Newsletter",
                SubHeading = "Get weekly updates delivered to your inbox",
                CollectName = true,
                CollectPhone = false,
                ButtonText = "Subscribe Now",
                SuccessMessage = "Welcome! Check your email to confirm your subscription.",
                PrivacyText = "We respect your privacy and never share your data.",
                ShowSocialIcons = false,
                BackgroundColor = "#ffffff",
                TextColor = "#333333",
                ButtonColor = "#007bff"
            }),
            DefaultTrigger = PopupTrigger.OnExitIntent,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.OncePerDay,
            PreviewImageUrl = "/images/templates/email-classic.png",
            SortOrder = 1
        });

        templates.Add(new PopupTemplate
        {
            Name = "Newsletter Signup - Modern",
            Description = "Modern design with gradient background",
            Category = "Email Collection",
            Type = PopupType.EmailCollector,
            Content = "{\"heading\":\"Stay in the Loop\",\"subheading\":\"Subscribe for exclusive content and offers\",\"buttonText\":\"Get Started\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new EmailCollectorOptions
            {
                Heading = "Stay in the Loop",
                SubHeading = "Subscribe for exclusive content and offers",
                CollectName = false,
                CollectPhone = false,
                ButtonText = "Get Started",
                SuccessMessage = "Success! You're now subscribed.",
                ShowSocialIcons = true,
                BackgroundColor = "#6c63ff",
                TextColor = "#ffffff",
                ButtonColor = "#ff6584"
            }),
            DefaultTrigger = PopupTrigger.OnScroll,
            DefaultDelayMs = 5000,
            DefaultFrequency = PopupFrequency.OncePerWeek,
            PreviewImageUrl = "/images/templates/email-modern.png",
            SortOrder = 2
        });

        // Advertising Templates
        templates.Add(new PopupTemplate
        {
            Name = "Sale Announcement",
            Description = "Promote your special offers and sales",
            Category = "Advertising",
            Type = PopupType.Advertising,
            Content = "{\"heading\":\"Flash Sale!\",\"subheading\":\"Up to 50% off selected items\",\"buttonText\":\"Shop Now\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new AdvertisingOptions
            {
                Heading = "Flash Sale!",
                SubHeading = "Up to 50% off selected items",
                ButtonText = "Shop Now",
                ButtonUrl = "/shop/sale",
                ShowCountdown = true,
                CountdownEndDate = DateTime.UtcNow.AddDays(3),
                BackgroundColor = "#ff4757",
                ButtonColor = "#ffffff"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 2000,
            DefaultFrequency = PopupFrequency.OncePerSession,
            PreviewImageUrl = "/images/templates/ad-sale.png",
            SortOrder = 3
        });

        templates.Add(new PopupTemplate
        {
            Name = "Product Launch",
            Description = "Announce new products or services",
            Category = "Advertising",
            Type = PopupType.Advertising,
            Content = "{\"heading\":\"Introducing Our Latest Product\",\"subheading\":\"Be the first to experience innovation\",\"buttonText\":\"Learn More\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new AdvertisingOptions
            {
                Heading = "Introducing Our Latest Product",
                SubHeading = "Be the first to experience innovation",
                ImageUrl = "/images/product-hero.jpg",
                ButtonText = "Learn More",
                ButtonUrl = "/products/new",
                ShowCountdown = false,
                BackgroundColor = "#f8f9fa",
                ButtonColor = "#28a745"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 3000,
            DefaultFrequency = PopupFrequency.OncePerDay,
            PreviewImageUrl = "/images/templates/ad-product.png",
            SortOrder = 4
        });

        // Lightbox Templates
        templates.Add(new PopupTemplate
        {
            Name = "Image Lightbox",
            Description = "Display full-screen images",
            Category = "Media",
            Type = PopupType.Lightbox,
            Content = "{\"imageUrl\":\"/images/hero.jpg\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new LightboxOptions
            {
                ImageUrl = "/images/hero.jpg",
                ShowCloseButton = true,
                CloseOnBackdropClick = true,
                BackdropColor = "rgba(0,0,0,0.9)",
                MaxWidth = 1200,
                MaxHeight = 800
            }),
            DefaultTrigger = PopupTrigger.OnClick,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            PreviewImageUrl = "/images/templates/lightbox-image.png",
            SortOrder = 5
        });

        // Spin Wheel Templates
        templates.Add(new PopupTemplate
        {
            Name = "Spin to Win - Discount",
            Description = "Interactive spin wheel for discounts",
            Category = "Gamification",
            Type = PopupType.SpinWheel,
            Content = "{\"heading\":\"Spin the Wheel!\",\"subheading\":\"Try your luck for a special discount\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new SpinWheelOptions
            {
                Heading = "Spin the Wheel!",
                SubHeading = "Try your luck for a special discount",
                Segments = new List<WheelSegment>
                {
                    new() { Label = "10% OFF", Value = "SPIN10", Color = "#ff6b6b", Probability = 40 },
                    new() { Label = "15% OFF", Value = "SPIN15", Color = "#4ecdc4", Probability = 30 },
                    new() { Label = "20% OFF", Value = "SPIN20", Color = "#45b7d1", Probability = 20 },
                    new() { Label = "25% OFF", Value = "SPIN25", Color = "#f9ca24", Probability = 10 }
                },
                ButtonText = "Spin Now",
                RequireEmail = true,
                ThankYouMessage = "Congratulations! Check your email for your discount code.",
                ShowTerms = true,
                TermsText = "One spin per email address. Offer valid for 24 hours."
            }),
            DefaultTrigger = PopupTrigger.OnExitIntent,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.OnceEver,
            PreviewImageUrl = "/images/templates/spinwheel-discount.png",
            IsPremium = true,
            SortOrder = 6
        });

        // Video Popup Templates
        templates.Add(new PopupTemplate
        {
            Name = "YouTube Video Popup",
            Description = "Embed YouTube videos in a popup",
            Category = "Video",
            Type = PopupType.VideoPopup,
            Content = "{\"videoUrl\":\"https://www.youtube.com/embed/dQw4w9WgXcQ\",\"heading\":\"Watch Our Video\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new VideoPopupOptions
            {
                VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                VideoProvider = "youtube",
                AutoPlay = false,
                ShowControls = true,
                Heading = "Watch Our Video",
                Description = "Learn more about our products",
                ShowCTA = true,
                CTAText = "Get Started",
                CTAUrl = "/signup",
                Width = 800,
                Height = 450
            }),
            DefaultTrigger = PopupTrigger.OnClick,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            PreviewImageUrl = "/images/templates/video-youtube.png",
            SortOrder = 7
        });

        // Coupon Templates
        templates.Add(new PopupTemplate
        {
            Name = "Welcome Discount",
            Description = "First-time visitor discount coupon",
            Category = "Coupon",
            Type = PopupType.Coupon,
            Content = "{\"heading\":\"Welcome! Here's 15% OFF\",\"couponCode\":\"WELCOME15\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new CouponOptions
            {
                Heading = "Welcome! Here's 15% OFF",
                CouponCode = "WELCOME15",
                DiscountValue = "15% OFF",
                Description = "Use this code at checkout for your first purchase",
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                RequireEmail = true,
                ButtonText = "Get My Code",
                SuccessMessage = "Code copied! Check your email for details.",
                BackgroundColor = "#fff3cd",
                CodeBackgroundColor = "#212529",
                CodeTextColor = "#ffffff"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 5000,
            DefaultFrequency = PopupFrequency.OnceEver,
            PreviewImageUrl = "/images/templates/coupon-welcome.png",
            SortOrder = 8
        });

        // Inline Templates
        templates.Add(new PopupTemplate
        {
            Name = "Article CTA",
            Description = "Call-to-action embedded in content",
            Category = "Content",
            Type = PopupType.Inline,
            Content = "{\"content\":\"<div><h3>Ready to get started?</h3><p>Join thousands of satisfied customers</p><button>Sign Up Free</button></div>\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new InlineOptions
            {
                Content = "<div class='inline-cta'><h3>Ready to get started?</h3><p>Join thousands of satisfied customers</p><button class='btn'>Sign Up Free</button></div>",
                Position = "afterParagraph",
                ParagraphNumber = 3,
                Sticky = false,
                BackgroundColor = "#e9ecef",
                BorderColor = "#dee2e6",
                Padding = 30
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            PreviewImageUrl = "/images/templates/inline-cta.png",
            SortOrder = 9
        });

        // Message Templates
        templates.Add(new PopupTemplate
        {
            Name = "Success Notification",
            Description = "Simple success message banner",
            Category = "Notification",
            Type = PopupType.Message,
            Content = "{\"message\":\"Your action was successful!\",\"type\":\"success\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new MessageOptions
            {
                Message = "Your action was successful!",
                MessageType = "success",
                Position = "top-right",
                ShowIcon = true,
                AutoDismiss = true,
                AutoDismissDelay = 5000,
                ShowCloseButton = true,
                ShowBorder = true,
                EnableSound = false
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 1000,
            DefaultFrequency = PopupFrequency.EveryVisit,
            PreviewImageUrl = "/images/templates/message-success.png",
            SortOrder = 10
        });

        templates.Add(new PopupTemplate
        {
            Name = "Cookie Consent",
            Description = "GDPR compliant cookie notice",
            Category = "Notification",
            Type = PopupType.Message,
            Content = "{\"message\":\"We use cookies to improve your experience. By continuing, you agree to our cookie policy.\",\"type\":\"info\"}",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new MessageOptions
            {
                Message = "We use cookies to improve your experience. By continuing, you agree to our cookie policy.",
                MessageType = "info",
                Position = "bottom-center",
                ShowIcon = false,
                AutoDismiss = false,
                ShowCloseButton = true,
                ShowBorder = false,
                EnableSound = false
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 2000,
            DefaultFrequency = PopupFrequency.OnceEver,
            PreviewImageUrl = "/images/templates/message-cookie.png",
            SortOrder = 11
        });

        context.PopupTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }
}