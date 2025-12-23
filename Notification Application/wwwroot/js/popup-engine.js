(function() {
    'use strict';

    // Auto-detect base URL for API calls
    function getBaseUrl() {
        // If script is loaded from file://, try to get URL from script tag
        if (window.location.protocol === 'file:') {
            // Try to find this script's source URL
            const scripts = document.getElementsByTagName('script');
            for (let i = 0; i < scripts.length; i++) {
                const src = scripts[i].src;
                if (src && (src.includes('popup-engine.js') || src.includes('GetTenantScript'))) {
                    // Extract origin from script URL
                    const url = new URL(src);
                    return url.origin;
                }
            }
            // Fallback to localhost for development
            console.warn('[PopupManager] Running from file://, using localhost fallback. Set data-api-url on script tag for custom URL.');
            return 'http://localhost:5117';
        }
        
        // Check if there's a custom API URL set on the script tag
        const scripts = document.getElementsByTagName('script');
        for (let i = 0; i < scripts.length; i++) {
            const apiUrl = scripts[i].getAttribute('data-api-url');
            if (apiUrl && (scripts[i].src.includes('popup-engine.js') || scripts[i].src.includes('GetTenantScript'))) {
                console.log('[PopupManager] Using custom API URL from data-api-url:', apiUrl);
                return apiUrl;
            }
        }
        
        // If script is loaded from a server, use the current origin
        return window.location.origin;
    }

    window.PopupManager = {
        config: null,
        isShown: false,
        popup: null,
        popups: [],
        displayedPopups: new Set(),
        tenantId: null,
        trackingQueue: [],
        baseUrl: getBaseUrl(),
        trackingEndpoint: getBaseUrl() + '/api/tracking/event',
        batchTrackingEndpoint: getBaseUrl() + '/api/tracking/batch',

        // Initialize single popup (legacy support)
        init: function(config) {
            this.config = config;
            console.log('[PopupManager] Initialized', config);
            console.log('[PopupManager] Base URL:', this.baseUrl);
            console.log('[PopupManager] Tracking endpoints:', this.trackingEndpoint, this.batchTrackingEndpoint);

            // Check if popup should be shown
            if (this.shouldShow()) {
                this.loadPopupData();
            }
        },

        // Initialize multiple popups (tenant-wide tracking)
        initMultiple: function(popupsData, tenantId) {
            console.log('[PopupManager] initMultiple called with', popupsData.length, 'popups', popupsData);
            console.log('[PopupManager] Base URL:', this.baseUrl);
            console.log('[PopupManager] Tracking endpoints:', this.trackingEndpoint, this.batchTrackingEndpoint);
            this.popups = popupsData;
            this.tenantId = tenantId || 1;
            this.loadStoredState();
            this.setupTriggers();
            this.startBatchTracking();
        },

        // Global tracking function - single entry point for all events
        track: function(eventType, campaignId, metadata = {}, conversionData = null) {
            const event = {
                tenantId: this.tenantId,
                campaignId: campaignId,
                variationId: metadata.variationId || null,
                eventType: eventType,
                metadata: {
                    ...metadata,
                    url: window.location.href,
                    referrer: document.referrer,
                    timestamp: Date.now()
                },
                conversionData: conversionData,
                timestamp: Date.now()
            };

            console.log('[TRACKING]', eventType, 'for campaign', campaignId, event);

            // Add to queue for batch processing
            this.trackingQueue.push(event);

            // Also send immediately for critical events
            if (eventType === 'conversion' || eventType === 'click') {
                this.sendTrackingEvent(event);
            }
        },

        // Send single tracking event
        sendTrackingEvent: function(event) {
            fetch(this.trackingEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(event),
                keepalive: true
            })
            .then(response => response.json())
            .then(data => {
                console.log('[TRACKING] Event sent successfully:', data);
            })
            .catch(error => {
                console.error('[TRACKING] Error sending event:', error);
            });
        },

        // Batch tracking - send queued events every 5 seconds
        startBatchTracking: function() {
            setInterval(() => {
                if (this.trackingQueue.length > 0) {
                    const eventsToSend = [...this.trackingQueue];
                    this.trackingQueue = [];

                    fetch(this.batchTrackingEndpoint, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify({
                            tenantId: this.tenantId,
                            events: eventsToSend
                        }),
                        keepalive: true
                    })
                    .then(response => response.json())
                    .then(data => {
                        console.log('[TRACKING] Batch sent successfully:', data);
                    })
                    .catch(error => {
                        console.error('[TRACKING] Error sending batch:', error);
                        // Re-add failed events to queue
                        this.trackingQueue.push(...eventsToSend);
                    });
                }
            }, 5000);

            // Send any remaining events when page unloads
            window.addEventListener('beforeunload', () => {
                if (this.trackingQueue.length > 0) {
                    navigator.sendBeacon(
                        this.batchTrackingEndpoint,
                        JSON.stringify({
                            tenantId: this.tenantId,
                            events: this.trackingQueue
                        })
                    );
                }
            });
        },

        // Load which popups have been shown before
        loadStoredState: function() {
            const stored = localStorage.getItem('notificationapp_shown_popups');
            if (stored) {
                try {
                    const shown = JSON.parse(stored);
                    this.displayedPopups = new Set(shown);
                } catch (e) {
                    console.error('Error loading popup state:', e);
                }
            }
        },

        // Save which popups have been shown
        saveStoredState: function() {
            localStorage.setItem('notificationapp_shown_popups', JSON.stringify([...this.displayedPopups]));
        },

        // Setup triggers for all popups
        setupTriggers: function() {
            console.log('Setting up triggers for', this.popups.length, 'popups');
            this.popups.forEach(popup => {
                console.log('Checking popup:', popup.name, 'trigger:', popup.trigger);
                if (!this.shouldDisplayPopup(popup)) {
                    console.log('Popup', popup.name, 'should not be displayed');
                    return;
                }

                console.log('Setting up trigger for popup:', popup.name, 'trigger type:', popup.trigger);
                switch (popup.trigger) {
                    case 'OnPageLoad':
                        this.triggerOnPageLoad(popup);
                        break;
                    case 'OnExitIntent':
                        this.triggerOnExitIntent(popup);
                        break;
                    case 'OnScroll':
                        this.triggerOnScroll(popup);
                        break;
                    case 'OnTimeDelay':
                        this.triggerOnTimeDelay(popup);
                        break;
                    case 'OnClick':
                        this.triggerOnClick(popup);
                        break;
                    case 'OnIdle':
                        this.triggerOnIdle(popup);
                        break;
                    default:
                        console.warn('Unknown trigger type:', popup.trigger);
                }
            });
        },

        // Check if popup should be displayed based on frequency and targeting
        shouldDisplayPopup: function(popup) {
            console.log('Checking if popup should display:', popup.name);
            
            // Check status (API should already filter, but double-check)
            // Status is returned as int from API (1 = Published)
            console.log('Popup status check - no status field needed, API filters published');

            // Check device targeting
            const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
            console.log('Device check - isMobile:', isMobile, 'showOnMobile:', popup.showOnMobile, 'showOnDesktop:', popup.showOnDesktop);
            
            if (!isMobile && !popup.showOnDesktop) {
                console.log('Blocked: Desktop not allowed');
                return false;
            }
            if (isMobile && !popup.showOnMobile) {
                console.log('Blocked: Mobile not allowed');
                return false;
            }

            // Check URL targeting
            if (popup.targetingRules) {
                try {
                    const rules = JSON.parse(popup.targetingRules);
                    if (rules.urlTargeting) {
                        const urlRules = rules.urlTargeting.split(',').map(r => r.trim());
                        const currentUrl = window.location.href;
                        let urlMatches = false;
                        
                        for (let rule of urlRules) {
                            if (rule.startsWith('!')) {
                                const excludePattern = rule.substring(1);
                                if (currentUrl.includes(excludePattern)) {
                                    console.log('Blocked: URL excluded by rule', rule);
                                    return false;
                                }
                            } else {
                                if (currentUrl.includes(rule)) {
                                    urlMatches = true;
                                }
                            }
                        }

                        if (urlRules.some(r => !r.startsWith('!')) && !urlMatches) {
                            console.log('Blocked: URL does not match targeting rules');
                            return false;
                        }
                    }
                } catch (e) {
                    console.warn('Error parsing targeting rules:', e);
                }
            }

            // Check frequency
            const popupKey = 'popup_' + popup.id;
            const now = Date.now();

            console.log('Frequency check:', popup.frequency);
            switch (popup.frequency) {
                case 'EveryVisit':
                    console.log('Allowed: EveryVisit');
                    return true;
                
                case 'OncePerSession':
                    const sessionAllowed = !sessionStorage.getItem(popupKey);
                    console.log('OncePerSession:', sessionAllowed);
                    return sessionAllowed;
                
                case 'OncePerDay':
                    const lastShown = localStorage.getItem(popupKey);
                    if (!lastShown) {
                        console.log('Allowed: Never shown before');
                        return true;
                    }
                    const dayInMs = 24 * 60 * 60 * 1000;
                    const dayAllowed = (now - parseInt(lastShown)) > dayInMs;
                    console.log('OncePerDay:', dayAllowed);
                    return dayAllowed;
                
                case 'OncePerWeek':
                    const lastShownWeek = localStorage.getItem(popupKey);
                    if (!lastShownWeek) return true;
                    const weekInMs = 7 * 24 * 60 * 60 * 1000;
                    return (now - parseInt(lastShownWeek)) > weekInMs;
                
                case 'OncePerMonth':
                    const lastShownMonth = localStorage.getItem(popupKey);
                    if (!lastShownMonth) return true;
                    const monthInMs = 30 * 24 * 60 * 60 * 1000;
                    return (now - parseInt(lastShownMonth)) > monthInMs;
                
                case 'OnceEver':
                    const everAllowed = !this.displayedPopups.has(popup.id);
                    console.log('OnceEver:', everAllowed);
                    return everAllowed;
                
                default:
                    console.log('Allowed: Default (unknown frequency)');
                    return true;
            }
        },

        // Trigger methods for multiple popups
        triggerOnPageLoad: function(popup) {
            const delay = popup.delay || 0;
            console.log('Triggering OnPageLoad for', popup.name, 'with delay', delay, 'ms');
            setTimeout(() => this.displayPopupMultiple(popup), delay);
        },

        triggerOnExitIntent: function(popup) {
            let triggered = false;
            document.addEventListener('mouseout', (e) => {
                if (!triggered && e.clientY < 0) {
                    triggered = true;
                    this.displayPopupMultiple(popup);
                }
            });
        },

        triggerOnScroll: function(popup) {
            let triggered = false;
            const scrollPercent = popup.scrollPercentage || 50;
            window.addEventListener('scroll', () => {
                if (!triggered) {
                    const scrolled = (window.scrollY / (document.documentElement.scrollHeight - window.innerHeight)) * 100;
                    if (scrolled >= scrollPercent) {
                        triggered = true;
                        this.displayPopupMultiple(popup);
                    }
                }
            });
        },

        triggerOnTimeDelay: function(popup) {
            const delay = popup.delay || 5000;
            console.log('Triggering OnTimeDelay for', popup.name, 'with delay', delay, 'ms');
            setTimeout(() => this.displayPopupMultiple(popup), delay);
        },

        triggerOnClick: function(popup) {
            const selector = popup.clickSelector || 'a';
            document.addEventListener('click', (e) => {
                if (e.target.matches(selector)) {
                    this.displayPopupMultiple(popup);
                }
            });
        },

        triggerOnIdle: function(popup) {
            let idleTimer;
            const idleTime = popup.delay || 30000;
            
            const resetTimer = () => {
                clearTimeout(idleTimer);
                idleTimer = setTimeout(() => this.displayPopupMultiple(popup), idleTime);
            };

            ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart'].forEach(event => {
                document.addEventListener(event, resetTimer);
            });

            resetTimer();
        },

        // Display popup (for multiple popups)
        displayPopupMultiple: function(popup) {
            console.log('Displaying popup:', popup.name);
            
            // Check if another popup is currently showing
            const existingOverlay = document.querySelector('[id^="popup-overlay-"]');
            if (existingOverlay) {
                console.log('Another popup is showing, queuing this one for later');
                // Wait 2 seconds after current popup closes to show next one
                setTimeout(() => {
                    if (!document.querySelector('[id^="popup-overlay-"]')) {
                        this.displayPopupMultiple(popup);
                    }
                }, 2000);
                return;
            }
            
            // Mark as displayed
            const popupKey = 'popup_' + popup.id;
            const now = Date.now();
            
            sessionStorage.setItem(popupKey, now.toString());
            localStorage.setItem(popupKey, now.toString());
            this.displayedPopups.add(popup.id);
            this.saveStoredState();

            // Track impression event
            this.track('impression', popup.id, {
                popupName: popup.name,
                trigger: popup.trigger
            });

            // Render popup
            this.renderPopup(popup);
        },

        renderPopup: function(popup) {
            console.log('Rendering popup:', popup.name, 'with content:', popup.content);
            
            // Parse content
            let contentHtml = '';
            try {
                const content = typeof popup.content === 'string' ? JSON.parse(popup.content) : popup.content;
                if (Array.isArray(content)) {
                    // New designer format - array of blocks
                    contentHtml = this.renderBlocks(content);
                } else if (content.html) {
                    // Old format - direct HTML
                    contentHtml = content.html;
                } else {
                    contentHtml = '<div style="padding: 20px;"><p>No content</p></div>';
                }
            } catch (e) {
                console.error('Failed to parse popup content:', e, popup.content);
                contentHtml = '<div style="padding: 20px;"><p>' + (popup.content || 'Error loading content') + '</p></div>';
            }

            // Create overlay
            const overlay = document.createElement('div');
            overlay.id = 'popup-overlay-' + popup.id;
            overlay.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.5);
                z-index: 999999;
                display: flex;
                align-items: center;
                justify-content: center;
                animation: fadeIn 0.3s ease-out;
            `;

            // Create popup container
            const container = document.createElement('div');
            container.id = 'popup-' + popup.id;
            container.style.cssText = `
                background: white;
                border-radius: 12px;
                box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
                max-width: 600px;
                width: 90%;
                max-height: 90vh;
                overflow-y: auto;
                position: relative;
                animation: popupSlideIn 0.3s ease-out;
            `;

            // Create close button
            const closeBtn = document.createElement('button');
            closeBtn.innerHTML = '&times;';
            closeBtn.style.cssText = `
                position: absolute;
                top: 16px;
                right: 16px;
                background: rgba(0, 0, 0, 0.1);
                border: none;
                width: 32px;
                height: 32px;
                border-radius: 50%;
                font-size: 24px;
                line-height: 1;
                cursor: pointer;
                color: #374151;
                transition: all 0.2s;
                z-index: 10;
            `;
            closeBtn.onmouseover = function() {
                this.style.background = 'rgba(0, 0, 0, 0.2)';
            };
            closeBtn.onmouseout = function() {
                this.style.background = 'rgba(0, 0, 0, 0.1)';
            };
            closeBtn.onclick = () => {
                // Track close event
                this.track('close', popup.id, { userInitiated: true });
                this.closePopup(popup.id);
            };

            // Content wrapper
            const contentWrapper = document.createElement('div');
            contentWrapper.style.cssText = 'padding: 40px;';
            contentWrapper.innerHTML = contentHtml;

            // Add click tracking to CTA buttons
            const ctaButtons = contentWrapper.querySelectorAll('button, a.btn, .cta-button');
            ctaButtons.forEach(btn => {
                btn.addEventListener('click', () => {
                    this.track('click', popup.id, {
                        buttonText: btn.textContent,
                        buttonType: btn.tagName
                    });
                });
            });

            // Add conversion tracking to forms
            const forms = contentWrapper.querySelectorAll('form');
            forms.forEach(form => {
                form.addEventListener('submit', (e) => {
                    const formData = new FormData(form);
                    const data = {};
                    formData.forEach((value, key) => {
                        data[key] = value;
                    });

                    this.track('conversion', popup.id, {
                        formId: form.id,
                        formName: form.name
                    }, data);
                });
            });

            // Build popup
            container.appendChild(closeBtn);
            container.appendChild(contentWrapper);
            overlay.appendChild(container);
            document.body.appendChild(overlay);

            // Add CSS animations if not already added
            if (!document.getElementById('popup-animations-style')) {
                const style = document.createElement('style');
                style.id = 'popup-animations-style';
                style.textContent = `
                    @keyframes fadeIn {
                        from { opacity: 0; }
                        to { opacity: 1; }
                    }
                    @keyframes popupSlideIn {
                        from { transform: translateY(-50px); opacity: 0; }
                        to { transform: translateY(0); opacity: 1; }
                    }
                `;
                document.head.appendChild(style);
            }

            // Setup event tracking
            this.setupPopupTracking(container, popup.id);
        },

        renderBlocks: function(blocks) {
            let html = '<div style="padding: 0;">';
            blocks.forEach(block => {
                switch (block.type) {
                    case 'text':
                        html += `<div style="margin-bottom: 15px; font-size: ${block.fontSize || 16}px; color: ${block.color || '#333'}; text-align: ${block.align || 'left'};">${block.text || ''}</div>`;
                        break;
                    case 'button':
                        html += `<button onclick="window.location.href='${block.link || '#'}'" style="padding: 12px 24px; background: ${block.backgroundColor || '#007bff'}; color: ${block.color || '#fff'}; border: none; border-radius: ${block.borderRadius || 4}px; cursor: pointer; font-size: ${block.fontSize || 16}px; margin-bottom: 15px;">${block.text || 'Click Here'}</button>`;
                        break;
                    case 'image':
                        html += `<img src="${block.src || ''}" alt="${block.alt || ''}" style="max-width: 100%; height: auto; margin-bottom: 15px; border-radius: ${block.borderRadius || 0}px;">`;
                        break;
                    case 'email':
                        html += `<input type="email" placeholder="${block.placeholder || 'Enter your email'}" style="width: 100%; padding: 12px; border: 1px solid #ddd; border-radius: 4px; margin-bottom: 15px;">`;
                        break;
                    case 'divider':
                        html += `<hr style="border: none; border-top: ${block.height || 1}px ${block.style || 'solid'} ${block.color || '#ddd'}; margin: 20px 0;">`;
                        break;
                    case 'spacer':
                        html += `<div style="height: ${block.height || 20}px;"></div>`;
                        break;
                    case 'html':
                        html += block.html || '';
                        break;
                }
            });
            html += '</div>';
            return html;
        },

        closePopup: function(popupId) {
            const overlay = document.getElementById('popup-overlay-' + popupId);
            if (overlay) {
                overlay.style.animation = 'fadeOut 0.3s ease';
                setTimeout(() => overlay.remove(), 300);
            }
        },

        setupPopupTracking: function(container, popupId) {
            // Track clicks on buttons and links
            container.querySelectorAll('a, button').forEach(el => {
                el.addEventListener('click', () => {
                    this.trackEvent('click', popupId);
                });
            });
        },

        trackEvent: function(eventType, popupId) {
            // Simple tracking - you can enhance this
            console.log('Track event:', eventType, 'for popup:', popupId);
        },

        shouldShow: function() {
            // Check device targeting
            const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
            if (isMobile && !this.config.showOnMobile) return false;
            if (!isMobile && !this.config.showOnDesktop) return false;

            // Check frequency
            const storageKey = 'popup_' + this.config.id + '_shown';
            const lastShown = localStorage.getItem(storageKey);
            
            if (this.config.frequency === 'OnceEver' && lastShown) {
                return false;
            }

            if (this.config.frequency === 'OncePerDay' && lastShown) {
                const lastDate = new Date(parseInt(lastShown));
                const now = new Date();
                if (now - lastDate < 24 * 60 * 60 * 1000) return false;
            }

            if (this.config.frequency === 'OncePerWeek' && lastShown) {
                const lastDate = new Date(parseInt(lastShown));
                const now = new Date();
                if (now - lastDate < 7 * 24 * 60 * 60 * 1000) return false;
            }

            if (this.config.frequency === 'OncePerSession' && sessionStorage.getItem(storageKey)) {
                return false;
            }

            return true;
        },

        loadPopupData: function() {
            const self = this;
            fetch(`http://localhost:5117/Api/Popup/${this.config.id}`)
                .then(response => response.json())
                .then(data => {
                    self.config.content = data.content;
                    self.setupTrigger();
                })
                .catch(err => console.error('Failed to load popup data:', err));
        },

        setupTrigger: function() {
            const self = this;
            
            switch (this.config.trigger) {
                case 'OnPageLoad':
                    setTimeout(() => self.show(), this.config.delay || 0);
                    break;

                case 'OnTimeDelay':
                    setTimeout(() => self.show(), this.config.delay || 3000);
                    break;

                case 'OnScroll':
                    let scrollTriggered = false;
                    window.addEventListener('scroll', function() {
                        if (scrollTriggered) return;
                        const scrollPercent = (window.scrollY / (document.documentElement.scrollHeight - window.innerHeight)) * 100;
                        if (scrollPercent > 50) {
                            scrollTriggered = true;
                            self.show();
                        }
                    });
                    break;

                case 'OnExitIntent':
                    document.addEventListener('mouseout', function(e) {
                        if (e.clientY < 0) {
                            self.show();
                        }
                    });
                    break;

                case 'OnClick':
                    document.addEventListener('click', function(e) {
                        if (e.target.matches('[data-popup-trigger]')) {
                            e.preventDefault();
                            self.show();
                        }
                    });
                    break;

                default:
                    setTimeout(() => self.show(), 1000);
            }
        },

        show: function() {
            if (this.isShown) return;
            this.isShown = true;

            // Parse content
            let htmlContent = '';
            try {
                const content = typeof this.config.content === 'string' 
                    ? JSON.parse(this.config.content) 
                    : this.config.content;
                htmlContent = content.html || '';
            } catch (e) {
                console.error('Failed to parse popup content:', e);
                return;
            }

            // Create popup overlay
            const overlay = document.createElement('div');
            overlay.id = 'popup-overlay-' + this.config.id;
            overlay.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.5);
                z-index: 999999;
                display: flex;
                align-items: center;
                justify-content: center;
                animation: fadeIn 0.3s ease-out;
            `;

            // Create popup container
            const popup = document.createElement('div');
            popup.id = 'popup-' + this.config.id;
            popup.style.cssText = `
                background: white;
                border-radius: 12px;
                box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
                max-width: 600px;
                width: 90%;
                max-height: 90vh;
                overflow-y: auto;
                position: relative;
                animation: popupSlideIn 0.3s ease-out;
            `;

            // Create close button
            const closeBtn = document.createElement('button');
            closeBtn.innerHTML = '&times;';
            closeBtn.style.cssText = `
                position: absolute;
                top: 16px;
                right: 16px;
                background: rgba(0, 0, 0, 0.1);
                border: none;
                width: 32px;
                height: 32px;
                border-radius: 50%;
                font-size: 24px;
                line-height: 1;
                cursor: pointer;
                color: #374151;
                transition: all 0.2s;
                z-index: 10;
            `;
            closeBtn.onmouseover = function() {
                this.style.background = 'rgba(0, 0, 0, 0.2)';
            };
            closeBtn.onmouseout = function() {
                this.style.background = 'rgba(0, 0, 0, 0.1)';
            };
            closeBtn.onclick = () => this.close();

            // Create content wrapper
            const contentWrapper = document.createElement('div');
            contentWrapper.style.cssText = 'padding: 40px;';
            contentWrapper.innerHTML = htmlContent;

            // Remove component wrappers and controls from the content
            const componentWrappers = contentWrapper.querySelectorAll('.component-wrapper');
            componentWrappers.forEach(wrapper => {
                // Remove controls
                const controls = wrapper.querySelector('.component-controls');
                if (controls) controls.remove();
                
                // Unwrap the content
                while (wrapper.firstChild) {
                    wrapper.parentNode.insertBefore(wrapper.firstChild, wrapper);
                }
                wrapper.remove();
            });

            popup.appendChild(closeBtn);
            popup.appendChild(contentWrapper);
            overlay.appendChild(popup);

            // Add styles
            this.addStyles();

            // Add to page
            document.body.appendChild(overlay);
            this.popup = overlay;

            // Close on overlay click
            overlay.addEventListener('click', (e) => {
                if (e.target === overlay) {
                    this.close();
                }
            });

            // Record view
            this.recordView();

            // Store shown timestamp
            const storageKey = 'popup_' + this.config.id + '_shown';
            localStorage.setItem(storageKey, Date.now().toString());
            sessionStorage.setItem(storageKey, 'true');

            // Setup form submission if exists
            const forms = popup.querySelectorAll('form');
            forms.forEach(form => {
                form.addEventListener('submit', (e) => {
                    e.preventDefault();
                    this.handleSubmit(form);
                });
            });

            // Setup button clicks
            const buttons = popup.querySelectorAll('button:not([onclick])');
            buttons.forEach(btn => {
                if (btn !== closeBtn) {
                    btn.addEventListener('click', () => this.recordClick());
                }
            });
        },

        close: function() {
            if (this.popup) {
                this.popup.style.animation = 'fadeOut 0.2s ease-out';
                setTimeout(() => {
                    if (this.popup && this.popup.parentNode) {
                        this.popup.parentNode.removeChild(this.popup);
                    }
                    this.popup = null;
                    this.isShown = false;
                }, 200);
            }
        },

        handleSubmit: function(form) {
            const formData = new FormData(form);
            const data = {};
            formData.forEach((value, key) => {
                data[key] = value;
            });

            // Record conversion
            this.recordConversion();

            // Send to webhook if configured
            const targeting = typeof this.config.targeting === 'string' 
                ? JSON.parse(this.config.targeting) 
                : this.config.targeting;
            
            if (targeting && targeting.webhook) {
                fetch(targeting.webhook, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                }).catch(err => console.error('Webhook error:', err));
            }

            // Show success message
            const popup = document.getElementById('popup-' + this.config.id);
            if (popup) {
                popup.innerHTML = `
                    <div style="padding: 60px 40px; text-align: center;">
                        <div style="width: 64px; height: 64px; background: #10b981; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 20px;">
                            <svg width="32" height="32" fill="white" viewBox="0 0 16 16">
                                <path d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"/>
                            </svg>
                        </div>
                        <h3 style="font-size: 24px; font-weight: 700; color: #111827; margin-bottom: 12px;">Thank You!</h3>
                        <p style="font-size: 16px; color: #6b7280;">Your submission has been received.</p>
                    </div>
                `;
                setTimeout(() => this.close(), 3000);
            }
        },

        recordView: function() {
            fetch(`http://localhost:5117/Api/Popup/${this.config.id}/view`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            }).catch(err => console.error('Failed to record view:', err));
        },

        recordClick: function() {
            fetch(`http://localhost:5117/Api/Popup/${this.config.id}/click`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            }).catch(err => console.error('Failed to record click:', err));
        },

        recordConversion: function() {
            fetch(`http://localhost:5117/Api/Popup/${this.config.id}/conversion`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            }).catch(err => console.error('Failed to record conversion:', err));
        },

        addStyles: function() {
            if (document.getElementById('popup-engine-styles')) return;

            const style = document.createElement('style');
            style.id = 'popup-engine-styles';
            style.textContent = `
                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                @keyframes fadeOut {
                    from { opacity: 1; }
                    to { opacity: 0; }
                }
                @keyframes popupSlideIn {
                    from { 
                        opacity: 0;
                        transform: scale(0.9) translateY(-20px);
                    }
                    to { 
                        opacity: 1;
                        transform: scale(1) translateY(0);
                    }
                }
                #popup-overlay-${this.config.id} input[type="email"],
                #popup-overlay-${this.config.id} input[type="text"],
                #popup-overlay-${this.config.id} input[type="tel"] {
                    font-family: inherit;
                    outline: none;
                }
                #popup-overlay-${this.config.id} button:not([onclick]) {
                    cursor: pointer;
                    transition: all 0.2s;
                }
                #popup-overlay-${this.config.id} button:not([onclick]):hover {
                    opacity: 0.9;
                    transform: translateY(-1px);
                }
            `;
            document.head.appendChild(style);
        }
    };
})();
