using DAL.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("full_name")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [Column("password")]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [Column("phone")]
        [MaxLength(15)]
        public string Phone { get; set; }

        [Required]
        [Column("province")]
        [MaxLength(150)]
        public string Province { get; set; }

        [Required]
        [Column("district")]
        [MaxLength(150)]
        public string District { get; set; }

        [Required]
        [Column("ward")]
        [MaxLength(150)]
        public string Ward { get; set; }

        [Column("specific_address")]
        [MaxLength(150)]
        public string SpecificAddress { get; set; }

        [Column("avatar")]
        [MaxLength(255)]
        public string Avatar { get; set; }

        [Column("points")]
        public int Points { get; set; } = 0;

        [Column("wallet_balance", TypeName = "decimal(12,2)")]
        public decimal WalletBalance { get; set; } = 0;

        [Column("status")]
        public UserStatus Status { get; set; } = UserStatus.ACTIVE;

        [Column("email_verified_at")]
        public DateTime? EmailVerifiedAt { get; set; }

        [Column("phone_verified_at")]
        public DateTime? PhoneVerifiedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }
        public virtual ICollection<Coupon> Coupons { get; set; }
        public virtual ICollection<PointTransaction> PointTransactions { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<PointEarningRule> PointEarningRulesCreated { get; set; }
        public virtual ICollection<Blog> Blogs { get; set; }
        public virtual ICollection<Banner> Banners { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<ReviewReply> ReviewReplies { get; set; }
        public virtual ICollection<ReviewVote> ReviewVotes { get; set; }

        public virtual Cart Cart { get; set; }
    }
}
