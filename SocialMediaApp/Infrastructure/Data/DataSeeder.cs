
using System;
using System.Collections.Generic;
using System.Linq;
using UserService.Core.Entities;
using UserService.Infrastructure.Data;

namespace SaleKiosk.Infrastructure
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _dbContext;

        public DataSeeder(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public void Seed()
        {
            try
            {
                _dbContext.Database.EnsureCreated();

                if (_dbContext.Database.CanConnect())
                {
                    if (!_dbContext.Users.Any())
                    {
                        SeedUsers();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during seeding: {ex.Message}");
            }
        }

        private void SeedUsers()
        {
            if (!_dbContext.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@example.com",
                        RegisteredAt = new DateTime(2023, 11, 24, 14, 0, 0)

                    },
                    new User
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "jane.smith@example.com",
                        RegisteredAt = new DateTime(2022, 01, 03, 14, 0, 0)

                    },
                    new User
                    {
                        FirstName = "Michael",
                        LastName = "Johnson",
                        Email = "michael.johnson@example.com",
                        RegisteredAt = new DateTime(2023, 10, 15, 9, 30, 0)
                    },
                    new User
                    {
                        FirstName = "Emily",
                        LastName = "Davis",
                        Email = "emily.davis@example.com",
                        RegisteredAt = new DateTime(2022, 06, 25, 16, 45, 0)
                    },
                    new User
                    {
                        FirstName = "Robert",
                        LastName = "Wilson",
                        Email = "robert.wilson@example.com",
                        RegisteredAt = new DateTime(2023, 03, 14, 12, 0, 0)
                    },
                    new User
                    {
                        FirstName = "Sophia",
                        LastName = "Martinez",
                        Email = "sophia.martinez@example.com",
                        RegisteredAt = new DateTime(2022, 09, 08, 14, 15, 0)
                    },
                    new User
                    {
                        FirstName = "William",
                        LastName = "Brown",
                        Email = "william.brown@example.com",
                        RegisteredAt = new DateTime(2021, 11, 24, 11, 0, 0)
                    },
                    new User
                    {
                        FirstName = "Olivia",
                        LastName = "Garcia",
                        Email = "olivia.garcia@example.com",
                        RegisteredAt = new DateTime(2023, 02, 05, 13, 20, 0)
                    },
                    new User
                    {
                        FirstName = "James",
                        LastName = "Lee",
                        Email = "james.lee@example.com",
                        RegisteredAt = new DateTime(2022, 04, 17, 10, 0, 0)
                    },
                    new User
                    {
                        FirstName = "Ava",
                        LastName = "Lopez",
                        Email = "ava.lopez@example.com",
                        RegisteredAt = new DateTime(2021, 07, 22, 18, 0, 0)
                    },
                    new User
                    {
                        FirstName = "Christopher",
                        LastName = "Harris",
                        Email = "christopher.harris@example.com",
                        RegisteredAt = new DateTime(2023, 08, 12, 19, 30, 0)
                    },
                    new User
                    {
                        FirstName = "Isabella",
                        LastName = "Clark",
                        Email = "isabella.clark@example.com",
                        RegisteredAt = new DateTime(2022, 10, 11, 14, 0, 0)
                    },
                    new User
                    {
                        FirstName = "Daniel",
                        LastName = "Young",
                        Email = "daniel.young@example.com",
                        RegisteredAt = new DateTime(2021, 03, 18, 15, 30, 0)
                    },
                    new User
                    {
                        FirstName = "Mia",
                        LastName = "King",
                        Email = "mia.king@example.com",
                        RegisteredAt = new DateTime(2023, 05, 06, 11, 15, 0)
                    }
                };


                _dbContext.Users.AddRange(users);
                _dbContext.SaveChanges();
            }
        }


    }
}
