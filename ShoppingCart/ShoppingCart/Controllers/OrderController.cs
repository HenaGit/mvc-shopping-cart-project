using Microsoft.AspNetCore.Mvc;
using ShoppingCart_DataAccess.Repository.IRepository;
using ShoppingCart_Utility.BrainTree;

namespace ShoppingCart.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;
        private readonly IBrainTreeGate _brain;

        public OrderController(
        IOrderHeaderRepository orderHRepo, IOrderDetailRepository orderDRepo, IBrainTreeGate brain)
        {
            _brain = brain;
            _orderDRepo = orderDRepo;
            _orderHRepo = orderHRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
