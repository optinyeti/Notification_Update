# Twilio Phone Tracking System - Quick Reference

## Overview
Complete call tracking system integrated with Twilio Voice API for purchasing local and toll-free numbers, tracking calls with UTM parameters, and automatic CRM lead generation.

## Configuration

### Required Twilio Settings (appsettings.json)
```json
{
  "Twilio": {
    "AccountSid": "your_twilio_account_sid",
    "AuthToken": "your_twilio_auth_token"
  },
  "AppUrl": "https://yourdomain.com"  // For webhook callbacks
}
```

### Getting Twilio Credentials
1. Sign up at https://www.twilio.com/console
2. Get your Account SID and Auth Token from the Twilio Console
3. Update appsettings.json with your credentials

### Webhook Configuration
After deploying, configure these webhooks in Twilio Console:
- **Voice URL**: `https://yourdomain.com/api/twilio/voice`
- **Status Callback**: `https://yourdomain.com/api/twilio/status`
- **Recording Callback**: `https://yourdomain.com/api/twilio/recording`

## Features

### 1. Number Management (`/Numbers`)
- **Search Available Numbers**: Find local or toll-free numbers by area code, city, or state
- **Purchase Numbers**: Buy numbers directly through Twilio API
- **Configure Forwarding**: Set where incoming calls should be forwarded
- **UTM Tracking**: Assign UTM parameters to track marketing campaigns

### 2. Call Tracking
Every call is automatically tracked with:
- Caller information (number, city, state, country, zip)
- Call duration and cost
- Recording URL
- UTM parameters from the tracking number
- Conversion to CRM lead (if call duration > 30 seconds)

### 3. Pricing
- **Local Numbers**: $1.15/month + $0.013/minute
- **Toll-Free Numbers**: $2.00/month + $0.02/minute

### 4. Analytics
View detailed statistics:
- Total calls received
- Total minutes
- Total cost
- Calls by UTM parameters
- Conversion rates

## Usage

### Purchase a Tracking Number
1. Navigate to **Numbers** in the menu
2. Click **Search Available Numbers**
3. Choose between Local or Toll-Free
4. Enter search criteria (area code, city, state)
5. Click **Order** on desired number
6. Configure:
   - Friendly name
   - Forward to number
   - UTM parameters (source, medium, campaign, etc.)
7. Click **Purchase**

### Viewing Call History
1. Go to **Numbers** page
2. Click on any number card
3. View all calls with details:
   - Caller information
   - Duration and cost
   - Status
   - Recording (if available)
   - Associated lead (if converted)

### UTM Parameter Tracking
Assign UTM parameters when purchasing a number:
- **Source**: Where the traffic comes from (e.g., "google", "facebook")
- **Medium**: Marketing medium (e.g., "cpc", "social", "email")
- **Campaign**: Campaign name (e.g., "summer_sale", "product_launch")
- **Term**: Paid keywords (e.g., "running shoes")
- **Content**: Ad variation (e.g., "banner_a", "text_link")

All calls to that number will automatically inherit these UTM parameters and pass them to generated leads.

### Automatic Lead Generation
Calls longer than 30 seconds automatically create leads in your CRM with:
- Contact information from caller ID
- Call details (duration, recording)
- UTM parameters for attribution
- Lead activity record

## Database Tables

### PhoneNumbers
Stores purchased tracking numbers with:
- Twilio SID and number
- Forwarding configuration
- UTM parameters
- Usage statistics
- Pricing information

### PhoneCalls
Records all inbound calls with:
- Caller details
- Call metrics (duration, cost)
- Status and recording URL
- Link to generated lead (if applicable)

## Dashboard Integration

### Quick Actions (Updated)
The dashboard now features three primary quick actions:
1. **Create Popup** - Launch popup builder
2. **Create Form** - Build web forms
3. **Create a Deal** - Add CRM opportunities

### Numbers Menu
Access phone tracking system from main navigation menu.

### Analytics
Phone tracking statistics integrated into main analytics dashboard showing:
- Call volume trends
- Cost per acquisition
- Lead conversion from calls
- UTM performance

## API Endpoints

### NumbersController
- `GET /Numbers` - List user's numbers
- `GET /Numbers/Search` - Search available numbers form
- `POST /Numbers/SearchAvailable` - Query Twilio API
- `GET /Numbers/Order` - Purchase form
- `POST /Numbers/Purchase` - Buy number
- `GET /Numbers/Details/{id}` - Number details and call history
- `GET /Numbers/Edit/{id}` - Edit number settings
- `POST /Numbers/Edit/{id}` - Save changes
- `POST /Numbers/Cancel/{id}` - Release number

### TwilioWebhookController
- `POST /api/twilio/voice` - Handle incoming calls
- `POST /api/twilio/status` - Update call status
- `POST /api/twilio/recording` - Save recording URL

## Security Notes

1. **Webhook Validation**: All webhook endpoints validate requests come from Twilio
2. **User Authorization**: Number management requires authentication
3. **Tenant Isolation**: Users can only access their own numbers
4. **Sensitive Data**: Store Twilio credentials in secure configuration (Azure Key Vault, AWS Secrets Manager)

## Troubleshooting

### Calls Not Being Tracked
- Verify webhook URLs are configured in Twilio Console
- Check that AppUrl in configuration matches your public URL
- Ensure phone number exists in database
- Check application logs for errors

### Number Purchase Fails
- Verify Twilio credentials are correct
- Check account balance in Twilio Console
- Ensure number is available (not already purchased)
- Review Twilio API error messages in logs

### Leads Not Being Created
- Confirm call duration > 30 seconds
- Check that caller has valid phone number
- Verify Lead entity can be saved (no validation errors)
- Review CreateLeadFromCall method logs

## Next Steps

1. **Add Twilio Credentials**: Update appsettings.json
2. **Deploy Application**: Deploy to publicly accessible URL
3. **Configure Webhooks**: Set webhook URLs in Twilio Console
4. **Purchase Test Number**: Buy a number and test the system
5. **Monitor Analytics**: Track call performance and conversions

## Support Resources

- Twilio Documentation: https://www.twilio.com/docs/voice
- Twilio Console: https://console.twilio.com
- Pricing Calculator: https://www.twilio.com/voice/pricing
