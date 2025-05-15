using EventTestTask.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTestTask.Infrastructure.ApplicationContext.Configurations;

public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Events);
        
        builder.HasOne(r => r.Event)
            .WithMany(e => e.Participants);
    }
}