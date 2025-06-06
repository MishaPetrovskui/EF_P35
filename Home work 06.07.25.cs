using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; } = "None";
    public Specialization Specialization { get; set; } = new Specialization { Name = "None" };
    public double Salary { get; set; } = 0;

    public override string ToString()
    {
        return $"{Id}. {Name,10} | {Specialization} | Зарплата: {Salary} грн";
    }
}


public class Specialization
{
    public int Id { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return $"{Id}. {Name}";
    }
}

public class UniversityContext : DbContext
{
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Specialization> Specializations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "server=localhost;database=doctors;user=root;password=";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;

        using (var context = new UniversityContext())
        {
            context.Database.EnsureCreated();
            if (context.Specializations.Count() == 0)
            {
                context.Specializations.AddRange(
                    new Specialization { Name = "Кардіолог" },
                    new Specialization { Name = "Невролог" },
                    new Specialization { Name = "Хірург" }
                );
                context.SaveChanges();
            }

            if (context.Doctors.Count() == 0)
            {
                var specs = context.Specializations.ToList();
                context.Doctors.AddRange(
                    new Doctor { Name = "Іван Іванов", Specialization = specs[0], Salary = 30000 },
                    new Doctor { Name = "Олена Петрівна", Specialization = specs[1], Salary = 25000 },
                    new Doctor { Name = "Андрій Андрійович", Specialization = specs[2], Salary = 40000 },
                    new Doctor { Name = "Марія Василівна", Specialization = specs[0], Salary = 20000 }
                );
                context.SaveChanges();
            }

            Console.WriteLine("Лікарі за зарплатнею (від більшої до меншої):");
            var doctorsOrdered = context.Doctors.Include(d => d.Specialization).OrderByDescending(d => d.Salary).ToList();
            Console.WriteLine(string.Join("\n", doctorsOrdered));

            var averageSalary = context.Doctors.Average(d => d.Salary);
            Console.WriteLine($"\nСередня зарплата: {averageSalary} грн");
            Console.WriteLine("\nЛікарі із зарплатнею нижчою за середню:");
            var belowAverage = context.Doctors.Include(d => d.Specialization).Where(d => d.Salary < averageSalary).OrderBy(d => d.Salary).ToList();
            Console.WriteLine(string.Join("\n", belowAverage));
        }
    }
}
