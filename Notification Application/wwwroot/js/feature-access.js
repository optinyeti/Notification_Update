/**
 * Billing and Feature Access Integration for Display Rules
 * This script checks if the user's plan includes advanced targeting features
 * and restricts Display Rules functionality accordingly.
 */

// Feature access checker
class FeatureAccessManager {
    constructor(tenantId) {
        this.tenantId = tenantId;
        this.hasAdvancedTargeting = false;
        this.usageStats = null;
    }

    /**
     * Initialize feature access by checking user's plan
     */
    async init() {
        try {
            const response = await fetch(`/UserDashboard/CheckFeatureAccess?tenantId=${this.tenantId}&feature=advanced_targeting`);
            const data = await response.json();
            this.hasAdvancedTargeting = data.hasAccess;

            // Get usage stats
            const usageResponse = await fetch(`/UserDashboard/GetUsageStats?tenantId=${this.tenantId}`);
            this.usageStats = await usageResponse.json();

            this.applyRestrictions();
            this.updateUI();
        } catch (error) {
            console.error('Failed to check feature access:', error);
        }
    }

    /**
     * Apply restrictions based on plan
     */
    applyRestrictions() {
        if (!this.hasAdvancedTargeting) {
            // Disable advanced targeting categories
            this.disableAdvancedCategories();
            this.showUpgradePrompt();
        }
    }

