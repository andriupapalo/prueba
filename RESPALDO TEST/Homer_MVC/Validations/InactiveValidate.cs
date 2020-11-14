using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Validations
{
    public class InactiveValidate : ValidationAttribute
    {
        private readonly bool state;


        public InactiveValidate(bool value)
            : base("{0} es obligatorio test.")
        {
            state = value;
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(value.ToString()) || (string)value == "")
            {
                bool stat = state;
                string errorMessage = FormatErrorMessage(validationContext.DisplayName);
                return new ValidationResult(errorMessage);
            }
            else
            {
                return ValidationResult.Success;
            }
        }

    }
}