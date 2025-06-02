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
        return $"{Id}.{Name,5} | {curator}";
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
            Console.SetCursorPosition(0, 1);
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
        context.Students.Add(new Student { Name = "Кирило", group = context.Group.Find(rnd.Next(1, 3)), AVG = rnd.Next(1, 13) });
        context.SaveChanges();
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;

        using (var context = new UniversityContext())
        {
            context.Database.EnsureCreated();
            // context.Curator.Add(new Curator { Name = "Петров П.П." });
            // context.Group.Add(new Group { Name = "P35", curator = context.Curator.Find(1) });
            // context.Students.Add(new Student { Name = "Кирило", group = context.Group.Find(1) });
            // context.SaveChanges();

            while (true)
            {
                Console.Clear();
                uint action = Menu(new List<string> {
                "Переглянути всіх студентів",
                "Додати студента",
                "Редагувати студента",
                "Видалити студента",
                "Вихід"
            });

                Console.Clear();
                switch (action)
                {
                    case 0:
                        {
                            var students = context.Students.Include(s => s.group).ToList();
                            Console.WriteLine("Список студентів:");
                            Console.WriteLine(string.Join("\n", students));
                            Console.ReadKey();
                            break;
                        }
                    case 1:
                        {
                            var groups = context.Group.Include(g => g.curator).ToList();
                            List<string> groupNames = new List<string>();
                            foreach (var g in groups)
                                groupNames.Add(g.Name);
                            Console.Write("Введіть ім'я студента: ");
                            string name = Console.ReadLine();
                            Console.Write("Середній бал (1-12): ");
                            int avg = int.Parse(Console.ReadLine());
                            int gIndex = (int)Menu(groupNames);
                            context.Students.Add(new Student { Name = name, AVG = avg, group = groups[gIndex] });
                            context.SaveChanges();
                            break;
                        }
                    case 2:
                        {
                            var students = context.Students.Include(s => s.group).ToList();
                            List<string> studentNames = new List<string>();
                            foreach (var s in students)
                                studentNames.Add($"{s.Id}. {s.Name}");
                            if (studentNames.Count == 0)
                            {
                                Console.WriteLine("Немає студентів для редагування."); 
                                Console.ReadKey(); 
                                break;
                            }
                            var student = students[(int)Menu(studentNames)];
                            Console.WriteLine($"Редагуємо: {student}");
                            Console.Write("Нове ім'я: ");
                            string inputName = Console.ReadLine();
                            if (inputName != null && inputName != "")
                                student.Name = inputName;
                            Console.Write("Новий середній бал: ");
                            string inputAVG = Console.ReadLine();
                            if (inputAVG != null && inputAVG != "")
                            {
                                int newAVG = 0;
                                bool correct = true;
                                for (int i = 0; i < inputAVG.Length; i++)
                                {
                                    if (inputAVG[i] < '0' || inputAVG[i] > '1')
                                        correct = false;
                                }
                                if (correct)
                                {
                                    newAVG = int.Parse(inputAVG);
                                    student.AVG = newAVG;
                                }
                            }
                            var groups = context.Group.Include(g => g.curator).ToList();
                            List<string> groupNames = new List<string>();
                            foreach (var g in groups)
                                groupNames.Add(g.Name);
                            Console.WriteLine("Оберіть нову групу:");
                            student.group = groups[(int)Menu(groupNames)];
                            context.SaveChanges();
                            break;
                        }
                    case 3:
                        {
                            var students = context.Students.Include(s => s.group).ToList();
                            List<string> studentNames = new List<string>();
                            foreach (var s in students)
                                studentNames.Add($"{s.Id}. {s.Name}");
                            if (studentNames.Count == 0)
                            {
                                Console.WriteLine("Немає студентів для видалення."); 
                                Console.ReadKey(); 
                                break;
                            }
                            var student = students[(int)Menu(studentNames)];
                            Console.WriteLine($"Видалити {student.Name}? (Y/N)");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                context.Students.Remove(student);
                                context.SaveChanges();
                            }
                            break;
                        }
                    case 4:
                        return;
                }
            }
        }
    }
}
