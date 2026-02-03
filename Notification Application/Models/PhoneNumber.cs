using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notification_Application.Models
{
    public class PhoneNumber
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;

        [StringLength(50)]
        public string FriendlyName { get; set; } = string.Empty;

        [StringLength(50)]
        public string TwilioSid { get; set; } = string.Empty;

        [Required]
        public PhoneNumberType Type { get; set; } = PhoneNumberType.Local;

        [StringLength(5)]
        public string? AreaCode { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string ForwardToNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime PurchasedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CanceledDate { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal MonthlyFee { get; set; } = 0;

        [Column(TypeName = "decimal(10,4)")]
        public decimal PerMinuteRate { get; set; } = 0;

        // UTM and Tracking
        [StringLength(255)]
        public string? UtmSource { get; set; }

        [StringLength(255)]
        public string? UtmMedium { get; set; }

        [StringLength(255)]
        public string? UtmCampaign { get; set; }

        [StringLength(255)]
        public string? UtmTerm { get; set; }

        [StringLength(255)]
        public string? UtmContent { get; set; }

        // Stats
        public int TotalCalls { get; set; } = 0;
        public int TotalMinutes { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; } = 0;

        public DateTime? LastCallDate { get; set; }

        // Navigation
        public ICollection<PhoneCall> PhoneCalls { get; set; } = new List<PhoneCall>();
    }

    public enum PhoneNumberType
    {
        Local = 0,
        TollFree = 1,
        Mobile = 2
    }

    public class PhoneCall
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PhoneNumberId { get; set; }

        [ForeignKey("PhoneNumberId")]
        public PhoneNumber? PhoneNumber { get; set; }

        [StringLength(50)]
        public string CallSid { get; set; } = string.Empty;

        [StringLength(20)]
        public string FromNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string ToNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string ForwardedTo { get; set; } = string.Empty;

        public CallStatus Status { get; set; } = CallStatus.Initiated;

        public DateTime CallDate { get; set; } = DateTime.UtcNow;

        public int DurationSeconds { get; set; } = 0;

        [Column(TypeName = "decimal(10,4)")]
        public decimal Cost { get; set; } = 0;

        [StringLength(255)]
        public string? CallerCity { get; set; }

        [StringLength(50)]
        public string? CallerState { get; set; }

        [StringLength(50)]
        public string? CallerCountry { get; set; }

        [StringLength(10)]
        public string? CallerZip { get; set; }

        // UTM Tracking (captured from number's UTM params)
        [StringLength(255)]
        public string? UtmSource { get; set; }

        [StringLength(255)]
        public string? UtmMedium { get; set; }

        [StringLength(255)]
        public string? UtmCampaign { get; set; }

        [StringLength(255)]
        public string? UtmTerm { get; set; }

        [StringLength(255)]
        public string? UtmContent { get; set; }

        // Recording
        [StringLength(500)]
        public string? RecordingUrl { get; set; }

        public int? LeadId { get; set; }

        [ForeignKey("LeadId")]
        public Lead? Lead { get; set; }

        public bool ConvertedToLead { get; set; } = false;
    }

    public enum CallStatus
    {
        Initiated = 0,
        Ringing = 1,
        InProgress = 2,
        Completed = 3,
        Busy = 4,
        Failed = 5,
        NoAnswer = 6,
        Canceled = 7
    }
}
