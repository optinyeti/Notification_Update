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

        // Create super admin user
        var superAdminUser = await userManager.FindByEmailAsync("superadmin@popupmanager.com");
        if (superAdminUser == null)
        {
            superAdminUser = new User
            {
                UserName = "superadmin@popupmanager.com",
                Email = "superadmin@popupmanager.com",
                FirstName = "Super",
                LastName = "Admin",
                TenantId = defaultTenant.Id,
                Role = UserRole.SuperAdmin,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
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

        // ==================== MESSAGE TYPE TEMPLATES (5) ====================
        templates.Add(new PopupTemplate
        {
            Name = "Success Notification",
            Description = "Simple success message banner",
            Category = "Notification",
            Type = PopupType.Message,
            Content = "{\"message\":\"Your action was successful!\",\"type\":\"success\"}",
            ImageUrl = "https://placehold.co/600x400/28a745/ffffff?text=Success",
            PreviewImageUrl = "https://placehold.co/400x300/28a745/ffffff?text=Success+Message",
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
            SortOrder = 1
        });

        templates.Add(new PopupTemplate
        {
            Name = "Cookie Consent",
            Description = "GDPR compliant cookie notice",
            Category = "Notification",
            Type = PopupType.Message,
            Content = "{\"message\":\"We use cookies to improve your experience.\",\"type\":\"info\"}",
            ImageUrl = "https://placehold.co/600x400/17a2b8/ffffff?text=Cookie+Notice",
            PreviewImageUrl = "https://placehold.co/400x300/17a2b8/ffffff?text=Cookie+Consent",
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
            SortOrder = 2
        });

        templates.Add(new PopupTemplate
        {
            Name = "Warning Alert",
            Description = "Important warning or alert message",
            Category = "Notification",
            Type = PopupType.Message,
            Content = "{\"message\":\"Please review your settings before proceeding.\",\"type\":\"warning\"}",
            ImageUrl = "https://placehold.co/600x400/ffc107/000000?text=Warning",
            PreviewImageUrl = "https://placehold.co/400x300/ffc107/000000?text=Warning+Alert",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new MessageOptions
            {
                Message = "Please review your settings before proceeding.",
                MessageType = "warning",
                Position = "top-center",
                ShowIcon = true,
                AutoDismiss = false,
                AutoDismissDelay = 0,
                ShowCloseButton = true,
                ShowBorder = true,
                EnableSound = true
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 500,
            DefaultFrequency = PopupFrequency.EveryVisit,
            SortOrder = 3
        });

        templates.Add(new PopupTemplate
        {
            Name = "Error Message",
            Description = "Display error notifications",
            Category = "Notification",
            Type = PopupType.Message,
            Content = "{\"message\":\"An error occurred. Please try again.\",\"type\":\"error\"}",
            ImageUrl = "https://placehold.co/600x400/dc3545/ffffff?text=Error",
            PreviewImageUrl = "https://placehold.co/400x300/dc3545/ffffff?text=Error+Message",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new MessageOptions
            {
                Message = "An error occurred. Please try again.",
                MessageType = "error",
                Position = "top-center",
                ShowIcon = true,
                AutoDismiss = false,
                AutoDismissDelay = 0,
                ShowCloseButton = true,
                ShowBorder = true,
                EnableSound = true
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            SortOrder = 4
        });

        templates.Add(new PopupTemplate
        {
            Name = "Announcement Bar",
            Description = "Top banner for site-wide announcements",
            Category = "Notification",
            Type = PopupType.Message,
            Content = "{\"message\":\"New features available! Click to learn more.\",\"type\":\"info\"}",
            ImageUrl = "https://placehold.co/600x400/6610f2/ffffff?text=Announcement",
            PreviewImageUrl = "https://placehold.co/400x300/6610f2/ffffff?text=Announcement+Bar",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new MessageOptions
            {
                Message = "🎉 New features available! Click to learn more.",
                MessageType = "info",
                Position = "top-center",
                ShowIcon = false,
                AutoDismiss = false,
                AutoDismissDelay = 0,
                ShowCloseButton = true,
                ShowBorder = false,
                EnableSound = false
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 1000,
            DefaultFrequency = PopupFrequency.OncePerSession,
            SortOrder = 5
        });

        // ==================== EMAIL COLLECTOR TEMPLATES (5) ====================
        templates.Add(new PopupTemplate
        {
            Name = "Newsletter Signup - Classic",
            Description = "A clean and simple newsletter signup form",
            Category = "Email Collection",
            Type = PopupType.EmailCollector,
            Content = "{\"heading\":\"Join Our Newsletter\",\"subheading\":\"Get weekly updates delivered to your inbox\",\"buttonText\":\"Subscribe Now\"}",
            ImageUrl = "https://placehold.co/600x400/007bff/ffffff?text=Newsletter",
            PreviewImageUrl = "https://placehold.co/400x300/007bff/ffffff?text=Classic+Newsletter",
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
            SortOrder = 6
        });

        templates.Add(new PopupTemplate
        {
            Name = "Newsletter Signup - Modern",
            Description = "Modern design with gradient background",
            Category = "Email Collection",
            Type = PopupType.EmailCollector,
            Content = "{\"heading\":\"Stay in the Loop\",\"subheading\":\"Subscribe for exclusive content and offers\",\"buttonText\":\"Get Started\"}",
            ImageUrl = "https://placehold.co/600x400/6c63ff/ffffff?text=Modern+Newsletter",
            PreviewImageUrl = "https://placehold.co/400x300/6c63ff/ffffff?text=Modern+Design",
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
            SortOrder = 7
        });

        templates.Add(new PopupTemplate
        {
            Name = "Lead Magnet Download",
            Description = "Offer free download in exchange for email",
            Category = "Email Collection",
            Type = PopupType.EmailCollector,
            Content = "{\"heading\":\"Get Your Free Guide\",\"subheading\":\"Download our comprehensive guide to boost your business\",\"buttonText\":\"Download Now\"}",
            ImageUrl = "https://placehold.co/600x400/20c997/ffffff?text=Free+Guide",
            PreviewImageUrl = "https://placehold.co/400x300/20c997/ffffff?text=Lead+Magnet",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new EmailCollectorOptions
            {
                Heading = "Get Your Free Guide",
                SubHeading = "Download our comprehensive guide to boost your business",
                CollectName = true,
                CollectPhone = false,
                ButtonText = "Download Now",
                SuccessMessage = "Check your email! Your guide is on its way.",
                PrivacyText = "Your email is safe with us.",
                ShowSocialIcons = false,
                BackgroundColor = "#f8f9fa",
                TextColor = "#212529",
                ButtonColor = "#20c997"
            }),
            DefaultTrigger = PopupTrigger.OnTimeDelay,
            DefaultDelayMs = 15000,
            DefaultFrequency = PopupFrequency.OnceEver,
            SortOrder = 8
        });

        templates.Add(new PopupTemplate
        {
            Name = "Webinar Registration",
            Description = "Collect registrations for webinars or events",
            Category = "Email Collection",
            Type = PopupType.EmailCollector,
            Content = "{\"heading\":\"Join Our Free Webinar\",\"subheading\":\"Learn the secrets to success from industry experts\",\"buttonText\":\"Reserve My Spot\"}",
            ImageUrl = "https://placehold.co/600x400/e83e8c/ffffff?text=Webinar",
            PreviewImageUrl = "https://placehold.co/400x300/e83e8c/ffffff?text=Webinar+Registration",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new EmailCollectorOptions
            {
                Heading = "Join Our Free Webinar",
                SubHeading = "Learn the secrets to success from industry experts",
                CollectName = true,
                CollectPhone = true,
                ButtonText = "Reserve My Spot",
                SuccessMessage = "You're registered! We'll send you the webinar link.",
                PrivacyText = "We'll only use this info for the webinar.",
                ShowSocialIcons = false,
                BackgroundColor = "#ffffff",
                TextColor = "#333333",
                ButtonColor = "#e83e8c"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 3000,
            DefaultFrequency = PopupFrequency.OncePerDay,
            SortOrder = 9
        });

        templates.Add(new PopupTemplate
        {
            Name = "Early Access Signup",
            Description = "Get early access subscribers",
            Category = "Email Collection",
            Type = PopupType.EmailCollector,
            Content = "{\"heading\":\"Be the First to Know\",\"subheading\":\"Get early access to our upcoming product launch\",\"buttonText\":\"Get Early Access\"}",
            ImageUrl = "https://placehold.co/600x400/fd7e14/ffffff?text=Early+Access",
            PreviewImageUrl = "https://placehold.co/400x300/fd7e14/ffffff?text=Early+Access",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new EmailCollectorOptions
            {
                Heading = "Be the First to Know",
                SubHeading = "Get early access to our upcoming product launch",
                CollectName = false,
                CollectPhone = false,
                ButtonText = "Get Early Access",
                SuccessMessage = "You're on the list! We'll notify you when we launch.",
                PrivacyText = "No spam, just product updates.",
                ShowSocialIcons = true,
                BackgroundColor = "#212529",
                TextColor = "#ffffff",
                ButtonColor = "#fd7e14"
            }),
            DefaultTrigger = PopupTrigger.OnExitIntent,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.OnceEver,
            SortOrder = 10
        });

        // ==================== ADVERTISING TEMPLATES (5) ====================
        templates.Add(new PopupTemplate
        {
            Name = "Flash Sale",
            Description = "Promote your special offers and sales",
            Category = "Advertising",
            Type = PopupType.Advertising,
            Content = "{\"heading\":\"Flash Sale!\",\"subheading\":\"Up to 50% off selected items\",\"buttonText\":\"Shop Now\"}",
            ImageUrl = "https://placehold.co/600x400/ff4757/ffffff?text=Flash+Sale",
            PreviewImageUrl = "https://placehold.co/400x300/ff4757/ffffff?text=Sale+50%25+OFF",
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
            SortOrder = 11
        });

        templates.Add(new PopupTemplate
        {
            Name = "Product Launch",
            Description = "Announce new products or services",
            Category = "Advertising",
            Type = PopupType.Advertising,
            Content = "{\"heading\":\"Introducing Our Latest Product\",\"subheading\":\"Be the first to experience innovation\",\"buttonText\":\"Learn More\"}",
            ImageUrl = "https://placehold.co/600x400/28a745/ffffff?text=New+Product",
            PreviewImageUrl = "https://placehold.co/400x300/28a745/ffffff?text=Product+Launch",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new AdvertisingOptions
            {
                Heading = "Introducing Our Latest Product",
                SubHeading = "Be the first to experience innovation",
                ImageUrl = "https://placehold.co/800x600/28a745/ffffff?text=New+Product+Image",
                ButtonText = "Learn More",
                ButtonUrl = "/products/new",
                ShowCountdown = false,
                BackgroundColor = "#f8f9fa",
                ButtonColor = "#28a745"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 3000,
            DefaultFrequency = PopupFrequency.OncePerDay,
            SortOrder = 12
        });

        templates.Add(new PopupTemplate
        {
            Name = "Limited Time Offer",
            Description = "Create urgency with countdown timer",
            Category = "Advertising",
            Type = PopupType.Advertising,
            Content = "{\"heading\":\"Limited Time Offer\",\"subheading\":\"Don't miss out! Offer ends soon.\",\"buttonText\":\"Claim Offer\"}",
            ImageUrl = "https://placehold.co/600x400/dc3545/ffffff?text=Limited+Offer",
            PreviewImageUrl = "https://placehold.co/400x300/dc3545/ffffff?text=Limited+Time",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new AdvertisingOptions
            {
                Heading = "⏰ Limited Time Offer",
                SubHeading = "Don't miss out! Offer ends in:",
                ButtonText = "Claim Offer",
                ButtonUrl = "/offers",
                ShowCountdown = true,
                CountdownEndDate = DateTime.UtcNow.AddHours(24),
                BackgroundColor = "#dc3545",
                ButtonColor = "#ffc107"
            }),
            DefaultTrigger = PopupTrigger.OnExitIntent,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.OncePerDay,
            SortOrder = 13
        });

        templates.Add(new PopupTemplate
        {
            Name = "Black Friday Special",
            Description = "Perfect for holiday sales campaigns",
            Category = "Advertising",
            Type = PopupType.Advertising,
            Content = "{\"heading\":\"Black Friday Mega Sale\",\"subheading\":\"Up to 70% OFF Everything!\",\"buttonText\":\"Shop Deals\"}",
            ImageUrl = "https://placehold.co/600x400/000000/ffffff?text=Black+Friday",
            PreviewImageUrl = "https://placehold.co/400x300/000000/ffffff?text=Black+Friday+70%25",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new AdvertisingOptions
            {
                Heading = "🔥 Black Friday Mega Sale",
                SubHeading = "Up to 70% OFF Everything!",
                ImageUrl = "https://placehold.co/800x600/000000/ffffff?text=BLACK+FRIDAY",
                ButtonText = "Shop Deals",
                ButtonUrl = "/black-friday",
                ShowCountdown = true,
                CountdownEndDate = DateTime.UtcNow.AddDays(7),
                BackgroundColor = "#000000",
                ButtonColor = "#ffd700"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 1000,
            DefaultFrequency = PopupFrequency.OncePerSession,
            SortOrder = 14
        });

        templates.Add(new PopupTemplate
        {
            Name = "Free Shipping Banner",
            Description = "Promote free shipping threshold",
            Category = "Advertising",
            Type = PopupType.Advertising,
            Content = "{\"heading\":\"Free Shipping on Orders Over $50\",\"subheading\":\"Shop now and save on delivery\",\"buttonText\":\"Start Shopping\"}",
            ImageUrl = "https://placehold.co/600x400/17a2b8/ffffff?text=Free+Shipping",
            PreviewImageUrl = "https://placehold.co/400x300/17a2b8/ffffff?text=Free+Shipping",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new AdvertisingOptions
            {
                Heading = "🚚 Free Shipping on Orders Over $50",
                SubHeading = "Shop now and save on delivery",
                ButtonText = "Start Shopping",
                ButtonUrl = "/shop",
                ShowCountdown = false,
                BackgroundColor = "#17a2b8",
                ButtonColor = "#ffffff"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 5000,
            DefaultFrequency = PopupFrequency.OncePerSession,
            SortOrder = 15
        });

        // ==================== LIGHTBOX TEMPLATES (4) ====================
        templates.Add(new PopupTemplate
        {
            Name = "Image Lightbox",
            Description = "Display full-screen images",
            Category = "Media",
            Type = PopupType.Lightbox,
            Content = "{\"imageUrl\":\"https://placehold.co/1200x800/6c757d/ffffff?text=Lightbox+Image\"}",
            ImageUrl = "https://placehold.co/600x400/6c757d/ffffff?text=Image+Lightbox",
            PreviewImageUrl = "https://placehold.co/400x300/6c757d/ffffff?text=Image+Gallery",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new LightboxOptions
            {
                ImageUrl = "https://placehold.co/1200x800/6c757d/ffffff?text=Full+Size+Image",
                ShowCloseButton = true,
                CloseOnBackdropClick = true,
                BackdropColor = "rgba(0,0,0,0.9)",
                MaxWidth = 1200,
                MaxHeight = 800
            }),
            DefaultTrigger = PopupTrigger.OnClick,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            SortOrder = 16
        });

        templates.Add(new PopupTemplate
        {
            Name = "Product Showcase",
            Description = "Showcase product images in lightbox",
            Category = "Media",
            Type = PopupType.Lightbox,
            Content = "{\"imageUrl\":\"https://placehold.co/1200x800/007bff/ffffff?text=Product+Showcase\"}",
            ImageUrl = "https://placehold.co/600x400/007bff/ffffff?text=Product",
            PreviewImageUrl = "https://placehold.co/400x300/007bff/ffffff?text=Product+Showcase",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new LightboxOptions
            {
                ImageUrl = "https://placehold.co/1200x800/007bff/ffffff?text=Product+Detail",
                ShowCloseButton = true,
                CloseOnBackdropClick = true,
                BackdropColor = "rgba(0,0,0,0.8)",
                MaxWidth = 1000,
                MaxHeight = 700
            }),
            DefaultTrigger = PopupTrigger.OnClick,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            SortOrder = 17
        });

        templates.Add(new PopupTemplate
        {
            Name = "Portfolio Gallery",
            Description = "Display portfolio or artwork",
            Category = "Media",
            Type = PopupType.Lightbox,
            Content = "{\"imageUrl\":\"https://placehold.co/1200x800/6610f2/ffffff?text=Portfolio+Item\"}",
            ImageUrl = "https://placehold.co/600x400/6610f2/ffffff?text=Portfolio",
            PreviewImageUrl = "https://placehold.co/400x300/6610f2/ffffff?text=Gallery",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new LightboxOptions
            {
                ImageUrl = "https://placehold.co/1200x800/6610f2/ffffff?text=Artwork",
                ShowCloseButton = true,
                CloseOnBackdropClick = true,
                BackdropColor = "rgba(0,0,0,0.95)",
                MaxWidth = 1400,
                MaxHeight = 900
            }),
            DefaultTrigger = PopupTrigger.OnClick,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            SortOrder = 18
        });

        templates.Add(new PopupTemplate
        {
            Name = "Infographic Display",
            Description = "Show infographics or charts",
            Category = "Media",
            Type = PopupType.Lightbox,
            Content = "{\"imageUrl\":\"https://placehold.co/1200x1600/20c997/ffffff?text=Infographic\"}",
            ImageUrl = "https://placehold.co/600x400/20c997/ffffff?text=Infographic",
            PreviewImageUrl = "https://placehold.co/400x300/20c997/ffffff?text=Infographic",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new LightboxOptions
            {
                ImageUrl = "https://placehold.co/1200x1600/20c997/ffffff?text=Full+Infographic",
                ShowCloseButton = true,
                CloseOnBackdropClick = true,
                BackdropColor = "rgba(255,255,255,0.95)",
                MaxWidth = 1200,
                MaxHeight = 1600
            }),
            DefaultTrigger = PopupTrigger.OnClick,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            SortOrder = 19
        });

        // ==================== SPIN WHEEL TEMPLATES (3) ====================
        templates.Add(new PopupTemplate
        {
            Name = "Spin to Win - Discount",
            Description = "Interactive spin wheel for discounts",
            Category = "Gamification",
            Type = PopupType.SpinWheel,
            Content = "{\"heading\":\"Spin the Wheel!\",\"subheading\":\"Try your luck for a special discount\"}",
            ImageUrl = "https://placehold.co/600x400/ff6b6b/ffffff?text=Spin+Wheel",
            PreviewImageUrl = "https://placehold.co/400x300/ff6b6b/ffffff?text=Discount+Wheel",
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
            IsPremium = true,
            SortOrder = 23
        });

        templates.Add(new PopupTemplate
        {
            Name = "Mystery Prize Wheel",
            Description = "Gamified prize wheel with multiple rewards",
            Category = "Gamification",
            Type = PopupType.SpinWheel,
            Content = "{\"heading\":\"Win a Mystery Prize!\",\"subheading\":\"Every spin wins something amazing\"}",
            ImageUrl = "https://placehold.co/600x400/e83e8c/ffffff?text=Mystery+Prize",
            PreviewImageUrl = "https://placehold.co/400x300/e83e8c/ffffff?text=Prize+Wheel",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new SpinWheelOptions
            {
                Heading = "🎁 Win a Mystery Prize!",
                SubHeading = "Every spin wins something amazing",
                Segments = new List<WheelSegment>
                {
                    new() { Label = "Free Gift", Value = "FREEGIFT", Color = "#e83e8c", Probability = 25 },
                    new() { Label = "$10 OFF", Value = "SAVE10", Color = "#007bff", Probability = 25 },
                    new() { Label = "Free Shipping", Value = "FREESHIP", Color = "#28a745", Probability = 30 },
                    new() { Label = "$25 OFF", Value = "SAVE25", Color = "#fd7e14", Probability = 15 },
                    new() { Label = "50% OFF", Value = "HALF", Color = "#dc3545", Probability = 5 }
                },
                ButtonText = "Spin to Win",
                RequireEmail = true,
                ThankYouMessage = "You won! Your prize code has been sent to your email.",
                ShowTerms = true,
                TermsText = "Limited to one spin per customer. Valid for 7 days."
            }),
            DefaultTrigger = PopupTrigger.OnExitIntent,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.OnceEver,
            IsPremium = true,
            SortOrder = 24
        });

        templates.Add(new PopupTemplate
        {
            Name = "Birthday Special Wheel",
            Description = "Birthday month special offers wheel",
            Category = "Gamification",
            Type = PopupType.SpinWheel,
            Content = "{\"heading\":\"Birthday Month Special!\",\"subheading\":\"Celebrate with exclusive birthday discounts\"}",
            ImageUrl = "https://placehold.co/600x400/ffc107/000000?text=Birthday+Special",
            PreviewImageUrl = "https://placehold.co/400x300/ffc107/000000?text=Birthday+Wheel",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new SpinWheelOptions
            {
                Heading = "🎂 Birthday Month Special!",
                SubHeading = "Celebrate with exclusive birthday discounts",
                Segments = new List<WheelSegment>
                {
                    new() { Label = "20% OFF", Value = "BDAY20", Color = "#ffc107", Probability = 35 },
                    new() { Label = "25% OFF", Value = "BDAY25", Color = "#ff9800", Probability = 30 },
                    new() { Label = "30% OFF", Value = "BDAY30", Color = "#ff6b6b", Probability = 25 },
                    new() { Label = "40% OFF", Value = "BDAY40", Color = "#e91e63", Probability = 10 }
                },
                ButtonText = "Spin for Birthday Gift",
                RequireEmail = true,
                ThankYouMessage = "Happy Birthday! Your gift code is waiting in your inbox.",
                ShowTerms = true,
                TermsText = "Valid during your birthday month only. One spin per year."
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 3000,
            DefaultFrequency = PopupFrequency.OncePerMonth,
            IsPremium = true,
            SortOrder = 25
        });

        // ==================== VIDEO POPUP TEMPLATES (4) ====================
        templates.Add(new PopupTemplate
        {
            Name = "YouTube Video Popup",
            Description = "Embed YouTube videos in a popup",
            Category = "Video",
            Type = PopupType.VideoPopup,
            Content = "{\"videoUrl\":\"https://www.youtube.com/embed/dQw4w9WgXcQ\",\"heading\":\"Watch Our Video\"}",
            ImageUrl = "https://placehold.co/600x400/ff0000/ffffff?text=YouTube+Video",
            PreviewImageUrl = "https://placehold.co/400x300/ff0000/ffffff?text=YouTube",
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
            SortOrder = 26
        });

        templates.Add(new PopupTemplate
        {
            Name = "Product Demo Video",
            Description = "Showcase product features with video",
            Category = "Video",
            Type = PopupType.VideoPopup,
            Content = "{\"videoUrl\":\"https://www.youtube.com/embed/demo\",\"heading\":\"See It In Action\"}",
            ImageUrl = "https://placehold.co/600x400/007bff/ffffff?text=Product+Demo",
            PreviewImageUrl = "https://placehold.co/400x300/007bff/ffffff?text=Demo+Video",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new VideoPopupOptions
            {
                VideoUrl = "https://www.youtube.com/embed/demo",
                VideoProvider = "youtube",
                AutoPlay = true,
                ShowControls = true,
                Heading = "See It In Action",
                Description = "Watch how our product can transform your workflow",
                ShowCTA = true,
                CTAText = "Try It Free",
                CTAUrl = "/trial",
                Width = 900,
                Height = 506
            }),
            DefaultTrigger = PopupTrigger.OnTimeDelay,
            DefaultDelayMs = 10000,
            DefaultFrequency = PopupFrequency.OncePerSession,
            SortOrder = 27
        });

        templates.Add(new PopupTemplate
        {
            Name = "Vimeo Video Player",
            Description = "Embed Vimeo videos with custom styling",
            Category = "Video",
            Type = PopupType.VideoPopup,
            Content = "{\"videoUrl\":\"https://player.vimeo.com/video/123456789\",\"heading\":\"Premium Content\"}",
            ImageUrl = "https://placehold.co/600x400/1ab7ea/ffffff?text=Vimeo+Video",
            PreviewImageUrl = "https://placehold.co/400x300/1ab7ea/ffffff?text=Vimeo",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new VideoPopupOptions
            {
                VideoUrl = "https://player.vimeo.com/video/123456789",
                VideoProvider = "vimeo",
                AutoPlay = false,
                ShowControls = true,
                Heading = "Premium Content",
                Description = "Exclusive video content for our members",
                ShowCTA = false,
                CTAText = "",
                CTAUrl = "",
                Width = 800,
                Height = 450
            }),
            DefaultTrigger = PopupTrigger.OnClick,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            SortOrder = 28
        });

        templates.Add(new PopupTemplate
        {
            Name = "Tutorial Video Popup",
            Description = "Educational video tutorials",
            Category = "Video",
            Type = PopupType.VideoPopup,
            Content = "{\"videoUrl\":\"https://www.youtube.com/embed/tutorial\",\"heading\":\"Learn How To\"}",
            ImageUrl = "https://placehold.co/600x400/28a745/ffffff?text=Tutorial",
            PreviewImageUrl = "https://placehold.co/400x300/28a745/ffffff?text=Tutorial+Video",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new VideoPopupOptions
            {
                VideoUrl = "https://www.youtube.com/embed/tutorial",
                VideoProvider = "youtube",
                AutoPlay = false,
                ShowControls = true,
                Heading = "Learn How To Use Our Platform",
                Description = "Follow this step-by-step tutorial to get started",
                ShowCTA = true,
                CTAText = "View All Tutorials",
                CTAUrl = "/tutorials",
                Width = 1000,
                Height = 563
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 5000,
            DefaultFrequency = PopupFrequency.OncePerWeek,
            SortOrder = 29
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

        // ==================== INLINE TEMPLATES (3) ====================
        templates.Add(new PopupTemplate
        {
            Name = "Article CTA",
            Description = "Call-to-action embedded in content",
            Category = "Content",
            Type = PopupType.Inline,
            Content = "{\"content\":\"<div><h3>Ready to get started?</h3><p>Join thousands of satisfied customers</p><button>Sign Up Free</button></div>\"}",
            ImageUrl = "https://placehold.co/600x400/17a2b8/ffffff?text=Article+CTA",
            PreviewImageUrl = "https://placehold.co/400x300/17a2b8/ffffff?text=Inline+CTA",
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
            SortOrder = 20
        });

        templates.Add(new PopupTemplate
        {
            Name = "Content Upgrade",
            Description = "Offer downloadable content within articles",
            Category = "Content",
            Type = PopupType.Inline,
            Content = "{\"content\":\"<div><h3>📥 Download Free Checklist</h3><p>Get our exclusive checklist and boost your productivity</p><button>Get It Now</button></div>\"}",
            ImageUrl = "https://placehold.co/600x400/28a745/ffffff?text=Content+Upgrade",
            PreviewImageUrl = "https://placehold.co/400x300/28a745/ffffff?text=Download+CTA",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new InlineOptions
            {
                Content = "<div class='content-upgrade'><h3>📥 Download Free Checklist</h3><p>Get our exclusive checklist and boost your productivity</p><button class='btn-success'>Get It Now</button></div>",
                Position = "afterParagraph",
                ParagraphNumber = 5,
                Sticky = false,
                BackgroundColor = "#d4edda",
                BorderColor = "#28a745",
                Padding = 25
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.OncePerSession,
            SortOrder = 21
        });

        templates.Add(new PopupTemplate
        {
            Name = "Related Products",
            Description = "Show related products in content",
            Category = "Content",
            Type = PopupType.Inline,
            Content = "{\"content\":\"<div><h3>You Might Also Like</h3><p>Check out these related products</p><button>View Products</button></div>\"}",
            ImageUrl = "https://placehold.co/600x400/6610f2/ffffff?text=Related+Products",
            PreviewImageUrl = "https://placehold.co/400x300/6610f2/ffffff?text=Product+CTA",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new InlineOptions
            {
                Content = "<div class='related-products'><h3>You Might Also Like</h3><p>Check out these related products</p><button class='btn-primary'>View Products</button></div>",
                Position = "afterParagraph",
                ParagraphNumber = 7,
                Sticky = false,
                BackgroundColor = "#f8f9fa",
                BorderColor = "#6610f2",
                Padding = 20
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.EveryVisit,
            SortOrder = 22
        });

        // ==================== COUPON TEMPLATES (5) ====================
        templates.Add(new PopupTemplate
        {
            Name = "Welcome Discount",
            Description = "First-time visitor discount coupon",
            Category = "Coupon",
            Type = PopupType.Coupon,
            Content = "{\"heading\":\"Welcome! Here's 15% OFF\",\"couponCode\":\"WELCOME15\"}",
            ImageUrl = "https://placehold.co/600x400/ffc107/000000?text=15%25+OFF",
            PreviewImageUrl = "https://placehold.co/400x300/ffc107/000000?text=Welcome+Coupon",
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
            SortOrder = 30
        });

        templates.Add(new PopupTemplate
        {
            Name = "Exit Intent Coupon",
            Description = "Last chance offer to prevent exit",
            Category = "Coupon",
            Type = PopupType.Coupon,
            Content = "{\"heading\":\"Wait! Don't Leave Yet\",\"couponCode\":\"STAY20\"}",
            ImageUrl = "https://placehold.co/600x400/dc3545/ffffff?text=20%25+OFF",
            PreviewImageUrl = "https://placehold.co/400x300/dc3545/ffffff?text=Exit+Coupon",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new CouponOptions
            {
                Heading = "Wait! Don't Leave Yet",
                CouponCode = "STAY20",
                DiscountValue = "20% OFF",
                Description = "Here's an exclusive discount just for you",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                RequireEmail = true,
                ButtonText = "Claim Discount",
                SuccessMessage = "Success! Your discount code is ready to use.",
                BackgroundColor = "#f8d7da",
                CodeBackgroundColor = "#dc3545",
                CodeTextColor = "#ffffff"
            }),
            DefaultTrigger = PopupTrigger.OnExitIntent,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.OncePerSession,
            SortOrder = 31
        });

        templates.Add(new PopupTemplate
        {
            Name = "Seasonal Sale Coupon",
            Description = "Holiday or seasonal discount code",
            Category = "Coupon",
            Type = PopupType.Coupon,
            Content = "{\"heading\":\"Summer Sale Special\",\"couponCode\":\"SUMMER30\"}",
            ImageUrl = "https://placehold.co/600x400/ff6b6b/ffffff?text=30%25+OFF",
            PreviewImageUrl = "https://placehold.co/400x300/ff6b6b/ffffff?text=Seasonal+Sale",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new CouponOptions
            {
                Heading = "☀️ Summer Sale Special",
                CouponCode = "SUMMER30",
                DiscountValue = "30% OFF",
                Description = "Limited time summer sale - save big on all items",
                ExpiryDate = DateTime.UtcNow.AddDays(14),
                RequireEmail = false,
                ButtonText = "Copy Code",
                SuccessMessage = "Code copied! Happy shopping!",
                BackgroundColor = "#ffebee",
                CodeBackgroundColor = "#ff6b6b",
                CodeTextColor = "#ffffff"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 3000,
            DefaultFrequency = PopupFrequency.OncePerDay,
            SortOrder = 32
        });

        templates.Add(new PopupTemplate
        {
            Name = "Free Shipping Code",
            Description = "Offer free shipping with coupon",
            Category = "Coupon",
            Type = PopupType.Coupon,
            Content = "{\"heading\":\"Free Shipping On Us!\",\"couponCode\":\"FREESHIP\"}",
            ImageUrl = "https://placehold.co/600x400/17a2b8/ffffff?text=Free+Shipping",
            PreviewImageUrl = "https://placehold.co/400x300/17a2b8/ffffff?text=Free+Shipping",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new CouponOptions
            {
                Heading = "🚚 Free Shipping On Us!",
                CouponCode = "FREESHIP",
                DiscountValue = "FREE SHIPPING",
                Description = "Get free shipping on any order - no minimum required",
                ExpiryDate = DateTime.UtcNow.AddDays(60),
                RequireEmail = true,
                ButtonText = "Get Free Shipping",
                SuccessMessage = "Awesome! Your free shipping code is ready.",
                BackgroundColor = "#d1ecf1",
                CodeBackgroundColor = "#17a2b8",
                CodeTextColor = "#ffffff"
            }),
            DefaultTrigger = PopupTrigger.OnScroll,
            DefaultDelayMs = 0,
            DefaultFrequency = PopupFrequency.OncePerWeek,
            SortOrder = 33
        });

        templates.Add(new PopupTemplate
        {
            Name = "VIP Member Exclusive",
            Description = "Exclusive coupon for VIP members",
            Category = "Coupon",
            Type = PopupType.Coupon,
            Content = "{\"heading\":\"VIP Exclusive: 40% OFF\",\"couponCode\":\"VIP40\"}",
            ImageUrl = "https://placehold.co/600x400/6610f2/ffffff?text=VIP+40%25",
            PreviewImageUrl = "https://placehold.co/400x300/6610f2/ffffff?text=VIP+Exclusive",
            TypeSpecificOptions = System.Text.Json.JsonSerializer.Serialize(new CouponOptions
            {
                Heading = "👑 VIP Exclusive: 40% OFF",
                CouponCode = "VIP40",
                DiscountValue = "40% OFF",
                Description = "As a valued VIP member, enjoy this exclusive discount",
                ExpiryDate = DateTime.UtcNow.AddDays(90),
                RequireEmail = true,
                ButtonText = "Activate VIP Code",
                SuccessMessage = "VIP code activated! Enjoy your exclusive discount.",
                BackgroundColor = "#e7e3fc",
                CodeBackgroundColor = "#6610f2",
                CodeTextColor = "#ffffff"
            }),
            DefaultTrigger = PopupTrigger.OnPageLoad,
            DefaultDelayMs = 2000,
            DefaultFrequency = PopupFrequency.OncePerMonth,
            IsPremium = true,
            SortOrder = 34
        });

        context.PopupTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }
}