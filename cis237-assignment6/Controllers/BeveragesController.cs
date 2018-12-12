/****************************************************************************************
*
* Kyle Nally
* CIS237 T/Th 3:30pm Assignment 6 - Beverage web application using ASP.NET MVC
* 12/11/2018
*
* This is the beverage controller. The logic for handling what happens with the beverages
* (editing, deleting, adding, and filtering) occurs here.
*
*****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using cis237_assignment6.Models;
using Microsoft.Ajax.Utilities;

namespace cis237_assignment6.Controllers
{
    // the user must be authenticated
    [Authorize]
    public class BeveragesController : Controller
    {
        // create a reference to the entities object
        private BeverageKNallyEntities db = new BeverageKNallyEntities();

        // GET: Beverages
        public ActionResult Index()
        {
            // create a variable to hold the Beverages data 
            DbSet<Beverage> BeveragesToFilter = db.Beverages;

            // these strings hold the data in the session.
            // We will be seeing these defaults later when we attempt
            // to create an error page during validation
            string filterName = "";
            string filterPack = "";
            string filterMin = "";
            string filterMax = "";

            // default min and max values for the beverages.
            // I think $10K is a reasonable max value
            decimal min = 0;
            decimal max = 10000;

            // check for nulls and emptiness and assign any values found to the session variable
            if (!String.IsNullOrWhiteSpace((string) Session["session_name"]))
            {
                filterName = (string)Session["session_name"];
            }

            if (!String.IsNullOrWhiteSpace((string)Session["session_pack"]))
            {
                filterPack = (string)Session["session_pack"];
            }

            if (!String.IsNullOrWhiteSpace((string)Session["session_min"]))
            {
                filterMin = (string)Session["session_min"];

                // if we can parse to a decimal, do so
                if (ValidateFields(filterMin))
                {
                    min = decimal.Parse(filterMin);
                }

                // if we can't, show the error page
                else
                {
                    Session["session_min"] = "";
                    return View("~/Views/Beverages/Error.cshtml");
                }
            }

            if (!String.IsNullOrWhiteSpace((string)Session["session_max"]))
            {

                // parse to decimal? great
                filterMax = (string) Session["session_max"];
                if (ValidateFields(filterMax))
                {
                    max = Decimal.Parse(filterMax);
                }

                // can't? go to error
                else
                {
                    Session["session_max"] = "";
                    return View("~/Views/Beverages/Error.cshtml");
                } 
            }

            IEnumerable<Beverage> filtered = BeveragesToFilter.Where(
                beverage => beverage.price >= min &&
                            beverage.price <= max &&
                            beverage.pack.Contains(filterPack) &&
                            beverage.name.Contains(filterName)
            );

            // put the string into the bag so we can show it later
            ViewBag.filterName = filterName;
            ViewBag.filterPack = filterPack;
            ViewBag.filterMin = filterMin;
            ViewBag.filterMax = filterMax;

            return View(filtered.ToList());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Filter()
        {
            // Get the form data. The "name" part of Request.Form.Get("name") 
            // refers to the name of the form control we're referring to
            string name = Request.Form.Get("name");
            string pack = Request.Form.Get("pack");
            string min = Request.Form.Get("min");
            string max = Request.Form.Get("max");

            // now put the data back into the session so other methods can use it
            Session["session_name"] = name;
            Session["session_pack"] = pack;
            Session["session_min"] = min;
            Session["session_max"] = max;

            return RedirectToAction("Index");
        }

        // GET: Beverages/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Beverage beverage = db.Beverages.Find(id);
            if (beverage == null)
            {
                return HttpNotFound();
            }
            return View(beverage);
        }

        // GET: Beverages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Beverages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,pack,price,active")] Beverage beverage)
        {
            // Run a validation method call on the entries
            if (ValidateFields(beverage.id, beverage.name, beverage.pack, beverage.price, beverage.active))
            {
                // try to add the beverage. If it fails (ex., a duplicate id...
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Beverages.Add(beverage);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }

                // ... send the user to the error page.
                catch
                {
                    return View("~/Views/Beverages/Error.cshtml");
                }
            }
            else
            {
                return View("~/Views/Beverages/Error.cshtml");
            }

            return View(beverage);
        }

        // GET: Beverages/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Beverage beverage = db.Beverages.Find(id);
            if (beverage == null)
            {
                return HttpNotFound();
            }
            return View(beverage);
        }

        // POST: Beverages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,pack,price,active")] Beverage beverage)
        {
            // validate the fields. If it succeeds...
            if (ValidateFields(beverage.name, beverage.pack, beverage.price, beverage.active))
            {
                // try to save the changes...
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Entry(beverage).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }

                // ... if it fails, burp
                catch
                {
                    return View("~/Views/Beverages/Error.cshtml");
                }
            }
            else
            {
                return View("~/Views/Beverages/Error.cshtml");
            }

            return View(beverage);
        }

        // GET: Beverages/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Beverage beverage = db.Beverages.Find(id);
            if (beverage == null)
            {
                return HttpNotFound();
            }
            return View(beverage);
        }

        // POST: Beverages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Beverage beverage = db.Beverages.Find(id);
            db.Beverages.Remove(beverage);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // validation overload method for prices in the filter
        public bool ValidateFields(string priceToValidate)
        {
            try
            {
                decimal.Parse(priceToValidate);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // validation overload method for adding a new beverage
        public bool ValidateFields(string id, string name, string pack, decimal price, bool active)
        {
            if (id.IsNullOrWhiteSpace() ||
                name.IsNullOrWhiteSpace() ||
                pack.IsNullOrWhiteSpace() ||
                price.ToString() == null ||
                active.ToString() == null)
            {
                return false;
            }

            return true;
        }

        // validation overload method for editing a beverage
        public bool ValidateFields(string name, string pack, decimal price, bool active)
        {
            if (name.IsNullOrWhiteSpace() ||
                pack.IsNullOrWhiteSpace() ||
                price.ToString() == null ||
                active.ToString() == null)
            {
                return false;
            }

            return true;
        }
    }
}
