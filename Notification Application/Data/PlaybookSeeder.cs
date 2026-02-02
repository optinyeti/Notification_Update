using Microsoft.EntityFrameworkCore;
using Notification_Application.Models;

namespace Notification_Application.Data;

public static class PlaybookSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Playbooks.AnyAsync())
            return;

        var playbooks = new List<Playbook>
        {
            // E-COMMERCE PLAYBOOKS
            new Playbook
            {
                Name = "E-commerce Conversion Booster",
                Description = "Complete funnel to convert visitors into customers with exit-intent offers, cart recovery, and urgency tactics",
                Industry = "E-commerce",
                Tactic = "Conversion Optimization",
                Icon = "bi-cart-check",
                IconColor = "blue",
                TemplateIds = "11,13,14,15,27",
                TargetingTips = "Show flash sale on entry, limited offers on exit intent, cart abandonment after 3 minutes of inactivity",
                ExpectedResults = "15-25% increase in conversion rate, 30% cart recovery rate",
                BestPractices = "Use countdown timers, create urgency, offer free shipping threshold, personalize abandoned cart messages",
                IsPopular = true,
                SortOrder = 1
            },
            new Playbook
            {
                Name = "E-commerce List Building",
                Description = "Build your email list with lead magnets, discount codes, and newsletter signups",
                Industry = "E-commerce",
                Tactic = "Lead Generation",
                Icon = "bi-envelope-heart",
                IconColor = "purple",
                TemplateIds = "6,8,26,28",
                TargetingTips = "Exit intent for discount codes, 30-second delay for lead magnets, scroll 50% for newsletter",
                ExpectedResults = "20-30% email capture rate, 10-15% conversion on welcome offers",
                BestPractices = "Offer 10-15% discount for first purchase, provide valuable lead magnets, segment by interest",
                IsPopular = true,
                SortOrder = 2
            },
            new Playbook
            {
                Name = "Holiday Sales Blitz",
                Description = "Maximize revenue during peak shopping seasons with targeted campaigns",
                Industry = "E-commerce",
                Tactic = "Seasonal Campaigns",
                Icon = "bi-gift",
                IconColor = "red",
                TemplateIds = "14,27,28,29",
                TargetingTips = "Start 7 days before holiday, increase urgency daily, target cart abandoners aggressively",
                ExpectedResults = "40-60% revenue increase during campaign period",
                BestPractices = "Stack offers strategically, create gift guides, enable easy gift wrapping, promote referrals",
                IsPopular = false,
                SortOrder = 3
            },

            // SAAS PLAYBOOKS
            new Playbook
            {
                Name = "SaaS Trial Conversion",
                Description = "Convert free trial users into paying customers with strategic messaging and offers",
                Industry = "SaaS",
                Tactic = "Conversion Optimization",
                Icon = "bi-rocket-takeoff",
                IconColor = "green",
                TemplateIds = "9,12,24,25",
                TargetingTips = "Show upgrade offers on day 3, 7, and 13 of trial. Target power users with premium features",
                ExpectedResults = "25-35% trial-to-paid conversion rate",
                BestPractices = "Educate on premium features, show ROI calculator, offer extended trials for engagement",
                IsPopular = true,
                SortOrder = 4
            },
            new Playbook
            {
                Name = "SaaS Onboarding Excellence",
                Description = "Reduce churn and increase activation with guided onboarding popups",
                Industry = "SaaS",
                Tactic = "User Activation",
                Icon = "bi-person-check",
                IconColor = "teal",
                TemplateIds = "1,3,18,31",
                TargetingTips = "Show success notifications for completed steps, warnings for incomplete setup",
                ExpectedResults = "40-50% increase in activation rate, 20% reduction in churn",
                BestPractices = "Progressive disclosure, celebrate milestones, provide contextual help, video tutorials",
                IsPopular = false,
                SortOrder = 5
            },
            new Playbook
            {
                Name = "SaaS Lead Nurturing",
                Description = "Capture and nurture leads through webinars, demos, and content",
                Industry = "SaaS",
                Tactic = "Lead Generation",
                Icon = "bi-people",
                IconColor = "purple",
                TemplateIds = "8,9,10,24",
                TargetingTips = "Offer webinars on product pages, lead magnets on blog, demo requests on pricing page",
                ExpectedResults = "30-40% lead capture rate, 15-20% demo conversion",
                BestPractices = "Segment by use case, personalize demos, follow up within 24 hours, nurture with value",
                IsPopular = true,
                SortOrder = 6
            },

            // EDUCATION PLAYBOOKS
            new Playbook
            {
                Name = "Course Enrollment Maximizer",
                Description = "Fill courses and programs with targeted enrollment campaigns",
                Industry = "Education",
                Tactic = "Enrollment",
                Icon = "bi-mortarboard",
                IconColor = "orange",
                TemplateIds = "9,10,13,26",
                TargetingTips = "Early bird discounts 30 days out, urgency tactics 7 days before deadline",
                ExpectedResults = "35-45% enrollment increase during campaign",
                BestPractices = "Offer early bird pricing, showcase testimonials, provide payment plans, limited seats messaging",
                IsPopular = false,
                SortOrder = 7
            },
            new Playbook
            {
                Name = "Student Engagement & Retention",
                Description = "Keep students engaged and reduce dropout rates",
                Industry = "Education",
                Tactic = "Retention",
                Icon = "bi-award",
                IconColor = "yellow",
                TemplateIds = "1,3,18,31",
                TargetingTips = "Celebrate completion milestones, remind about inactive courses, promote new content",
                ExpectedResults = "25-30% improvement in course completion rates",
                BestPractices = "Gamify progress, send encouragement, offer help resources, create study groups",
                IsPopular = false,
                SortOrder = 8
            },
            new Playbook
            {
                Name = "Education Lead Capture",
                Description = "Build prospective student lists and nurture them to enrollment",
                Industry = "Education",
                Tactic = "Lead Generation",
                Icon = "bi-journal-bookmark",
                IconColor = "indigo",
                TemplateIds = "6,8,9,10",
                TargetingTips = "Free resources on blog, webinars for course previews, early access for new programs",
                ExpectedResults = "25-35% lead capture rate from website traffic",
                BestPractices = "Offer free mini-courses, career guides, scholarship info, program previews",
                IsPopular = true,
                SortOrder = 9
            },

            // AGENCY/SERVICES PLAYBOOKS
            new Playbook
            {
                Name = "Agency Client Acquisition",
                Description = "Generate high-quality leads for service-based businesses",
                Industry = "Agency/Services",
                Tactic = "Lead Generation",
                Icon = "bi-briefcase",
                IconColor = "blue",
                TemplateIds = "8,22,30,31",
                TargetingTips = "Case studies as lead magnets, contact forms on service pages, free consultations on exit",
                ExpectedResults = "20-30% lead generation increase, 10-15% consultation booking rate",
                BestPractices = "Showcase portfolio, offer free audits, provide ROI calculators, testimonials",
                IsPopular = true,
                SortOrder = 10
            },
            new Playbook
            {
                Name = "Service Package Upsells",
                Description = "Increase average order value with strategic package offers",
                Industry = "Agency/Services",
                Tactic = "Revenue Growth",
                Icon = "bi-graph-up-arrow",
                IconColor = "green",
                TemplateIds = "11,12,25,26",
                TargetingTips = "Show premium packages to engaged visitors, limited-time package deals quarterly",
                ExpectedResults = "30-40% increase in average project value",
                BestPractices = "Bundle complementary services, show value comparison, offer guarantees",
                IsPopular = false,
                SortOrder = 11
            },

            // MEDIA/CONTENT PLAYBOOKS
            new Playbook
            {
                Name = "Content Site Monetization",
                Description = "Build subscriber base and monetize content through memberships",
                Industry = "Media/Publishing",
                Tactic = "Monetization",
                Icon = "bi-newspaper",
                IconColor = "purple",
                TemplateIds = "6,7,8,20",
                TargetingTips = "Newsletter signup on 2nd article view, premium content offers on 5th visit",
                ExpectedResults = "15-25% email conversion rate, 5-10% premium membership conversion",
                BestPractices = "Offer exclusive content, early access, ad-free experience, community access",
                IsPopular = false,
                SortOrder = 12
            },
            new Playbook
            {
                Name = "Event Promotion & Registration",
                Description = "Drive registrations for webinars, conferences, and virtual events",
                Industry = "Events",
                Tactic = "Registration",
                Icon = "bi-calendar-event",
                IconColor = "pink",
                TemplateIds = "9,10,13,32",
                TargetingTips = "Early bird 60 days out, regular promotion 30-7 days, last chance 3 days before",
                ExpectedResults = "40-60% increase in registrations during campaign",
                BestPractices = "Show speaker lineup, offer group discounts, create FOMO, countdown timers",
                IsPopular = false,
                SortOrder = 13
            },

            // GENERAL BUSINESS PLAYBOOKS
            new Playbook
            {
                Name = "Exit Intent Recovery",
                Description = "Recover abandoning visitors with targeted last-chance offers",
                Industry = "Universal",
                Tactic = "Abandonment Recovery",
                Icon = "bi-arrow-return-left",
                IconColor = "red",
                TemplateIds = "13,26,27,28",
                TargetingTips = "Trigger on exit intent, personalize by cart value, escalate offers on repeat exits",
                ExpectedResults = "10-20% recovery of abandoning visitors",
                BestPractices = "Make offer compelling, create urgency, simplify checkout, address objections",
                IsPopular = true,
                SortOrder = 14
            },
            new Playbook
            {
                Name = "Mobile App User Acquisition",
                Description = "Drive mobile app downloads and registrations",
                Industry = "Mobile Apps",
                Tactic = "User Acquisition",
                Icon = "bi-phone",
                IconColor = "teal",
                TemplateIds = "10,12,24,26",
                TargetingTips = "Target mobile visitors only, show on homepage and feature pages",
                ExpectedResults = "20-30% increase in app downloads from web",
                BestPractices = "Show app benefits, QR codes for easy download, app-exclusive offers",
                IsPopular = false,
                SortOrder = 15
            },
            new Playbook
            {
                Name = "Customer Feedback Collection",
                Description = "Gather valuable customer insights and testimonials",
                Industry = "Universal",
                Tactic = "Customer Insights",
                Icon = "bi-chat-quote",
                IconColor = "yellow",
                TemplateIds = "20,21,23,30",
                TargetingTips = "Request reviews after 30 days of use, surveys after key interactions",
                ExpectedResults = "25-35% response rate on feedback requests",
                BestPractices = "Keep surveys short, incentivize participation, act on feedback visibly",
                IsPopular = false,
                SortOrder = 16
            }
        };

        context.Playbooks.AddRange(playbooks);
        await context.SaveChangesAsync();
    }
}
