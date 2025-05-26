using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

public class Student
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "None";
    public Group group { get; set; } = new Group { Name = "None" };
    public override string ToString()
    {
        return $"{Id}.{Name,10} | {group}";
    }
}

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; }
    public override string ToString()
    {
        return $"{Id}.{Name}";
    }
}

public class UniversityContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Group> Group { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "server=localhost;database=University;user=root;password=";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}

class Program
{
    public static Group FindGroupByIndex(List<Group> group, int id)
    {
        foreach (Group groups in group)
        {
            if (groups.Id == id)
                return groups;
        }
        return null;
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;

        using(var context = new UniversityContext())
        {
            context.Database.EnsureCreated();
            /*context.Group.Add(new Group { Name = "P35" });*/
            var groups = context.Group.ToList();
            context.Students.Add(new Student { Name = "Dmitro", group = FindGroupByIndex(groups, 2) });
            context.SaveChanges();
            var student = context.Students.Include(s => s.group).ToList();
            Console.WriteLine("students in base: ");
            Console.WriteLine(string.Join("\n", student));
            Console.WriteLine("group in base: ");
            Console.WriteLine(string.Join("\n", groups));
        }
    }
}
