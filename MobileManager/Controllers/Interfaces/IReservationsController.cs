using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Reservations;

namespace Controllers.Interfaces
{
    public interface IReservationsController
    {
        [HttpGet("{id}", Name = "getReservation")]
        IActionResult GetById(string id);

        [HttpPost]
        IActionResult Create([FromBody] Reservation item);

        [HttpDelete("{id}")]
        IActionResult Delete(string id);
    }
}
