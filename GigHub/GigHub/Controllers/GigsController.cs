using GigHub.Models;
using GigHub.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GigHub.Controllers
{
    public class GigsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GigsController()
        {
            _context = new ApplicationDbContext();
        }

        // RETURNS: list of gigs
        [Authorize]
        public ActionResult Mine()
        {
            var userId = User.Identity.GetUserId();

            // get the the gigs of the logged-in that 
            // are not cancelled and are in the future
            var gigs = _context.Gigs
                .Where(g => g.ArtistId == userId && g.DateTime > DateTime.Now && !g.IsCanceled)
                .Include(g => g.Genre)
                .ToList();

            return View(gigs);
        }

        // gets all of the gigs that the
        // logged-in user is going/attending
        // RETURNS: GigsViewModel
        [Authorize]
        public ActionResult Attending()
        {
            var userId = User.Identity.GetUserId();

            // get the gigs the logged-in user 
            // is attending/going
            var gigs = _context.Attendances
                .Where(a => a.AttendeeId == userId)
                .Select(a => a.Gig)
                .Include(g => g.Artist)
                .Include(g => g.Genre)
                .ToList();

            // The upcoming gigs of the USER is passed in the GigsViewModel.
            // Actions like attend/follow are only shown if
            // the user is logged-in/authenticated.
            var viewModel = new GigsViewModel()
            {
                UpcomingGigs = gigs,
                ShowActions = User.Identity.IsAuthenticated,
                Heading = "Gigs I'm Attending"
            };

            return View("Gigs", viewModel);
        }

        [Authorize]
        public ActionResult Create()
        {
            // pass in all of the genres
            // for the dropdown list
            var viewModel = new GigFormViewModel
            {
                Genres = _context.Genres.ToList(),
                Heading = "Add a Gig"
            };

            return View("GigForm", viewModel);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();

            // Get the gig based on the id passed on as the parameter.
            // The userId is also used to make sure only the logged-in user
            // that created the gig is able to edit the gig.
            var gig = _context.Gigs.Single(g => g.Id == id && g.ArtistId == userId);

            var viewModel = new GigFormViewModel
            {
                Heading = "Edit A Gig",
                Id = gig.Id,
                Genres = _context.Genres.ToList(),
                Date = gig.DateTime.ToString("d MMM yyyy"),
                Time = gig.DateTime.ToString("HH:mm"),
                Genre = gig.GenreId,
                Venue = gig.Venue
            };

            return View("GigForm", viewModel);
        }

        // Redirects the user to the home page with the search term
        [HttpPost]
        public ActionResult Search(GigsViewModel viewModel)
        {
            return RedirectToAction("Index", "Home", new { query = viewModel.SearchTerm });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GigFormViewModel viewModel)
        {
            // pass in all of the genres
            // for the dropdown list and return to the GigForm view
            // if the model state is not valid
            // with the viewModel so that the user doesn't lose 
            // all of the data inputed in the textboxes
            if (!ModelState.IsValid)
            {
                viewModel.Genres = _context.Genres.ToList();
                return View("GigForm", viewModel);
            }

            // if the model state is valid
            // pass all of the data from the view
            // to create a new gig
            var gig = new Gig
            {
                ArtistId = User.Identity.GetUserId(),
                DateTime = viewModel.GetDateTime(),
                GenreId = viewModel.Genre,
                Venue = viewModel.Venue
            };

            _context.Gigs.Add(gig);
            _context.SaveChanges();

            return RedirectToAction("Mine", "Gigs");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(GigFormViewModel viewModel)
        {
            // pass in all of the genres
            // for the dropdown list and return to the GigForm view
            // if the model state is not valid
            // with the viewModel so that the user doesn't lose 
            // all of the data inputed in the textboxes
            if (!ModelState.IsValid)
            {
                viewModel.Genres = _context.Genres.ToList();
                return View("GigForm", viewModel);
            }

            var userId = User.Identity.GetUserId();

            // get the gig of the that is being updated
            // based on the id passed in the viewModel
            var gig = _context.Gigs
                .Include(u => u.Attendances.Select(a => a.Attendee))
                .Single(g => g.Id == viewModel.Id && g.ArtistId == userId);

            // update/modify the gig 
            gig.Modify(viewModel.GetDateTime(), viewModel.Venue, viewModel.Genre);

            _context.SaveChanges();

            return RedirectToAction("Mine", "Gigs");
        }


        
    }
}