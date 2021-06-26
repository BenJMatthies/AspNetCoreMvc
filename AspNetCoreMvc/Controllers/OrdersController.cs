using AspNetCoreMvc.Models;
using DataLibrary.Data;
using DataLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreMvc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IFoodData _foodData;
        private readonly IOrderData _orderData;

        public OrdersController(IFoodData foodData, IOrderData orderData)
        {
            _foodData = foodData;
            _orderData = orderData;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Defaults to HttpGet
        public async Task<IActionResult> Create()
        {
            var food = await _foodData.GetFood();

            OrderCreateModel model = new OrderCreateModel();

            food.ForEach(x =>
            {
                model.FoodItems.Add(new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                });
            });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create (OrderModel order)
        {
            if (ModelState.IsValid == false)
                return View();

            var food = await _foodData.GetFood();

            order.Total = order.Quantity * food.Where(x => x.Id == order.FoodId).First().Price;

            int id = await _orderData.CreateOrder(order);

            return RedirectToAction("Display", new { id });
        }

        /* Trying to save a trip to the database.
         * Not sure if this just won't work, or I just don't know how to make it work.... 
         * No time to figure it out today :(
        [HttpPost]
        public async Task<IActionResult> Create (OrderCreateModel model)
        {
            if (ModelState.IsValid == false)
                return View();

            var food = model.FoodItems.ToList();

            model.Order.Total = model.Order.Quantity * food.Where(x=> x. //can't access the Id...
        }*/

        public async Task<IActionResult> Display (int id)
        {
            OrderDisplayModel displayOrder = new OrderDisplayModel();
            displayOrder.Order = await _orderData.GetOrderById(id);

            if (displayOrder.Order != null)
            {
                //not efficient because it returns all data for all menu items
                //would be better to get one row by foodId from displayOrder.Order
                var food = await _foodData.GetFood();

                displayOrder.ItemPurchased = food.Where(x => x.Id == displayOrder.Order.FoodId).FirstOrDefault()?.Title;
            }

            return View(displayOrder);
        }

        /*Nope, doesn't work lol
        [HttpPost]
        public async Task<IActionResult> Display (OrderModel order)
        {
            if (ModelState.IsValid == false)
                return View();

            await _orderData.UpdateOrderName(order.Id, order.OrderName);

            return View("Display", new { order.Id });
        }*/

        
        [HttpPost]
        public async Task<IActionResult> Update(int id, string orderName)
        {
            await _orderData.UpdateOrderName(id, orderName);

            return RedirectToAction("Display", new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderData.GetOrderById(id);

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(OrderModel order)
        {
            await _orderData.DeleteOrder(order.Id);

            return RedirectToAction("Create");
        }
    }
}
