﻿using NCS.DSS.Action.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Action.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IAction resource, bool validateModelForPost);
    }
}