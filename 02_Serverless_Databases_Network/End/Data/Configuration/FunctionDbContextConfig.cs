using Domain;

using Microsoft.EntityFrameworkCore;

namespace Data
{
    public static class FunctionDbContextConfig
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            var person = modelBuilder.Entity<Person>();
            person.Property(x => x.Id).ValueGeneratedOnAdd();
            person.Property(x => x.Name);
            person.Property(x => x.Prediction);
        }
    }
}
