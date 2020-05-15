using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ticketo5.Data;
using Ticketo5.Models;

namespace Ticketo5.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        [Authorize]
        public async Task<IActionResult> Index(
            string status,
            string searchString,
            string currentFilter,
            string sortOrder,
            string myAssignedTicket,
            string myOwnedTicket,
            int? pageNumber)
        {
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            // create selectlist of status
            var statusQry = from d in _context.Tickets
                            orderby d.status
                            select d.status;
            ViewBag.status = new SelectList(statusQry.Distinct());

            // filter ticket
            var tickets = from ticket in _context.Tickets select ticket;
            if (!String.IsNullOrEmpty(searchString))
            {
                tickets = tickets.Where(t => t.name.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(status))
            {
                tickets = tickets.Where(x => x.status == (Status)Enum.Parse(typeof(Status), status));
            }

            if (!String.IsNullOrEmpty(myAssignedTicket))
            {
                if (myAssignedTicket == "true")
                {
                    tickets = tickets.Where(x => x.assignedBy == User.Identity.Name);
                    myOwnedTicket = "false";
                }
            }

            if (!String.IsNullOrEmpty(myOwnedTicket))
            {
                if (myOwnedTicket == "true")
                {
                    tickets = tickets.Where(x => x.ownedBy == User.Identity.Name);
                    myAssignedTicket = "false";
                }

            }
            ViewData["ShowAssigned"] = String.IsNullOrEmpty(myAssignedTicket) ? "false" : myAssignedTicket;
            ViewData["ShowOwned"] = String.IsNullOrEmpty(myOwnedTicket) ? "false" : myOwnedTicket;

            // sorting row
            switch (sortOrder)
            {
                case "name_desc":
                    tickets = tickets.OrderByDescending(t => t.name);
                    break;
                case "Date":
                    tickets = tickets.OrderBy(t => t.createdOn);
                    break;
                case "date_desc":
                    tickets = tickets.OrderByDescending(t => t.createdOn);
                    break;
                default:
                    tickets = tickets.OrderBy(t => t.name);
                    break;
            }

            int pageSize = 5;
            return View(await PaginatedList<Ticket>.CreateAsync(tickets.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // POST: Tickets/Update/5
        [Authorize]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ticket = await _context.Tickets.FindAsync(id);
                    if(ticket.status == Status.Open)
                    {
                        ticket.status = Status.InProgress;
                    } else if (ticket.status == Status.InProgress)
                    {
                        ticket.status = Status.Closed;
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View();
        }

        // GET: Tickets/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize]
        public IActionResult Create()
        {
            //create selectlist of registered users, category, and status
            var userQry = (from u in _context.Users
                           select u.UserName);
            ViewBag.usersList = new SelectList(userQry.Distinct());
            ViewBag.categoryList = new SelectList(Enum.GetValues(typeof(Category)));
            ViewBag.statusList = new SelectList(Enum.GetValues(typeof(Status)));
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("name,description,ownedBy,category")] Ticket ticket)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ticket.createdOn = DateTime.UtcNow;
                    ticket.assignedBy = User.Identity.Name;
                    _context.Add(ticket);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (!AssignedByMe(ticket))
            {
                return NotMyAssignedTicket("edit");
            }

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("name,description,ownedBy,category")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingTicket = await _context.Tickets.FindAsync(id);
                    if (!AssignedByMe(existingTicket))
                    {
                        return NotMyAssignedTicket("edit");
                    }
                    existingTicket.name = ticket.name;
                    existingTicket.description = ticket.description;
                    if (existingTicket.ownedBy != ticket.ownedBy)
                    {
                        existingTicket.ownedBy = ticket.ownedBy;
                        existingTicket.createdOn = DateTime.UtcNow;
                    }
                    existingTicket.category = ticket.category;
                    existingTicket.ID = id;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (!AssignedByMe(ticket))
            {
                return NotMyAssignedTicket("delete");
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (!AssignedByMe(ticket))
            {
                return NotMyAssignedTicket("delete");
            }
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.ID == id);
        }

        private bool AssignedByMe(Ticket ticket)
        {
            return ticket.assignedBy == User.Identity.Name;
        }

        private bool OwnedByMe(Ticket ticket)
        {
            return ticket.ownedBy == User.Identity.Name;
        }

        private IActionResult NotMyAssignedTicket(String action)
        {
            return StatusCode(401, "You can only " + action + " tickets assigned by you");
        }
    }
}
