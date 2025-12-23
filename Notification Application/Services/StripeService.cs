using Stripe;
using Stripe.Checkout;
using Notification_Application.Data;
using Notification_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Notification_Application.Services;

public interface IStripeService
{
    Task<Session> CreateCheckoutSessionAsync(int tenantId, int subscriptionPlanId, string billingPeriod, string successUrl, string cancelUrl);
    Task<Customer> CreateOrGetCustomerAsync(int tenantId, string email, string name);
    Task HandleWebhookEventAsync(string json, string stripeSignature);
    Task<string?> CreateCustomerPortalSessionAsync(int tenantId, string returnUrl);
    Task CancelSubscriptionAsync(int tenantId);
}

public class StripeService : IStripeService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public StripeService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        
        // Note: API key will be set per-tenant in methods below
        // For backward compatibility, also check appsettings
        var fallbackKey = _configuration["Stripe:SecretKey"];
        if (!string.IsNullOrEmpty(fallbackKey))
        {
            StripeConfiguration.ApiKey = fallbackKey;
        }
    }

    private async Task<string> GetStripeSecretKeyAsync(int tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        
        // Try tenant-specific key first, fallback to appsettings
        if (!string.IsNullOrEmpty(tenant?.StripeSecretKey))
        {
            return tenant.StripeSecretKey;
        }
        
        var key = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("Stripe Secret Key not configured. Please configure it in Admin Settings.");
        }
        
        return key;
    }

    private async Task<string> GetStripeWebhookSecretAsync(int? tenantId = null)
    {
        if (tenantId.HasValue)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId.Value);
            if (!string.IsNullOrEmpty(tenant?.StripeWebhookSecret))
            {
                return tenant.StripeWebhookSecret;
            }
        }
        
        var secret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("Stripe Webhook Secret not configured. Please configure it in Admin Settings.");
        }
        
        return secret;
    }

    public async Task<Session> CreateCheckoutSessionAsync(int tenantId, int subscriptionPlanId, string billingPeriod, string successUrl, string cancelUrl)
    {
        // Set API key for this tenant
        StripeConfiguration.ApiKey = await GetStripeSecretKeyAsync(tenantId);
        
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null) throw new ArgumentException("Tenant not found");

        var plan = await _context.SubscriptionPlans.FindAsync(subscriptionPlanId);
        if (plan == null) throw new ArgumentException("Subscription plan not found");

        // Get the correct price ID based on billing period
        var priceId = billingPeriod.ToLower() == "yearly" 
            ? plan.StripePriceIdYearly 
            : plan.StripePriceIdMonthly;

        if (string.IsNullOrEmpty(priceId))
            throw new InvalidOperationException("Stripe price ID not configured for this plan");

        // Create or get Stripe customer
        var customer = await CreateOrGetCustomerAsync(tenantId, "", tenant.Name);

        // Create Checkout Session
        var options = new SessionCreateOptions
        {
            Customer = customer.Id,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                }
            },
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = tenantId.ToString(),
            Metadata = new Dictionary<string, string>
            {
                { "tenant_id", tenantId.ToString() },
                { "plan_id", subscriptionPlanId.ToString() },
                { "billing_period", billingPeriod }
            }
        };

        var service = new SessionService();
        return await service.CreateAsync(options);
    }

    public async Task<Customer> CreateOrGetCustomerAsync(int tenantId, string email, string name)
    {
        // Set API key for this tenant
        StripeConfiguration.ApiKey = await GetStripeSecretKeyAsync(tenantId);
        
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null) throw new ArgumentException("Tenant not found");

        // If customer already exists, return it
        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            try
            {
                var service = new CustomerService();
                return await service.GetAsync(tenant.StripeCustomerId);
            }
            catch
            {
                // Customer might have been deleted, create new one
            }
        }

        // Create new customer
        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = name,
            Metadata = new Dictionary<string, string>
            {
                { "tenant_id", tenantId.ToString() }
            }
        };

        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(options);

        // Save customer ID to tenant
        tenant.StripeCustomerId = customer.Id;
        await _context.SaveChangesAsync();

        return customer;
    }

    public async Task<string?> CreateCustomerPortalSessionAsync(int tenantId, string returnUrl)
    {
        // Set API key for this tenant
        StripeConfiguration.ApiKey = await GetStripeSecretKeyAsync(tenantId);
        
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null || string.IsNullOrEmpty(tenant.StripeCustomerId))
            return null;

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            ReturnUrl = returnUrl,
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);

        return session.Url;
    }

    public async Task HandleWebhookEventAsync(string json, string stripeSignature)
    {
        // First, try to extract tenant_id from the event to get the right webhook secret
        // For now, use fallback from config (can be enhanced to parse JSON first)
        var webhookSecret = await GetStripeWebhookSecretAsync();
        
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Invalid webhook signature");
        }

        // Handle the event
        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
                await HandleSubscriptionUpdatedAsync(stripeEvent.Data.Object as Subscription);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync(stripeEvent.Data.Object as Subscription);
                break;

            case "invoice.payment_succeeded":
                await HandleInvoicePaymentSucceededAsync(stripeEvent.Data.Object as Invoice);
                break;

            case "invoice.payment_failed":
                await HandleInvoicePaymentFailedAsync(stripeEvent.Data.Object as Invoice);
                break;

            case "checkout.session.completed":
                await HandleCheckoutSessionCompletedAsync(stripeEvent.Data.Object as Session);
                break;

            default:
                Console.WriteLine($"Unhandled event type: {stripeEvent.Type}");
                break;
        }
    }

    private async Task HandleSubscriptionUpdatedAsync(Subscription? subscription)
    {
        if (subscription == null) return;

        var customerId = subscription.CustomerId;
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == customerId);
        
        if (tenant == null) return;

        tenant.StripeSubscriptionId = subscription.Id;
        tenant.SubscriptionStatus = subscription.Status;
        tenant.SubscriptionStartDate = subscription.CurrentPeriodStart;
        tenant.SubscriptionEndDate = subscription.CurrentPeriodEnd;

        await _context.SaveChangesAsync();
    }

    private async Task HandleSubscriptionDeletedAsync(Subscription? subscription)
    {
        if (subscription == null) return;

        var customerId = subscription.CustomerId;
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == customerId);
        
        if (tenant == null) return;

        tenant.SubscriptionStatus = "canceled";
        tenant.SubscriptionEndDate = DateTime.UtcNow;

        // Optionally downgrade to free plan
        var freePlan = await _context.SubscriptionPlans
            .Where(p => p.MonthlyPrice == 0)
            .FirstOrDefaultAsync();
        
        if (freePlan != null)
        {
            tenant.SubscriptionPlanId = freePlan.Id;
        }

        await _context.SaveChangesAsync();
    }

    private async Task HandleInvoicePaymentSucceededAsync(Invoice? invoice)
    {
        if (invoice == null) return;

        var customerId = invoice.CustomerId;
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == customerId);
        
        if (tenant == null) return;

        tenant.SubscriptionStatus = "active";
        await _context.SaveChangesAsync();
    }

    private async Task HandleInvoicePaymentFailedAsync(Invoice? invoice)
    {
        if (invoice == null) return;

        var customerId = invoice.CustomerId;
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == customerId);
        
        if (tenant == null) return;

        tenant.SubscriptionStatus = "past_due";
        await _context.SaveChangesAsync();
    }

    private async Task HandleCheckoutSessionCompletedAsync(Session? session)
    {
        if (session == null) return;

        var tenantId = session.Metadata.ContainsKey("tenant_id") 
            ? int.Parse(session.Metadata["tenant_id"]) 
            : 0;

        if (tenantId == 0) return;

        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null) return;

        // Update tenant with subscription info
        if (session.SubscriptionId != null)
        {
            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(session.SubscriptionId);

            tenant.StripeSubscriptionId = subscription.Id;
            tenant.SubscriptionStatus = subscription.Status;
            tenant.SubscriptionStartDate = subscription.CurrentPeriodStart;
            tenant.SubscriptionEndDate = subscription.CurrentPeriodEnd;

            // Update plan if specified in metadata
            if (session.Metadata.ContainsKey("plan_id"))
            {
                var planId = int.Parse(session.Metadata["plan_id"]);
                tenant.SubscriptionPlanId = planId;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task CancelSubscriptionAsync(int tenantId)
    {
        // Set API key for this tenant
        StripeConfiguration.ApiKey = await GetStripeSecretKeyAsync(tenantId);
        
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null || string.IsNullOrEmpty(tenant.StripeSubscriptionId))
            throw new InvalidOperationException("No active subscription found");

        var service = new SubscriptionService();
        await service.CancelAsync(tenant.StripeSubscriptionId);

        tenant.SubscriptionStatus = "canceled";
        await _context.SaveChangesAsync();
    }
}
