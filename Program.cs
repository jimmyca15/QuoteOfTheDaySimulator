using QuoteOfTheDaySimulator;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("What port is your 'Quote of the Day' application running on?");

string portInput = Console.ReadLine();

if (!int.TryParse(portInput, out int port))
{
    throw new ArgumentException("The provided port should be a number. Example: 7206.");
}

using var cts = new CancellationTokenSource();

cts.CancelAfter(TimeSpan.FromMinutes(5));

Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Cancel key pressed.");

    cts.Cancel();
};

Dictionary<Variant, double> simulationPercentages = new Dictionary<Variant, double>()
{
    {
        Variant.On,
        .82
    },
    {
        Variant.Off,
        .14
    }
};

int userCount = 1000;

var users = new User[userCount];

for (int i = 0; i < userCount; i++)
{
    users[i] = new User
    {
        Name = $"user{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 14)}@contoso.com",
        Password = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6) + "Aa1!"
    };
}

var client = new QuoteOfTheDayClient(port);

var random = new Random();

var metrics = new List<Metric>()
{
    new Metric
    {
        Variant = Variant.On
    },
    new Metric
    {
        Variant = Variant.Off
    }
};

int totalSimulatedUsers = 0;

Console.WriteLine("Simulating user:");

try
{
    foreach (User user in users)
    {
        if (cts.IsCancellationRequested)
        {
            break;
        }

        totalSimulatedUsers++;

        Console.Write
            ("\r{0}/{1}       ",
            totalSimulatedUsers,
            userCount);

        await client.RegisterUser(user.Name, user.Password, cts.Token);

        await client.Login(user.Name, user.Password, cts.Token);

        Variant v = await client.GetVariant(cts.Token);

        Metric metric = metrics.First(m => m.Variant == v);

        double rand = random.NextDouble();

        if (rand < simulationPercentages[v])
        {
            await client.LikeQuote(cts.Token);

            metric.LikeCount++;
        }

        metric.UserCount++;

        await client.Logout(cts.Token);
    }
}
catch (OperationCanceledException)
{
    //
    // Graceful shutdown
}

Console.WriteLine("Simulation completed.");

foreach (Metric metric in metrics)
{
    Console.WriteLine(JsonSerializer.Serialize(
        metric,
        new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        }));
}

Console.WriteLine("Shutting down.");