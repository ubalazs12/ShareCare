// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace ShareCare.Models.AccountViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Az email cím mező kitöltése kötelező!")]
    [EmailAddress(ErrorMessage = "Az email cím formátuma nem megfelelő!")]
    [Display(Name = "Email cím")]
    public string Email { get; set; }

    [Required(ErrorMessage = "A felhasználónév mező kitöltése kötelező!")]
    [Display(Name = "Felhasználónév")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "A vezetéknév mező kitöltése kötelező!")]
    [Display(Name = "Vezetéknév")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "A keresztnév mező kitöltése kötelező!")]
    [Display(Name = "Keresztnév")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "A jelszó mező kitöltése kötelező!")]
    [StringLength(100, ErrorMessage = "A jelszónak legalább {2} karakter hosszúságúnak kell lennie!", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Jelszó")]
    public string Password { get; set; }

    [Required(ErrorMessage = "A jelszó újra mező kitöltése kötelező!")]
    [DataType(DataType.Password)]
    [Display(Name = "Jelszó újra")]
    [Compare("Password", ErrorMessage = "A jelszó megerősítése sikertelen!")]
    public string ConfirmPassword { get; set; }
}
