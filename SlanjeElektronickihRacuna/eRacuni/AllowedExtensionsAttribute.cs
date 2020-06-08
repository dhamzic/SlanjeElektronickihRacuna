using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SlanjeElektronickihRacuna.eRacuni
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _Extensions;
        public AllowedExtensionsAttribute(string[] Extensions)
        {
            _Extensions = Extensions;
        }

        protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file!=null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!(file == null))
                {
                    if (!_Extensions.Contains(extension.ToLower()))
                    {
                        return new ValidationResult(GetErrorMessage());
                    }
                } 
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Priložena datoteka mora biti .xml formata.";
        }
    }
}
