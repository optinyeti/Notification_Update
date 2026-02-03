/**
 * Universal Tracking Pixel
 * Handles popups, forms, landing pages, and CRM tracking
 */
(function() {
    'use strict';

    // Get tenant key from script src parameter
    const scripts = document.getElementsByTagName('script');
    let tenantKey = null;
    let baseUrl = null;

    for (let i = 0; i < scripts.length; i++) {
        const src = scripts[i].src;
        if (src && src.includes('pixel.js')) {
            const url = new URL(src);
            tenantKey = url.searchParams.get('key');
            baseUrl = url.origin;
            break;
        }
    }

    if (!tenantKey) {
        console.error('[Pixel] Tenant key not found in script URL');
        return;
    }

    console.log(`[Pixel] Initialized for tenant: ${tenantKey}`);

    // Tracking queue for batch sending
    const trackingQueue = [];
    let sessionId = generateSessionId();
    let leadEmail = null;

    // Initialize tracking
    const PixelTracker = {
        tenantKey: tenantKey,
        baseUrl: baseUrl,
        sessionId: sessionId,
        
        /**
         * Track any event (pageview, popup_view, popup_click, form_view, form_submit, etc.)
         */
        track: function(eventType, data = {}) {
            const event = {
                tenantKey: this.tenantKey,
                eventType: eventType,
                email: leadEmail,
                pageUrl: window.location.href,
                pageTitle: document.title,
                timeOnPage: Math.floor((Date.now() - pageLoadTime) / 1000),
                deviceType: getDeviceType(),
                browser: getBrowser(),
                eventData: JSON.stringify(data),
                timestamp: Date.now()
            };

            trackingQueue.push(event);
            
            // Send immediately for important events
            if (['form_submit', 'purchase', 'signup'].includes(eventType)) {
                this.flush();
            }
        },

        /**
         * Identify a lead (called when email is captured)
         */
        identify: function(email, data = {}) {
            leadEmail = email;
            localStorage.setItem('pixel_lead_email', email);

            fetch(`${this.baseUrl}/api/Pixel/IdentifyLead`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    tenantKey: this.tenantKey,
                    email: email,
                    firstName: data.firstName || data.first_name,
                    lastName: data.lastName || data.last_name,
                    phone: data.phone,
                    company: data.company,
                    jobTitle: data.jobTitle || data.job_title,
                    website: data.website,
                    pageUrl: window.location.href,
                    source: data.source || 'Website'
                })
            }).catch(err => console.error('[Pixel] Error identifying lead:', err));

            this.track('lead_identified', data);
        },

        /**
         * Send queued events to server
         */
        flush: function() {
            if (trackingQueue.length === 0) return;

            const events = [...trackingQueue];
            trackingQueue.length = 0;

            events.forEach(event => {
                fetch(`${this.baseUrl}/api/Pixel/Track`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(event),
                    keepalive: true
                }).catch(err => console.error('[Pixel] Error sending event:', err));
            });
        },

        /**
         * Load popups for this tenant/website
         */
        loadPopups: function() {
            const script = document.createElement('script');
            script.src = `${this.baseUrl}/Popup/GetTenantScript/${this.tenantKey}`;
            script.async = true;
            document.head.appendChild(script);
            console.log('[Pixel] Loading popups...');
        },

        /**
         * Load forms for this tenant/website
         */
        loadForms: function() {
            // Forms are typically embedded via iframe or direct HTML
            // This watches for form submissions
            document.addEventListener('submit', (e) => {
                const form = e.target;
                if (form.dataset.pixelTracked === 'true') {
                    const formData = new FormData(form);
                    const data = {};
                    formData.forEach((value, key) => data[key] = value);
                    
                    // Identify lead if email is in form
                    if (data.email) {
                        this.identify(data.email, data);
                    }
                    
                    this.track('form_submit', {
                        formId: form.id,
                        formAction: form.action,
                        formData: data
                    });
                }
            });
        }
    };

    // Helper Functions
    function generateSessionId() {
        return 'sess_' + Math.random().toString(36).substr(2, 9) + Date.now();
    }

    function getDeviceType() {
        const ua = navigator.userAgent;
        if (/tablet|ipad|playbook|silk/i.test(ua)) return 'tablet';
        if (/mobile|iphone|ipod|android|blackberry|mini|windows\sce|palm/i.test(ua)) return 'mobile';
        return 'desktop';
    }

    function getBrowser() {
        const ua = navigator.userAgent;
        if (ua.includes('Firefox')) return 'Firefox';
        if (ua.includes('Chrome')) return 'Chrome';
        if (ua.includes('Safari')) return 'Safari';
        if (ua.includes('Edge')) return 'Edge';
        return 'Other';
    }

    // Track page load time
    const pageLoadTime = Date.now();

    // Initialize on load
    window.PixelTracker = PixelTracker;

    // Track pageview
    PixelTracker.track('pageview');

    // Load popups
    PixelTracker.loadPopups();

    // Setup form tracking
    PixelTracker.loadForms();

    // Check for existing lead email in storage
    const storedEmail = localStorage.getItem('pixel_lead_email');
    if (storedEmail) {
        leadEmail = storedEmail;
    }

    // Flush events every 10 seconds
    setInterval(() => PixelTracker.flush(), 10000);

    // Flush on page unload
    window.addEventListener('beforeunload', () => PixelTracker.flush());

    // Track time on page every 30 seconds
    setInterval(() => {
        PixelTracker.track('heartbeat');
    }, 30000);

    console.log('[Pixel] Tracking active ✓');
})();
