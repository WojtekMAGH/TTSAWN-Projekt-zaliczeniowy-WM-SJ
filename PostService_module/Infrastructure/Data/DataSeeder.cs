
using System;
using System.Collections.Generic;
using System.Linq;
using PostService_module.Core.Entities;
using PostService_module.Infrastructure.Data;

namespace PostService_module.Infrastructure.Data
{
    public class DataSeeder
    {
        private readonly PostDbContext _dbContext;

        public DataSeeder(PostDbContext context)
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
                    if (!_dbContext.Posts.Any())
                    {
                        SeedPosts();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during seeding: {ex.Message}");
            }
        }

        private void SeedPosts()
        {
            if (!_dbContext.Posts.Any())
            {
                var posts = new List<Post>
        {
            new Post
            {
                UserId = 1,
                Content = "Heyyy Everyone! Wooooooooooow, this app looks amazing",
                IsPublic = true,
                ImageUrl = "/uploads/images/hej1.png",  
                CreatedAt = new DateTime(2023, 11, 25, 17, 0, 0)
            },
            new Post
            {
                UserId = 2,
                Content = "Did you know that \"Cześć Wszystkim\" is a friendly way to say \"Hi Everyone\" in Polish? 🌍\r\nIt's a great greeting to use when you're addressing a group of people in Poland. 😊",
                IsPublic = true,
                ImageUrl = "/uploads/images/hej2.jpg", 
                CreatedAt = new DateTime(2023, 11, 24, 14, 0, 0)
            },
            new Post
            {
                UserId = 1,
                Content = "The sky is so clear today, perfect weather for a long walk. Feeling grateful for the little things in life!",
                IsPublic = true,
                ImageUrl = "/uploads/images/Niebo.png",
                CreatedAt = DateTime.UtcNow
            },
            new Post
            {
                UserId = 2,
                Content = "Finally finished that big project at work! Time to relax and enjoy the weekend. #WorkLifeBalance",
                IsPublic = true,
                ImageUrl = "/uploads/images/Projekt.png",
                CreatedAt = new DateTime(2023, 11, 23, 17, 30, 0)
            },
            new Post
            {
                UserId = 3,
                Content = "Nothing beats the smell of coffee on a crisp morning. Let's tackle this day with energy and positivity!",
                IsPublic = true,
                CreatedAt = new DateTime(2023, 10, 16, 8, 15, 0)
            },
            new Post
            {
                UserId = 4,
                Content = "Midweek motivation: Remember, every small step takes you closer to your goals. Keep going! 💪",
                IsPublic = true,
                CreatedAt = new DateTime(2022, 06, 29, 12, 0, 0)
            },
            new Post
            {
                UserId = 5,
                Content = "The rain this morning was so peaceful. It’s amazing how nature has its own way of soothing the soul.",
                IsPublic = true,
                CreatedAt = new DateTime(2023, 03, 14, 7, 45, 0)
            },
            new Post
            {
                UserId = 6,
                Content = "Just finished an incredible book about personal growth. Highly recommend taking time for some self-reflection.",
                IsPublic = true,
                CreatedAt = new DateTime(2022, 09, 12, 19, 0, 0)
            },
            new Post
            {
                UserId = 7,
                Content = "Had the best study session today! Feeling more confident about my upcoming exams. #StayPositive",
                IsPublic = true,
                CreatedAt = new DateTime(2023, 02, 05, 15, 30, 0)
            },
            new Post
            {
                UserId = 8,
                Content = "Sunset vibes! Grateful for moments like this to just pause and appreciate the beauty around us. 🌅",
                IsPublic = true,
                CreatedAt = new DateTime(2021, 07, 23, 20, 0, 0)
            },
            new Post
            {
                UserId = 9,
                Content = "Feeling proud of myself for starting my morning jogs again. Day 3, and it feels amazing already!",
                IsPublic = false,
                CreatedAt = new DateTime(2023, 08, 13, 6, 45, 0)
            },
            new Post
            {
                UserId = 10,
                Content = "Spent the afternoon planting flowers in the garden. Can't wait to see them bloom in a few weeks! 🌸",
                IsPublic = false,
                CreatedAt = new DateTime(2022, 10, 12, 14, 15, 0)
            },
            new Post
            {
                UserId = 11,
                Content = "Weekend trip to the mountains was everything I needed. Fresh air and good vibes all around!",
                IsPublic = true,
                ImageUrl = "/uploads/images/Weekend.png",
                CreatedAt = new DateTime(2021, 03, 19, 18, 0, 0)
            },
            new Post
            {
                UserId = 12,
                Content = "Monday mindset: A new week, a new opportunity to chase my dreams. Let’s make it count!",
                IsPublic = true,
                CreatedAt = new DateTime(2023, 05, 08, 9, 0, 0)
            }
        };

                _dbContext.Posts.AddRange(posts);
                _dbContext.SaveChanges();
            }
        }


    }
}
