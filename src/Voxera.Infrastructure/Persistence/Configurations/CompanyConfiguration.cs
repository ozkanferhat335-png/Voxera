using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voxera.Domain.Entities;

namespace Voxera.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.Slug).IsUnique();
        builder.Property(e => e.Domain).HasColumnName("domain").HasMaxLength(255);
        builder.Property(e => e.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
        builder.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
        builder.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(e => e.TaxNumber).HasColumnName("tax_number").HasMaxLength(50);
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(e => e.Plan).HasColumnName("plan").HasConversion<int>();
        builder.Property(e => e.PlanExpiresAt).HasColumnName("plan_expires_at");
        builder.Property(e => e.MaxExtensions).HasColumnName("max_extensions");
        builder.Property(e => e.MaxConcurrentCalls).HasColumnName("max_concurrent_calls");
        builder.Property(e => e.IsTrialActive).HasColumnName("is_trial_active");
        builder.Property(e => e.TrialEndsAt).HasColumnName("trial_ends_at");
        builder.Property(e => e.SipDomain).HasColumnName("sip_domain").HasMaxLength(255);
        builder.Property(e => e.WebhookUrl).HasColumnName("webhook_url").HasMaxLength(500);
        builder.Property(e => e.WebhookSecret).HasColumnName("webhook_secret").HasMaxLength(255);
        builder.Property(e => e.TimeZone).HasColumnName("time_zone").HasMaxLength(100);
        builder.Property(e => e.Country).HasColumnName("country").HasMaxLength(10);
        builder.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        builder.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(50);
        builder.Property(e => e.Role).HasColumnName("role").HasConversion<int>();
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(e => e.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
        builder.Property(e => e.RefreshToken).HasColumnName("refresh_token").HasMaxLength(500);
        builder.Property(e => e.RefreshTokenExpiresAt).HasColumnName("refresh_token_expires_at");
        builder.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
        builder.Property(e => e.LastLoginIp).HasColumnName("last_login_ip").HasMaxLength(50);
        builder.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
        builder.Property(e => e.TwoFactorSecret).HasColumnName("two_factor_secret").HasMaxLength(100);
        builder.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts");
        builder.Property(e => e.LockedUntil).HasColumnName("locked_until");
        builder.Property(e => e.PasswordResetToken).HasColumnName("password_reset_token").HasMaxLength(500);
        builder.Property(e => e.PasswordResetTokenExpiresAt).HasColumnName("password_reset_token_expires_at");
        builder.Property(e => e.TimeZone).HasColumnName("time_zone").HasMaxLength(100);
        builder.Property(e => e.Language).HasColumnName("language").HasMaxLength(10);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(e => e.Company).WithMany(c => c.Users).HasForeignKey(e => e.CompanyId);
    }
}

