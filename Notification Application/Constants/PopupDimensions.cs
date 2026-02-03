namespace Notification_Application.Constants
{
    /// <summary>
    /// SACRED POPUP DIMENSIONS - DO NOT MODIFY WITHOUT UPDATING ALL RELATED SYSTEMS
    /// These dimensions are used across the Designer canvas, template rendering, and frontend display
    /// </summary>
    public static class PopupDimensions
    {
        // Promotion Bar (Top/Bottom Bar)
        public const string PROMOTION_BAR_WIDTH = "100%";
        public const int PROMOTION_BAR_HEIGHT = 192;  // 2 inches at 96 DPI
        public const string PROMOTION_BAR_CSS = "width: 100%; height: 192px; min-height: 192px; max-height: 192px;";
        
        // Center Modal/Lightbox
        public const int CENTER_MODAL_WIDTH = 800;
        public const int CENTER_MODAL_HEIGHT = 600;
        public const string CENTER_MODAL_CSS = "width: 800px; height: 600px; min-width: 800px; max-width: 800px; min-height: 600px; max-height: 600px;";
        
        // Fullscreen
        public const string FULLSCREEN_WIDTH = "100%";
        public const string FULLSCREEN_HEIGHT = "100%";
        public const string FULLSCREEN_CSS = "width: 100%; height: 100%; min-width: 100%; max-width: 100%; min-height: 100%; max-height: 100%;";
        
        // Side Panel (Slide-in)
        public const int SIDE_PANEL_WIDTH = 400;
        public const string SIDE_PANEL_HEIGHT = "100%";
        public const string SIDE_PANEL_CSS = "width: 400px; height: 100%; min-width: 400px; max-width: 400px; min-height: 100%; max-height: 100%;";
        
        // Mobile Breakpoint
        public const int MOBILE_BREAKPOINT = 768;
        
        /// <summary>
        /// Get template wrapper style for specific popup type
        /// </summary>
        public static string GetTemplateWrapperStyle(string popupType)
        {
            return popupType?.ToLower() switch
            {
                "promotionbar" => PROMOTION_BAR_CSS,
                "top" => PROMOTION_BAR_CSS,
                "bottom" => PROMOTION_BAR_CSS,
                "center" => CENTER_MODAL_CSS,
                "modal" => CENTER_MODAL_CSS,
                "lightbox" => CENTER_MODAL_CSS,
                "fullscreen" => FULLSCREEN_CSS,
                "side" => SIDE_PANEL_CSS,
                "slidein" => SIDE_PANEL_CSS,
                _ => CENTER_MODAL_CSS  // Default to center modal
            };
        }
    }
}