    /**
     * Disable categories that require advanced targeting
     */
    disableAdvancedCategories() {
        // Categories that require advanced plans
        const restrictedCategories = [
            'who',           // Who/Personalization
            'when',          // When/Triggers (advanced)
            'onsite',        // OnSite Retargeting
            'ecommerce'      // Ecommerce tracking
        ];

        restrictedCategories.forEach(categoryId => {
            const categoryElem = document.querySelector(`[data-category="${categoryId}"]`);
            if (categoryElem) {
                categoryElem.classList.add('disabled', 'requires-upgrade');
                categoryElem.style.opacity = '0.5';
                categoryElem.style.cursor = 'not-allowed';
                
                // Add lock icon
                const lockIcon = document.createElement('i');
                lockIcon.className = 'bi bi-lock-fill';
                lockIcon.style.marginLeft = '8px';
                lockIcon.style.color = '#f39c12';
                categoryElem.querySelector('.category-name')?.appendChild(lockIcon);
                
                // Add click handler to show upgrade prompt
                categoryElem.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    this.showUpgradeModal(categoryElem.textContent.trim());
                });
            }
        });
    }

    /**
     * Show upgrade prompt banner
     */
    showUpgradePrompt() {
        const displayRulesContainer = document.querySelector('.display-rules-container');
        if (!displayRulesContainer) return;

        const banner = document.createElement('div');
        banner.className = 'upgrade-banner alert alert-warning';
        banner.style.cssText = 'margin: 15px; padding: 15px; border-radius: 8px; display: flex; align-items: center; justify-content: space-between;';
        banner.innerHTML = `
            <div style="display: flex; align-items: center;">
                <i class="bi bi-lock-fill" style="font-size: 24px; margin-right: 12px; color: #f39c12;"></i>
                <div>
                    <strong>Advanced Targeting Features Locked</strong>
                    <p style="margin: 0; font-size: 14px;">Upgrade to access advanced targeting like personalization, triggers, and ecommerce tracking.</p>
                </div>
            </div>
            <button class="btn btn-primary" onclick="window.location.href='/Payment/Plans'">
                <i class="bi bi-arrow-up-circle"></i> Upgrade Plan
            </button>
        `;
        
        displayRulesContainer.insertBefore(banner, displayRulesContainer.firstChild);
    }

    /**
     * Show upgrade modal when clicking locked features
     */
    showUpgradeModal(featureName) {
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.id = 'upgradeModal';
        modal.setAttribute('tabindex', '-1');
        modal.innerHTML = `
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header bg-primary text-white">
                        <h5 class="modal-title">
                            <i class="bi bi-lock-fill"></i> Feature Locked
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body text-center py-4">
                        <i class="bi bi-stars" style="font-size: 48px; color: #f39c12; margin-bottom: 20px;"></i>
                        <h4>${featureName} requires an upgrade</h4>
                        <p class="text-muted">
                            Advanced targeting features like ${featureName} are available on Pro and Enterprise plans.
                            Upgrade your plan to unlock powerful targeting capabilities.
                        </p>
                        <div class="alert alert-info mt-3">
                            <strong>Current Plan:</strong> ${this.usageStats?.planName || 'Free'}<br>
                            <strong>Popups:</strong> ${this.usageStats?.popupsCreated}/${this.usageStats?.maxPopups || 'Unlimited'}<br>
                            <strong>Monthly Views:</strong> ${this.usageStats?.monthlyViews}/${this.usageStats?.maxMonthlyViews || 'Unlimited'}
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Maybe Later</button>
                        <a href="/Payment/Plans" class="btn btn-primary">
                            <i class="bi bi-arrow-up-circle"></i> View Upgrade Options
                        </a>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
        
        // Remove modal from DOM when hidden
        modal.addEventListener('hidden.bs.modal', () => {
            modal.remove();
        });
    }

    /**
     * Update UI with usage statistics
     */
    updateUI() {
        if (!this.usageStats) return;

        // Add usage badge to Display Rules tab
        const displayRulesTab = document.querySelector('[data-tab="display-rules"]');
        if (displayRulesTab && this.usageStats.popupsCreated) {
            const badge = document.createElement('span');
            badge.className = 'badge bg-info ms-2';
            badge.textContent = `${this.usageStats.popupsCreated}/${this.usageStats.maxPopups || '∞'}`;
            badge.title = 'Popups created / limit';
            displayRulesTab.appendChild(badge);
        }

        // Show usage warning if near limit
        if (this.usageStats.popupUsagePercentage > 80) {
            this.showUsageWarning();
        }
    }

    /**
     * Show usage warning if near limit
     */
    showUsageWarning() {
        const displayRulesContainer = document.querySelector('.display-rules-container');
        if (!displayRulesContainer) return;

        const warning = document.createElement('div');
        warning.className = 'usage-warning alert alert-warning';
        warning.style.cssText = 'margin: 15px; padding: 10px; border-radius: 8px;';
        warning.innerHTML = `
            <div style="display: flex; align-items: center;">
                <i class="bi bi-exclamation-triangle-fill" style="font-size: 20px; margin-right: 10px; color: #f39c12;"></i>
                <div style="flex: 1;">
                    <strong>Usage Alert:</strong> You've used ${this.usageStats.popupUsagePercentage}% of your popup limit.
                    <a href="/UserDashboard/Subscription" style="margin-left: 10px;">View Usage Details</a>
                </div>
            </div>
        `;
        
        displayRulesContainer.insertBefore(warning, displayRulesContainer.firstChild);
    }

    /**
     * Check if user can create more popups
     */
    async canCreatePopup() {
        try {
            const response = await fetch(`/UserDashboard/CanCreatePopup?tenantId=${this.tenantId}`);
            const data = await response.json();
            
            if (!data.success) {
                this.showLimitReachedModal(data.message);
                return false;
            }
            return true;
        } catch (error) {
            console.error('Failed to check popup creation limit:', error);
            return true; // Allow on error
        }
    }

    /**
     * Show modal when popup limit is reached
     */
    showLimitReachedModal(message) {
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.id = 'limitReachedModal';
        modal.setAttribute('tabindex', '-1');
        modal.innerHTML = `
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header bg-danger text-white">
                        <h5 class="modal-title">
                            <i class="bi bi-exclamation-circle-fill"></i> Popup Limit Reached
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body text-center py-4">
                        <i class="bi bi-x-octagon" style="font-size: 48px; color: #dc3545; margin-bottom: 20px;"></i>
                        <h4>You've reached your popup limit</h4>
                        <p class="text-muted">${message}</p>
                        <div class="alert alert-info mt-3">
                            <strong>Current Plan:</strong> ${this.usageStats?.planName || 'Free'}<br>
                            <strong>Popups Created:</strong> ${this.usageStats?.popupsCreated}/${this.usageStats?.maxPopups}<br>
                            <strong>To create more popups, please upgrade your plan.</strong>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        <a href="/Payment/Plans" class="btn btn-primary">
                            <i class="bi bi-arrow-up-circle"></i> Upgrade Now
                        </a>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
        
        modal.addEventListener('hidden.bs.modal', () => {
            modal.remove();
        });
    }
}

// Initialize feature access manager when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Get tenantId from page (should be added to Designer.cshtml)
    const tenantId = window.tenantId || document.querySelector('[data-tenant-id]')?.dataset.tenantId;
    
    if (tenantId) {
        window.featureAccessManager = new FeatureAccessManager(tenantId);
        window.featureAccessManager.init();
    }
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = FeatureAccessManager;
}
