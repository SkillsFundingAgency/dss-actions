using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Action.Models;

namespace NCS.DSS.Action.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IAction resource, bool validateModelForPost);
    }
}