using System.CommandLine;
using SportReservation.Models;
using SportReservation.Services;

namespace SportReservation;

public class Cli
{
    public static async Task Run(string[] args, IServiceScope scope)
    {
        Command registerAdmin = new Command("register-admin", "Register admin")
        {
            Arguments =
            {
                new Argument<string>("email"),
                new Argument<string>("fullname"),
                new Argument<string>("password")
            }
        };

        RootCommand root = new("CLI for administrators");
        root.Subcommands.Add(registerAdmin);

        ParseResult parse = root.Parse(args);

        if (parse.Errors.Count != 0)
        {
            foreach (var parseError in parse.Errors)
            {
                await Console.Error.WriteLineAsync(parseError.Message);
            }

            return;
        }

        if (parse.CommandResult.Command.Name == "register-admin")
        {
            var email = parse.GetValue<string>("email")!;
            var fullname = parse.GetValue<string>("fullname")!;
            var password = parse.GetValue<string>("password")!;

            try
            {
                await scope.ServiceProvider.GetRequiredService<UserService>().Register(
                    new RegisterDto(email, fullname, password),
                    UserRole.Admin
                );
            }
            catch (BadHttpRequestException exception)
            {
                Console.WriteLine($"Error when creating admin: {exception.Message}");
                return;
            }

            Console.WriteLine($"Successfully registered admin: {fullname}");
        }
    }
}