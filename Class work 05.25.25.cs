using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

public class Student
{
    public int Id { get; set; } = 0;
    public int AVG { get; set; } = 0;
    public string Name { get; set; } = "None";
    public Group group { get; set; } = new Group { Name = "None" };
    public override string ToString()
    {
        return $"{Id}.{Name,10} | {group} | {AVG}";
    }
}

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Curator curator { get; set; }
    public override string ToString()
    {
        return $"{Id}.{Name, 5} | {curator}";
    }
}

public class Curator
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
    public DbSet<Student> Students { get; set; }
    public DbSet<Group> Group { get; set; }
    public DbSet<Curator> Curator { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "server=localhost;database=University;user=root;password=";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}

class Program
{
    public static Random rnd = new Random();
    public static Group FindGroupByIndex(List<Group> group, int id)
    {
        foreach (Group groups in group)
        {
            if (groups.Id == id)
                return groups;
        }
        return null;
    }

    public static uint Menu(IEnumerable<string> Action)
    {
        uint active = 0;
        while (true)
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < Action.Count(); i++)
            {

                if (i == active)
                    Console.WriteLine($" > {Action.ElementAt(i)}");
                else
                    Console.WriteLine($"   {Action.ElementAt(i)}");
            }

            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (active > 0 && (key == ConsoleKey.UpArrow || key == ConsoleKey.W))
                    active--;
                else if ((key == ConsoleKey.DownArrow || key == ConsoleKey.S) && active < Action.Count() - 1)
                    active++;
                else if (key == ConsoleKey.Enter)
                {
                    //Console.Clear();
                    return active;
                }
            }
        }
    }

    public static void GenereteStudentWithRandomAVG(UniversityContext context)
    {
        context.Students.Add(new Student { Name = "Кирило", group = context.Group.Find(rnd.Next(1,3)), AVG=rnd.Next(1,13) });
        context.SaveChanges();
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;

        using(var context = new UniversityContext())
        {
            context.Database.EnsureCreated();
            // context.Curator.Add(new Curator { Name = "Петров П.П." });
            // context.Group.Add(new Group { Name = "P35", curator = context.Curator.Find(1) });
            // context.Students.Add(new Student { Name = "Кирило", group = context.Group.Find(1) });
            context.SaveChanges();

            // FIRST

            /*var student = context.Students.Include(s => s.group).ToList();//.Where(s => s.group != null && s.group.Id == 2);
            // var student = context.Students.Include(s => s.group).ToList();
            var curators = context.Curator.ToList();
            Console.WriteLine("students in base: ");
            Console.WriteLine(string.Join("\n", student));
            Console.WriteLine("curators in base: ");
            Console.WriteLine(string.Join("\n", curators));

            Console.Write(">> Input curators name: ");
            string a = Console.ReadLine();

            foreach (var curator in curators)
            {
                if (a == curator.Name)
                {
                    var b = context.Students.Include(s => s.group).Where(s => s.group != null && s.group.curator.Id == curator.Id);
                    Console.WriteLine(string.Join("\n", b));
                }
            }*/

            //SECOND

            /*var curator = context.Curator.ToList();
            var group = context.Group.ToList();
            List<string> list = new List<string>();
            foreach (var curators in curator)
            {
                list.Add(curators.Name);
            }
            int i = (int)Menu(list) + 1;
            var b = context.Students.Include(s => s.group).Where(s => s.group != null && s.group.curator.Id == i);
            Console.WriteLine(string.Join("\n", b));*/

            // THIRD
            
            var students = context.Students.ToList();
            int i = (int)(students.Count * 0.45);
            Console.WriteLine(i);
            //GenereteStudentWithRandomAVG(context);
            var a = context.Students.Include(s => s.group).OrderByDescending(s => s.AVG);
            Console.WriteLine(string.Join("\n", a));
            Console.WriteLine();
            var b = context.Students.Include(s => s.group).OrderByDescending(s => s.AVG).Take(i);
            Console.WriteLine(string.Join("\n", b));
        }
    }
}
