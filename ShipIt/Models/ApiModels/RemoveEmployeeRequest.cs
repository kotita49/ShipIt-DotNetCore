﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipIt.Models.ApiModels
{
    public class RemoveEmployeeRequest
    {
        public string Name { get; set; }
        public int EmployeeId { get; set; }
    }
}