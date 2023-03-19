﻿namespace BoilerServicesSystem.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Authorization;

    using BoilerServicesSystem.Data;
    using BoilerServicesSystem.Data.Models;
    using BoilerServicesSystem.Data.Models.Enums;

    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = _context.Users
                .First(u => u.UserName == User.Identity.Name);

            List<ServiceRequest>? currentUserRequests = null;

            if (User.IsInRole("User"))
            {
                currentUserRequests = await _context.ServiceRequests
                    .Where(sr => sr.CreatedById == currentUser.Id)
                    .ToListAsync();
            }

            if (User.IsInRole("Tech"))
            {
                currentUserRequests = await _context.ServiceRequests
                    .Where(sr => sr.TechResponsible == null ||
                        sr.TechResponsible.Id == currentUser.Id)
                    .ToListAsync();
            }

            if (User.IsInRole("Administrator"))
            {
                return RedirectToPage("/Administrator/Index");
            }

            return View(currentUserRequests);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ServiceRequests == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeRequest(ServiceRequest service)
        {
            var currentUser = _context.Users
                .First(u => u.UserName == User.Identity.Name);

            service.Status = ServiceStatuses.InProgress;
            service.TechResponsible = currentUser;

            _context.Update(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest)
        {
            var currentUser = _context.Users
                .First(u => u.UserName == User.Identity.Name);

            serviceRequest.Id = Guid.NewGuid().ToString();
            serviceRequest.Status = ServiceStatuses.Waiting;
            serviceRequest.VisitedDate = null;
            serviceRequest.CreatedById = currentUser.Id;
            serviceRequest.Image = new byte[1];

            ModelState.Remove("Image");
            ModelState.Remove("VisitedDate");

            if (ModelState.IsValid)
            {
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(serviceRequest);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ServiceRequests == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            var currentUser = _context.Users
                .First(u => u.UserName == User.Identity.Name);

            if (serviceRequest.CreatedById != currentUser.Id)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
            {
                return NotFound();
            }

            serviceRequest.Status = ServiceStatuses.Waiting;
            serviceRequest.VisitedDate = null;
            serviceRequest.Image = new byte[1];

            ModelState.Remove("Image");
            ModelState.Remove("VisitedDate");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.Id))
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
            return View(serviceRequest);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ServiceRequests == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            if (serviceRequest.Status != ServiceStatuses.Waiting)
            {
                return RedirectToPage("/ServiceRequests/Index");
            }

            var currentUser = _context.Users
                .First(u => u.UserName == User.Identity.Name);

            if (serviceRequest.CreatedById != currentUser.Id)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ServiceRequests == null)
            {
                return Problem("Entity set 'ApplicationDbContext.ServiceRequests'  is null.");
            }
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            var currentUser = _context.Users
                .First(u => u.UserName == User.Identity.Name);

            if (serviceRequest.CreatedById != currentUser.Id)
            {
                return NotFound();
            }

            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceRequestExists(string id)
        {
            return (_context.ServiceRequests?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}