using FluentValidation;
using LABPOWER_APC.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LABPOWER_APC.Utilities
{
    public class UPSValidator : AbstractValidator<UPS>
    {
        public UPSValidator()
        {
            RuleFor(ups => ups.InputVoltage)
                .NotNull().WithMessage("InputVoltage nesmí být null.")
                .NotEmpty().WithMessage("InputVoltage nesmí být prázdný.")
                .Must(BeAValidVoltage).WithMessage("InputVoltage musí být platné číslo.");

            RuleFor(ups => ups.OutputVoltage)
                .NotNull().WithMessage("OutputVoltage nesmí být null.")
                .NotEmpty().WithMessage("OutputVoltage nesmí být prázdný.")
                .Must(BeAValidVoltage).WithMessage("OutputVoltage musí být platné číslo.");
                
        }

        private bool BeAValidVoltage(string inputVoltage)
        {
            return double.TryParse(inputVoltage, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _);
        }
    }
}
