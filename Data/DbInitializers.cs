using System;
using System.Linq;
using Ticketo5.Models;

namespace Ticketo5.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any tickets.
            if (context.Tickets.Any())
            {
                return;   // DB has been seeded
            }

            var tickets = new Ticket[]
            {
                new Ticket{ID=0,description="first ticket", createdOn=DateTime.Parse("2020-07-02"), assignedBy="Jonathan", ownedBy="Jonathan", category=Category.Urgent, name="ToMyself", status=Status.Closed},
                new Ticket{ID=1,description="second ticket", createdOn=DateTime.Parse("2020-08-02"), assignedBy="Jonathan", ownedBy="Wiliiam", category=Category.Urgent, name="ToMyFriend", status=Status.Closed},
                new Ticket{ID=2,description="third ticket", createdOn=DateTime.Parse("2020-07-02"), assignedBy="William", ownedBy="Jovan", category=Category.NonUrgent, name="TestDone", status=Status.InProgress}
            };
            foreach (Ticket t in tickets)
            {
                context.Tickets.Add(t);
            }
            context.SaveChanges();
        }
    }
}
