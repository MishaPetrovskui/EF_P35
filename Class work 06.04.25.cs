
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

public class Category
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "None";
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Category category { get; set; }
    public double price { get; set; }
    public int InStock { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public List<ProductInOrder> products { get; set; }
    public double TotalPrice { get; set; }
}

public class ProductInOrder
{
    public int Id { get; set; }
    public Product Product { get; set; }
    public Order Order { get; set; }
}

public class UniversityContext : DbContext
{
    public DbSet<Category> Category { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<ProductInOrder> ProductInOrder { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "server=localhost;database=shop;user=root;password=";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}

class Program
{
    public static Random rnd = new Random();

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

    static void AddCategory (UniversityContext context)
    {
        Console.Write("Write name category: ");
        string a = Console.ReadLine();
        context.Category.Add(new Category { Name = a });
        context.SaveChanges();
    }

    static void AddProduct (UniversityContext context)
    {
        var category = context.Category.ToList();
        List<string> productsNames = new List<string>();
        foreach (var g in category)
            productsNames.Add(g.Name);
        Console.Write("Введіть назву продукту: ");
        string name = Console.ReadLine();
        Console.WriteLine("Ведіть категорію продукту: ");
        int gIndex = (int)Menu(productsNames);
        Console.Write("Введіть ціну продукту: ");
        int price = int.Parse(Console.ReadLine());
        Console.Write("Введіть кількість продукту: ");
        int quantity = int.Parse(Console.ReadLine());
        context.Product.Add(new Product { Name = name, category = category[gIndex], price=price, InStock=quantity  } );
        context.SaveChanges();
    }

    static void EditProduct (UniversityContext context)
    {
        var products = context.Product.ToList();
        List<string> productsNames = new List<string>();
        foreach (var s in products)
            productsNames.Add($"{s.Id}. {s.Name}");
        if (productsNames.Count == 0)
        {
            Console.WriteLine("Немає продуктів для редагування.");
            Console.ReadKey();
        }
        else
        {
            var product = products[(int)Menu(productsNames)];
            Console.WriteLine($"Редагуємо: {product}");
            Console.Write("Нова назва: ");
            string inputName = Console.ReadLine();
            if (inputName != null && inputName != "")
                product.Name = inputName;
            var products1 = context.Category.ToList();
            List<string> productsNames1 = new List<string>();
            foreach (var g in products1)
                productsNames1.Add(g.Name);
            Console.WriteLine("Оберіть нову категорію:");
            product.category = products1[(int)Menu(productsNames1)];
            Console.Write("Введіть ціну продукту: ");
            int price = int.Parse(Console.ReadLine());
            if (price != null)
                product.price = price;
            Console.Write("Введіть кількість продукту: ");
            int quantity = int.Parse(Console.ReadLine());
            if (quantity != null)
                product.InStock = quantity;
            context.SaveChanges();
        }
    }

    static void RemoveProduct (UniversityContext context)
    {
        var products = context.Product.Include(s => s.category).ToList();
        List<string> productsNames = new List<string>();
        foreach (var s in products)
            productsNames.Add($"{s.Id}. {s.Name}");
        if (productsNames.Count == 0)
        {
            Console.WriteLine("Немає продуктів для редагування.");
            Console.ReadKey();
        }
        else
        {
            var product = products[(int)Menu(productsNames)];
            Console.WriteLine($"Видалити {product.Name}? (Y/N)");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                context.Product.Remove(product);
                context.SaveChanges();
            }
        }
    }

    static void AddOrder (UniversityContext context)
    {
        var products = context.Category.Include(g => g.Name).ToList();
        List<string> productsNames = new List<string>();
        foreach (var g in products)
            productsNames.Add(g.Name);
        Console.Write("Введіть назву продукту: ");
        string name = Console.ReadLine();
        Console.WriteLine("Ведіть категорію продукту: ");
        int gIndex = (int)Menu(productsNames);
        Console.Write("Введіть ціну продукту: ");
        int price = int.Parse(Console.ReadLine());
        Console.Write("Введіть кількість продукту: ");
        int quantity = int.Parse(Console.ReadLine());
        context.Product.Add(new Product { Name = name, category = products[gIndex], price = price, InStock = quantity });
        context.SaveChanges();
    }

    static List<ProductInOrder> AddProductInOrder (UniversityContext context)
    {
        List<ProductInOrder> ProductInOrder = new List<ProductInOrder>();
        while (true)
        {
            var products = context.Product.Include(s => s.category).ToList();
            List<string> productsNames = new List<string>();
            foreach (var s in products)
                productsNames.Add($"{s.Id}. {s.Name}");
            Console.WriteLine("Ведіть категорію продукту: ");
            int gIndex = (int)Menu(productsNames);
            ProductInOrder.Add(new ProductInOrder { Product = products[gIndex] });
            Console.WriteLine($"Добавити ще? (Y/N)");
            if (!(Console.ReadKey().Key == ConsoleKey.Y))
            {
                break;
            }
        }
        return ProductInOrder;
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

            //AddCategory(context);
            // AddProduct(context);
            /*context.Product.Add(new Product { Name = "мікрохвильовка", category = context.Category.Find(1), price = 10, InStock = 1 });
            context.SaveChanges();*/
            EditProduct(context);
        }
    }
}