public class ExtensionConfiguration : IEntityTypeConfiguration<Extension>
{
    public void Configure(EntityTypeBuilder<Extension> builder)
    {
        builder.ToTable("extensions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.Number).HasColumnName("number").HasMaxLength(20).IsRequired();
        builder.HasIndex(e => new { e.CompanyId, e.Number }).IsUnique();
        builder.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(e => e.Type).HasColumnName("type").HasConversion<int>();
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(e => e.VoicemailEnabled).HasColumnName("voicemail_enabled");
        builder.Property(e => e.VoicemailPin).HasColumnName("voicemail_pin").HasMaxLength(10);
        builder.Property(e => e.ForwardTo).HasColumnName("forward_to").HasMaxLength(50);
        builder.Property(e => e.ForwardCondition).HasColumnName("forward_condition").HasConversion<int>();
        builder.Property(e => e.MaxCallDuration).HasColumnName("max_call_duration");
        builder.Property(e => e.RecordCalls).HasColumnName("record_calls");
        builder.Property(e => e.CallerIdName).HasColumnName("caller_id_name").HasMaxLength(100);
        builder.Property(e => e.CallerIdNumber).HasColumnName("caller_id_number").HasMaxLength(50);
        builder.Property(e => e.RingTimeout).HasColumnName("ring_timeout");
        builder.Property(e => e.DoNotDisturb).HasColumnName("do_not_disturb");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(e => e.Company).WithMany(c => c.Extensions).HasForeignKey(e => e.CompanyId);
        builder.HasOne(e => e.User).WithOne(u => u.Extension).HasForeignKey<Extension>(e => e.UserId);
    }
}

public class SipAccountConfiguration : IEntityTypeConfiguration<SipAccount>
{
    public void Configure(EntityTypeBuilder<SipAccount> builder)
    {
        builder.ToTable("sip_accounts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        builder.Property(e => e.ExtensionId).HasColumnName("extension_id");
        builder.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(e => e.Domain).HasColumnName("domain").HasMaxLength(255).IsRequired();
        builder.HasIndex(e => new { e.Username, e.Domain }).IsUnique();
        builder.Property(e => e.ContactUri).HasColumnName("contact_uri").HasMaxLength(500);
        builder.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(e => e.AgentStatus).HasColumnName("agent_status").HasConversion<int>();
        builder.Property(e => e.LastRegisteredAt).HasColumnName("last_registered_at");
        builder.Property(e => e.LastRegisteredIp).HasColumnName("last_registered_ip").HasMaxLength(50);
        builder.Property(e => e.LastRegisteredPort).HasColumnName("last_registered_port");
        builder.Property(e => e.Transport).HasColumnName("transport").HasMaxLength(10);
        builder.Property(e => e.WebRtcEnabled).HasColumnName("webrtc_enabled");
        builder.Property(e => e.NatIp).HasColumnName("nat_ip").HasMaxLength(50);
        builder.Property(e => e.MaxSessions).HasColumnName("max_sessions");
        builder.Property(e => e.CurrentSessions).HasColumnName("current_sessions");
        builder.Property(e => e.Codec).HasColumnName("codec").HasMaxLength(200);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(e => e.Company).WithMany(c => c.SipAccounts).HasForeignKey(e => e.CompanyId);
        builder.HasOne(e => e.Extension).WithOne(ex => ex.SipAccount).HasForeignKey<SipAccount>(e => e.ExtensionId);
    }
}

public class CallLogConfiguration : IEntityTypeConfiguration<CallLog>
{
    public void Configure(EntityTypeBuilder<CallLog> builder)
    {
        builder.ToTable("call_logs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        builder.Property(e => e.CallId).HasColumnName("call_id").HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.CallId).IsUnique();
        builder.Property(e => e.CallerNumber).HasColumnName("caller_number").HasMaxLength(50);
        builder.Property(e => e.CallerName).HasColumnName("caller_name").HasMaxLength(200);
        builder.Property(e => e.CalleeNumber).HasColumnName("callee_number").HasMaxLength(50);
        builder.Property(e => e.CalleeName).HasColumnName("callee_name").HasMaxLength(200);
        builder.Property(e => e.FromExtensionId).HasColumnName("from_extension_id");
        builder.Property(e => e.ToExtensionId).HasColumnName("to_extension_id");
        builder.Property(e => e.Direction).HasColumnName("direction").HasConversion<int>();
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(e => e.EndReason).HasColumnName("end_reason").HasConversion<int>();
        builder.Property(e => e.StartedAt).HasColumnName("started_at");
        builder.Property(e => e.AnsweredAt).HasColumnName("answered_at");
        builder.Property(e => e.EndedAt).HasColumnName("ended_at");
        builder.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
        builder.Property(e => e.BillableSeconds).HasColumnName("billable_seconds");
        builder.Property(e => e.RingDurationSeconds).HasColumnName("ring_duration_seconds");
        builder.Property(e => e.IsRecorded).HasColumnName("is_recorded");
        builder.Property(e => e.RecordingPath).HasColumnName("recording_path").HasMaxLength(500);
        builder.Property(e => e.RecordingSize).HasColumnName("recording_size");
        builder.Property(e => e.SipTrunkId).HasColumnName("sip_trunk_id").HasMaxLength(100);
        builder.Property(e => e.IvrPath).HasColumnName("ivr_path").HasMaxLength(500);
        builder.Property(e => e.QueueId).HasColumnName("queue_id");
        builder.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(e => e.Tags).HasColumnName("tags").HasMaxLength(500);
        builder.Property(e => e.IsTransferred).HasColumnName("is_transferred");
        builder.Property(e => e.TransferredTo).HasColumnName("transferred_to").HasMaxLength(50);
        builder.Property(e => e.Cost).HasColumnName("cost").HasPrecision(10, 4);
        builder.Property(e => e.AiSummary).HasColumnName("ai_summary");
        builder.Property(e => e.Transcript).HasColumnName("transcript");
        builder.Property(e => e.Sentiment).HasColumnName("sentiment").HasConversion<int>();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(e => e.Company).WithMany(c => c.CallLogs).HasForeignKey(e => e.CompanyId);
        builder.HasOne(e => e.FromExtension).WithMany(ex => ex.OutboundCalls).HasForeignKey(e => e.FromExtensionId);
        builder.HasOne(e => e.ToExtension).WithMany(ex => ex.InboundCalls).HasForeignKey(e => e.ToExtensionId);
        builder.HasOne(e => e.Queue).WithMany(q => q.CallLogs).HasForeignKey(e => e.QueueId);

        // Indexes for performance
        builder.HasIndex(e => new { e.CompanyId, e.StartedAt });
        builder.HasIndex(e => new { e.CompanyId, e.Status });
        builder.HasIndex(e => e.CallerNumber);
    }
}

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CompanyId).HasColumnName("company_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.KeyHash).HasColumnName("key_hash").HasMaxLength(500).IsRequired();
        builder.Property(e => e.KeyPrefix).HasColumnName("key_prefix").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(e => e.Permissions).HasColumnName("permissions").HasConversion(
            v => string.Join(",", v),
            v => v.Split(",", StringSplitOptions.RemoveEmptyEntries));
        builder.Property(e => e.IpWhitelist).HasColumnName("ip_whitelist").HasMaxLength(1000);
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
        builder.Property(e => e.LastUsedIp).HasColumnName("last_used_ip").HasMaxLength(50);
        builder.Property(e => e.RequestCount).HasColumnName("request_count");
        builder.Property(e => e.RateLimitPerMinute).HasColumnName("rate_limit_per_minute");
        builder.Property(e => e.RateLimitPerDay).HasColumnName("rate_limit_per_day");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(e => e.Company).WithMany(c => c.ApiKeys).HasForeignKey(e => e.CompanyId);
    }
}
