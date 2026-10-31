using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCAE.Domain.AccessPoints;
using SGCAE.Domain.AccessRecords;
using SGCAE.Domain.Credentials;
using SGCAE.Domain.Employees;

namespace SGCAE.Infrastructure.Persistence;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.OwnsOne(e => e.Identification, b =>
        {
            b.Property(i => i.Value).HasColumnName("Identification").HasMaxLength(Identification.MaxLength).IsRequired();
            b.HasIndex(i => i.Value).IsUnique();
        });
        builder.Navigation(e => e.Identification).IsRequired();

        builder.OwnsOne(e => e.Name, b =>
        {
            b.Property(n => n.FirstNames).HasColumnName("FirstNames").HasMaxLength(FullName.MaxLength).IsRequired();
            b.Property(n => n.LastNames).HasColumnName("LastNames").HasMaxLength(FullName.MaxLength).IsRequired();
        });
        builder.Navigation(e => e.Name).IsRequired();

        builder.OwnsOne(e => e.Email, b =>
        {
            b.Property(m => m.Value).HasColumnName("Email").HasMaxLength(Email.MaxLength).IsRequired();
            b.HasIndex(m => m.Value).IsUnique();
        });
        builder.Navigation(e => e.Email).IsRequired();

        builder.Property(e => e.Position).HasMaxLength(Employee.PositionMaxLength).IsRequired();
        builder.Property(e => e.Area).HasMaxLength(Employee.AreaMaxLength).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => e.Status);

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.IsActive);
    }
}

internal sealed class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("Credentials");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.OwnsOne(c => c.Code, b =>
        {
            b.Property(x => x.Value).HasColumnName("Code").HasMaxLength(CredentialCode.MaxLength).IsRequired();
            b.HasIndex(x => x.Value).IsUnique();
        });
        builder.Navigation(c => c.Code).IsRequired();

        builder.HasOne<Employee>().WithMany().HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.StatusReason).HasMaxLength(Credential.ReasonMaxLength);
        builder.HasIndex(c => new { c.EmployeeId, c.Status });

        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.CanBeUsed);
    }
}

internal sealed class AccessPointConfiguration : IEntityTypeConfiguration<AccessPoint>
{
    public void Configure(EntityTypeBuilder<AccessPoint> builder)
    {
        builder.ToTable("AccessPoints");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Code).HasMaxLength(AccessPoint.CodeMaxLength).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(AccessPoint.NameMaxLength).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Location).HasMaxLength(AccessPoint.LocationMaxLength);

        builder.Ignore(p => p.DomainEvents);
    }
}

internal sealed class AccessRecordConfiguration : IEntityTypeConfiguration<AccessRecord>
{
    public void Configure(EntityTypeBuilder<AccessRecord> builder)
    {
        builder.ToTable("AccessRecords");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.RequestedCredentialCode).HasMaxLength(AccessRecord.RequestedCodeMaxLength).IsRequired();
        builder.Property(r => r.RequestedAccessPointCode).HasMaxLength(AccessRecord.RequestedCodeMaxLength).IsRequired();
        builder.Property(r => r.MovementType).HasConversion<string>().HasMaxLength(10);
        builder.Property(r => r.Result).HasConversion<string>().HasMaxLength(15);
        builder.Property(r => r.RejectionReason).HasConversion<string>().HasMaxLength(30);

        builder.HasOne<Credential>().WithMany().HasForeignKey(r => r.CredentialId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AccessPoint>().WithMany().HasForeignKey(r => r.AccessPointId).OnDelete(DeleteBehavior.Restrict);

        // Índices orientados a las consultas del historial y a la validación de secuencia.
        builder.HasIndex(r => r.OccurredAtUtc);
        builder.HasIndex(r => new { r.EmployeeId, r.AccessPointId, r.Result, r.OccurredAtUtc });
        builder.HasIndex(r => new { r.AccessPointId, r.OccurredAtUtc });

        builder.Ignore(r => r.DomainEvents);
        builder.Ignore(r => r.IsAuthorized);
    }
}
