using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TSB100UserProfileService.Validation
{
    public class StringValidator
    {
        internal static void NotNull(List<string> errors, string property, string propertyName)
        {
            if (property == null)
            {
                errors.Add($"Validation error: Required property '{propertyName}' is null");
            }
        }

        internal static void MinMax(List<string> errors, string property, int minLength, int maxLength, string propertyName)
        {
            if (property == null)
            {
                return;
            }

            //Adds an error message as written below to the error list regarding the strings 
            if (property.Length < minLength)
            {
                errors.Add($"Validation error: Property: '{propertyName}', MinLength: {minLength}, ActualStringLength: {property.Length} Data: '{property}'");
            }

            if (property.Length > maxLength)
            {
                errors.Add($"Validation error: Property: '{propertyName}', MaxLength: {maxLength}, ActualStringLength: {property.Length} Data: '{property}'");
            }
        }

        internal static void Email(List<string> errors, string email)
        {
            if (email == null)
            {
                return;
            }
            MinMax(errors, email, 0, 50, "Email");
            //TODO: check if valid email. But... 
            //      Since no central decision has been made about this we risk breaking other 
            //      web services och web sites that depend on us.
        }
    }
}