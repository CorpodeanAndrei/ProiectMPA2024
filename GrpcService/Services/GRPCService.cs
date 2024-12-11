using Azure.Core;
using Grpc.Core;
using GrpcService;
using GRPCService;
using DataAccess = ProiectMPA.Data;
using ModelAccess = ProiectMPA.Models;
using Microsoft.EntityFrameworkCore;
using ProiectMPA.Models;


namespace GrpcService.Services
{
    public class GRPCService : OrderService.OrderServiceBase
    {
        private DataAccess.ProiectMPAContext db = null;
        public GRPCService(DataAccess.ProiectMPAContext db)
        {
            this.db = db;
        }

        //public override Task<OrderList> GetAll(Empty empty, ServerCallContext context)
        //{
        //    OrderList pl = new OrderList();
        //    var query = db.Order
        //                    .Include(o => o.Client)
        //                    .Include(o => o.Car)
        //                    .Select(ord => new Order
        //                        {
        //                        OrderId = ord.OrderID,
        //                        ClientId = ord.ClientID.Value,
        //                        CarId = ord.CarID.Value,
        //                        OrderDate = ord.OrderDate.ToString(),
        //                        Client = ord.Client.Name ?? string.Empty,
        //                        Car = ord.Car.Model ?? string.Empty
        //                        });
        //    pl.Item.AddRange(query.ToArray());
        //    return Task.FromResult(pl);
        //}

        public override Task<OrderList> GetAll(Empty empty, ServerCallContext context)
        {
            // Step 1: Query the database
            var orders = db.Order
                .Include(o => o.Client)
                .Include(o => o.Car)
                .ToList(); // Execute the query and load into memory

            // Step 2: Map to gRPC OrderList with null-safe checks
            var orderList = new OrderList();
            orderList.Item.AddRange(orders.Select(ord => new Order
            {
                OrderId = ord.OrderID,
                ClientId = ord.ClientID ?? 0,
                CarId = ord.CarID ?? 0,
                OrderDate = ord.OrderDate.ToString("o"), // ISO format
                Client = ord.Client?.Name ?? string.Empty, // Null-safe
                Car = ord.Car?.Model ?? string.Empty       // Null-safe
            }));

            return Task.FromResult(orderList);
        }


        public override Task<Empty> Insert(Order requestData, ServerCallContext context)
        {
            try
            {
                var client = db.Client.FirstOrDefault(c => c.ClientID == requestData.ClientId);
                var car = db.Car.FirstOrDefault(c => c.ID == requestData.CarId);

                if (client == null || car == null)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ClientID or CarID"));
                }

                var order = new ProiectMPA.Models.Order
                {
                    ClientID = client.ClientID,
                    CarID = car.ID,
                    OrderDate = DateTime.Parse(requestData.OrderDate)
                };

                db.Order.Add(order);
                db.SaveChanges();

                return Task.FromResult(new Empty());
            }
            catch (DbUpdateException dbEx)
            {
                // Log the database update exception
                //_logger.LogError($"Database error: {dbEx.Message}");
                throw new RpcException(new Status(StatusCode.Internal, "Error saving changes to database."));
            }
            catch (Exception ex)
            {
                // General error logging
                //_logger.LogError($"Error inserting order: {ex.Message}");
                throw new RpcException(new Status(StatusCode.Internal, $"Error inserting order: {ex.Message}"));
            }
        }



        public override Task<Order> Get(OrderId requestData, ServerCallContext context)
        {
            var data = db.Order.Include(o => o.Client).Include(o => o.Car).FirstOrDefault(o => o.OrderID == requestData.Id);

            if (data == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));
            }

            var order = new Order()
            {
                OrderId = data.OrderID,
                ClientId = data.ClientID ?? 0,
                CarId = data.CarID ?? 0,
                OrderDate = data.OrderDate.ToString("o"), // Convert DateTime to ISO 8601 string
                Client = data.Client?.Name ?? string.Empty,
                Car = data.Car?.Model ?? string.Empty,
            };

            return Task.FromResult(order);
        }

        public override Task<Empty> Delete(OrderId requestData, ServerCallContext context)
        {
            var data = db.Order.Find(requestData.Id);

            if (data == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));
            }

            db.Order.Remove(data);
            db.SaveChanges();

            return Task.FromResult(new Empty());
        }

        public override Task<Order> Update(Order request, ServerCallContext context)
        {
            var existingOrder = db.Order.Include(o => o.Client).Include(o => o.Car).FirstOrDefault(o => o.OrderID == request.OrderId);

            if (existingOrder == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));
            }

            // Update fields
            //existingOrder.ClientID = request.ClientId == 0 ? null : request.ClientId;
            //existingOrder.CarID = request.CarId == 0 ? null : request.CarId;

            var client = db.Client.FirstOrDefault(c => c.ClientID == request.ClientId);
            var car = db.Car.FirstOrDefault(c => c.ID == request.CarId);

            if (client == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ClientID"));
            }

            if (car == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid CarID"));
            }

            existingOrder.ClientID = client.ClientID;
            existingOrder.CarID = car.ID;
            existingOrder.OrderDate = !string.IsNullOrWhiteSpace(request.OrderDate) ? DateTime.Parse(request.OrderDate) : DateTime.MinValue;
            existingOrder.Client = client;
            existingOrder.Car = car;

            db.SaveChanges();

            return Task.FromResult(new Order
            {
                OrderId = existingOrder.OrderID,
                ClientId = existingOrder.ClientID ?? 0,
                CarId = existingOrder.CarID ?? 0,
                OrderDate = existingOrder.OrderDate.ToString("o"),
                Client = client.Name,
                Car = car.Model
            });

            //Parse OrderDate if provided
            if (!string.IsNullOrWhiteSpace(request.OrderDate))
                {
                    if (DateTime.TryParse(request.OrderDate, out var parsedDate))
                    {
                        existingOrder.OrderDate = parsedDate;
                    }
                    else
                    {
                        throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid OrderDate format"));
                    }
                }

            db.Order.Update(existingOrder);
            db.SaveChanges();

            //Return updated order
           var updatedOrder = new Order()
           {
               OrderId = existingOrder.OrderID,
               ClientId = existingOrder.ClientID ?? 0,
               CarId = existingOrder.CarID ?? 0,
               OrderDate = existingOrder.OrderDate.ToString("o"), // Convert back to ISO 8601 string
               Client = existingOrder.Client?.Name ?? string.Empty,
               Car = existingOrder.Car?.Model ?? string.Empty,
           };

            return Task.FromResult(updatedOrder);
        }
    }
}
