using System;
using Library.Domain.Books.Managers;
using Library.Domain.Books.Repositories;
using Library.Domain.Users.Managers;
using Library.Domain.Users.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Library.Infrastructure;

public class DatabaseSeeder(
    IUsersManager usersManager,
    IUsersRepository usersRepository,
    IBooksManager booksManager,
    IBooksRepository booksRepository
)
{
    public async Task CreateUsers()
    {
        if (await usersRepository.Count(null) != 0) return;
        await usersManager.Create("super_admin", "12345678@sa", Domain.Users.Entities.UserType.Admin);
        await usersManager.Create("ma7moud_kanbar", "12345678@mk", Domain.Users.Entities.UserType.Client);
        await usersManager.Create("yanar_aldaghestani", "12345678@ya", Domain.Users.Entities.UserType.Client);
    }

    public async Task CreateBooks()
    {
        if (await booksRepository.Count(null, null, null) != 0) return;

        await booksManager.AddNew("Clean Code", "Robert C. Martin", "9780132350884");
        await booksManager.AddNew("Effective Java", "Joshua Bloch", "9780321356680");
        await booksManager.AddNew("Design Patterns", "Gamma, Helm, Johnson, Vlissides", "9780201630992");
        await booksManager.AddNew("Head First Design Patterns", "Elisabeth Freeman, Kathy Sierra, Bert Bates", "9780596007126");
        await booksManager.AddNew("Introduction to Algorithms", "Thomas H. Cormen, Charles E. Leiserson, Ronald L. Rivest, Clifford Stein", "9780262033848");
    }
}
