using Microsoft.AspNetCore.Mvc;
using OrderApi.Models;
using OrderApi.Services;
using OrderApi.Validation;
using OrderApi.Exceptions;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _Service;

        public OrdersController(IOrderService service)
        {
            _Service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetAll()
        {
            return Ok(_Service.GetAll());
        }

        [HttpGet("{id:int}")]
        public ActionResult<Order> GetById(int id)
        {
            Order? order = _Service.GetById(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Order order)
        {
            try
            {
                OrderValidator.Validate(order);
            }
            catch (OrderValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            Order created = _Service.Create(order);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Order order)
        {
            try
            {
                OrderValidator.Validate(order);
            }
            catch (OrderValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            bool updated = _Service.Update(id, order);
            if (updated == false)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            bool deleted = _Service.Delete(id);
            if (deleted == false)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}