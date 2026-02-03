-- Add new fields to SubscriptionPlans table
ALTER TABLE SubscriptionPlans ADD COLUMN MaxWebsites INTEGER DEFAULT 1;
ALTER TABLE SubscriptionPlans ADD COLUMN MaxLandingPages INTEGER DEFAULT 0;
ALTER TABLE SubscriptionPlans ADD COLUMN HasLandingPageBuilder INTEGER DEFAULT 0;

-- Add Google Analytics fields to Tenants table  
ALTER TABLE Tenants ADD COLUMN GoogleTagManagerId TEXT;
ALTER TABLE Tenants ADD COLUMN GoogleAnalytics4Id TEXT;

-- Create AllowedWebsites table
CREATE TABLE IF NOT EXISTS AllowedWebsites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TenantId INTEGER NOT NULL,
    Domain TEXT NOT NULL,
    Url TEXT NOT NULL,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    Notes TEXT,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_AllowedWebsites_TenantId ON AllowedWebsites(TenantId);
CREATE INDEX IF NOT EXISTS IX_AllowedWebsites_Domain ON AllowedWebsites(Domain);

-- Create LandingPages table
CREATE TABLE IF NOT EXISTS LandingPages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TenantId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Slug TEXT NOT NULL,
    CustomDomain TEXT,
    HtmlContent TEXT NOT NULL,
    CssContent TEXT,
    BuilderConfig TEXT,
    MetaTitle TEXT,
    MetaDescription TEXT,
    OgImageUrl TEXT,
    IsPublished INTEGER DEFAULT 0,
    ViewCount INTEGER DEFAULT 0,
    ConversionCount INTEGER DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    PublishedAt TEXT,
    CreatedById TEXT NOT NULL,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS IX_LandingPages_TenantId ON LandingPages(TenantId);
CREATE INDEX IF NOT EXISTS IX_LandingPages_Slug ON LandingPages(Slug);
CREATE INDEX IF NOT EXISTS IX_LandingPages_CreatedById ON LandingPages(CreatedById);

-- Create OnboardingProgress table
CREATE TABLE IF NOT EXISTS OnboardingProgress (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    TenantId INTEGER NOT NULL,
    HasAddedWebsite INTEGER DEFAULT 0,
    HasCreatedPopup INTEGER DEFAULT 0,
    HasInstalledPixel INTEGER DEFAULT 0,
    HasConnectedIntegration INTEGER DEFAULT 0,
    HasConfiguredTargeting INTEGER DEFAULT 0,
    HasViewedAnalytics INTEGER DEFAULT 0,
    HasCustomizedBranding INTEGER DEFAULT 0,
    HasDismissed INTEGER DEFAULT 0,
    CompletedAt TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_OnboardingProgress_UserId ON OnboardingProgress(UserId);
CREATE INDEX IF NOT EXISTS IX_OnboardingProgress_TenantId ON OnboardingProgress(TenantId);

-- Update existing subscription plans with website limits
UPDATE SubscriptionPlans SET MaxWebsites = 1, MaxLandingPages = 0, HasLandingPageBuilder = 0 WHERE Id = 1; -- Basic
UPDATE SubscriptionPlans SET MaxWebsites = 3, MaxLandingPages = 5, HasLandingPageBuilder = 1 WHERE Id = 2; -- Pro
UPDATE SubscriptionPlans SET MaxWebsites = 10, MaxLandingPages = 25, HasLandingPageBuilder = 1 WHERE Id = 3; -- Business
UPDATE SubscriptionPlans SET MaxWebsites = 9999, MaxLandingPages = 9999, HasLandingPageBuilder = 1 WHERE Id = 4; -- Enterprise
