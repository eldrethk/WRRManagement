using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WRR.Admin.Extension
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        public string StartDate { get; private set; }
        public DateGreaterThanAttribute(string startdate)
        {
            this.StartDate = startdate;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(StartDate);
            if (property == null)
                return new ValidationResult("Dates are invalid");

            try
            {
                if (value == null)
                    return new ValidationResult("End date is invalid");

                DateTime startDate = Convert.ToDateTime(property.GetValue(validationContext.ObjectInstance));
                DateTime endDate = Convert.ToDateTime(value);

                if (endDate <= startDate)
                    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
                else
                    return null; //implemented as ValidationResult.Success 
                                 //ValidationResult.Success;
            }
            catch
            {
                return new ValidationResult("Invalid Date Entry");
            }
        }
    }

    public class ValidCCExpiration : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {

            if (value == null)
                return new ValidationResult("Expire date is invalid");

            string expiredate = value?.ToString() ?? string.Empty;
            try
            {
                if (string.IsNullOrEmpty(expiredate))
                    return new ValidationResult("Expire date is invalid");

                string[] expire = expiredate.Split('/');
                int month = Convert.ToInt32(expire[0]);
                int year = Convert.ToInt32(expire[1]);

                if (month >= 1 && month <= 12)
                {
                    if (year.ToString().Length < 4)
                    {
                        year = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(year);
                    }
                    if (year == DateTime.Today.Year)
                    {
                        if (month <= DateTime.Today.Month)
                        {
                            return new ValidationResult("Credit card has expired");
                        }
                        else
                        {
                            return ValidationResult.Success;
                        }
                    }
                    else if (year >= DateTime.Today.Year && year <= DateTime.Today.AddYears(10).Year)
                        return ValidationResult.Success;
                    else
                        return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
                }
                else
                {
                    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
                }

            }
            catch
            {
                return new ValidationResult("Expiration date is invalid");
            }

        }
    }

    public class DateGreaterThanTodayAttribute : ValidationAttribute
    {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Date is invalid");
            DateTime Date = Convert.ToDateTime(value);


            try
            {
                int comparevalue = DateTime.Compare(Date, DateTime.Today);
                if (comparevalue < 0)
                    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
                else
                    return ValidationResult.Success;
            }
            catch
            {
                return new ValidationResult("This Date must be greater than today");
            }

        }
    }

    public class NumGreaterThanOrEqualAttribute : ValidationAttribute
    {
        public string MinNum { get; private set; }
        public NumGreaterThanOrEqualAttribute(string minNum)
        {
            this.MinNum = minNum;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(MinNum);
            if (property == null)
                return new ValidationResult("Numbers are invalid");
            try
            {
                int minNum = Convert.ToInt32(property.GetValue(validationContext.ObjectInstance));
                int maxNum = Convert.ToInt32(value);

                return (minNum <= maxNum) ? null : new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }
            catch
            {
                return new ValidationResult("Invalid Numbers");
            }
        }
    }

    public class AtLeastOneSelectedAttribute : ValidationAttribute
    {
        public string[] PropertyNames { get; private set; }

        public AtLeastOneSelectedAttribute(params string[] propertyNames)
        {
            this.PropertyNames = propertyNames;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var properties = PropertyNames.Select(validationContext.ObjectType.GetProperty);
            var values = properties.Select(p => p?.GetValue(validationContext.ObjectInstance)).OfType<bool>();

            if (values.Any(v => v))
            {
                return null; //ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }

        }

    }
}
