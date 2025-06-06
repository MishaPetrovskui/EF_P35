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

public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; } = "None";
    public int Age { get; set; } = 0;

    public Doctor Doctor { get; set; } = new Doctor();

    public override string ToString()
    {
        return $"{Id}. {Name}, Вік: {Age} | Лікар: {Doctor.Name}";
    }
}

public class UniversityContext : DbContext
{
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<Patient> Patients { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "server=localhost;database=doctors;user=root;password=";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}

class Program
{
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
                    new Doctor { Name = "Андрій Андрійович", Specialization = specs[2], Salary = 40000 }
                );
                context.SaveChanges();
            }
            if (context.Patients.Count() == 0)
            {
                var doctors = context.Doctors.ToList();
                context.Patients.AddRange(
                    new Patient { Name = "Петро Сидоров", Age = 45, Doctor = doctors[0] },
                    new Patient { Name = "Марія Коваленко", Age = 30, Doctor = doctors[1] },
                    new Patient { Name = "Антон Шевченко", Age = 60, Doctor = doctors[2] }
                );
                context.SaveChanges();
            }

            while (true)
            {
                Console.Clear();
                var doctors = context.Doctors.Include(d => d.Specialization).ToList();
                List<string> doctorMenu = new List<string>();
                foreach (var doc in doctors)
                    doctorMenu.Add($"{doc.Id}. {doc.Name} | {doc.Specialization.Name} | {doc.Salary} грн");
                doctorMenu.Add("Додати нового лікаря");
                doctorMenu.Add("Exit");
                uint choice = Menu(doctorMenu);
                if (choice == doctorMenu.Count - 1)
                    break;
                else if (choice == doctorMenu.Count - 2)
                {
                    Console.Clear();
                    Console.WriteLine("Додавання нового лікаря:");
                    Console.Write("Ім'я лікаря: ");
                    string name = Console.ReadLine();
                    Console.Write("Зарплата: ");
                    double salary = double.Parse(Console.ReadLine());
                    var specs = context.Specializations.ToList();
                    List<string> specMenu = new List<string>();
                    foreach (var spec in specs)
                        specMenu.Add(spec.Name);
                    Console.WriteLine("Оберіть спеціалізацію:");
                    int specIndex = (int)Menu(specMenu);
                    context.Doctors.Add(new Doctor
                    {
                        Name = name,
                        Salary = salary,
                        Specialization = specs[specIndex]
                    });
                    context.SaveChanges();
                }
                else
                {
                    Doctor selectedDoctor = doctors[(int)choice];
                    Console.Clear();
                    Console.WriteLine($"Лікар: {selectedDoctor}");
                    uint editChoice = Menu(new List<string> { "Редагувати", "Видалити", "Назад" });

                    switch (editChoice)
                    {
                        case 0:
                            Console.Write("Нове ім'я (Enter — залишити): ");
                            string newName = Console.ReadLine();
                            if (newName != null && newName != "")
                                selectedDoctor.Name = newName;
                            Console.Write("Нова зарплата (Enter — залишити): ");
                            string newSalaryInput = Console.ReadLine();
                            if (double.TryParse(newSalaryInput, out double newSalary))
                                selectedDoctor.Salary = newSalary;
                            var specs = context.Specializations.ToList();
                            List<string> specMenu = new List<string>();
                            foreach (var spec in specs)
                                specMenu.Add(spec.Name);
                            Console.WriteLine("Оберіть нову спеціалізацію:");
                            int specIndex = (int)Menu(specMenu);
                            selectedDoctor.Specialization = specs[specIndex];

                            context.SaveChanges();
                            break;

                        case 1: 
                            Console.WriteLine($"Видалити {selectedDoctor.Name}? (Y/N)");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                context.Doctors.Remove(selectedDoctor);
                                context.SaveChanges();
                            }
                            break;

                        case 2: break;
                    }
                }
            }
        }
    }
}
