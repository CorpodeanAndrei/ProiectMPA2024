using Microsoft.AspNetCore.Mvc;
using ProiectMPA.Models;
using GRPCService;
using Grpc.Net.Client;
using Order = GRPCService.Order;
using Grpc.Core;

namespace ProiectMPA.Controllers
{
    public class OrderGrpcController : Controller
    {
        private readonly GrpcChannel _channel;
        public OrderGrpcController()
        {
            _channel = GrpcChannel.ForAddress("https://localhost:7152");
        }
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var client = new OrderService.OrderServiceClient(_channel);
                var response = client.GetAll(new Empty());

                return View(response); // Pass the OrderList to the View
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error fetching orders: {ex.Message}");
                return View();
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProiectMPA.Models.Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var client = new OrderService.OrderServiceClient(_channel);

                    // Map local Order model to gRPC Order model
                    var grpcOrder = new Order
                    {
                        OrderId = order.OrderID,
                        ClientId = order.ClientID ?? 0,
                        CarId = order.CarID ?? 0,
                        OrderDate = order.OrderDate.ToString("yyyy-MM-dd")
                    };

                    client.Insert(grpcOrder);

                    return RedirectToAction(nameof(Index));
                }
                catch (Grpc.Core.RpcException ex)
                {
                    ModelState.AddModelError("", $"Error creating order: {ex.Status.Detail}");
                }
            }
            return View(order);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                var comanda = new OrderService.OrderServiceClient(_channel);
                var grpcOrder = comanda.Get(new OrderId { Id = id });

                // Map gRPC Order to local Order model
                var order = new ProiectMPA.Models.Order
                {
                    OrderID = grpcOrder.OrderId,
                    ClientID = grpcOrder.ClientId,
                    CarID = grpcOrder.CarId,
                    OrderDate = DateTime.Parse(grpcOrder.OrderDate)
                };

                return View(order);
            }
            catch (Grpc.Core.RpcException ex)
            {
                ModelState.AddModelError("", $"Error fetching order: {ex.Status.Detail}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult Edit(ProiectMPA.Models.Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var comanda = new OrderService.OrderServiceClient(_channel);

                    // Map local Order model to gRPC Order model
                    var grpcOrder = new GRPCService.Order
                    {
                        OrderId = order.OrderID,
                        ClientId = order.ClientID ?? 0,
                        CarId = order.CarID ?? 0,
                        OrderDate = order.OrderDate.ToString("yyyy-MM-dd")
                    };

                    comanda.Update(grpcOrder);

                    return RedirectToAction(nameof(Index));
                }
                catch (Grpc.Core.RpcException ex)
                {
                    ModelState.AddModelError("", $"Error updating order: {ex.Status.Detail}");
                }
            }

            return View(order);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                var comanda = new OrderService.OrderServiceClient(_channel);
                var grpcOrder = comanda.Get(new OrderId { Id = id });

                return View(grpcOrder);
            }
            catch (Grpc.Core.RpcException ex)
            {
                ModelState.AddModelError("", $"Error fetching order: {ex.Status.Detail}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                var comanda = new OrderService.OrderServiceClient(_channel);
                var grpcOrder = comanda.Get(new OrderId { Id = id });

                return View(grpcOrder);
            }
            catch (Grpc.Core.RpcException ex)
            {
                ModelState.AddModelError("", $"Error fetching order: {ex.Status.Detail}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var comanda = new OrderService.OrderServiceClient(_channel);
                comanda.Delete(new OrderId { Id = id });

                return RedirectToAction(nameof(Index));
            }
            catch (Grpc.Core.RpcException ex)
            {
                ModelState.AddModelError("", $"Error deleting order: {ex.Status.Detail}");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
