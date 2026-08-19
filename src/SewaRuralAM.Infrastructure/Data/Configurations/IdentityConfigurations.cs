using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SewaRuralAM.Core.Entities;

namespace SewaRuralAM.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.Property(u => u.UserName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PasswordHash).IsRequired();
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(r => r.RoleName).IsUnique();
        builder.Property(r => r.RoleName).IsRequired().HasMaxLength(100);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.Property(m => m.MenuName).IsRequired().HasMaxLength(150);

        builder.HasOne(m => m.ParentMenu)
            .WithMany(m => m.SubMenus)
            .HasForeignKey(m => m.ParentMenuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MenuRightConfiguration : IEntityTypeConfiguration<MenuRight>
{
    public void Configure(EntityTypeBuilder<MenuRight> builder)
    {
        builder.HasOne(mr => mr.Menu)
            .WithMany(m => m.MenuRights)
            .HasForeignKey(mr => mr.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.Role)
            .WithMany(r => r.MenuRights)
            .HasForeignKey(mr => mr.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
