// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace ShareCare.Models.AccountViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Az email cím vagy felhasználónév mező kitöltése kötelező!")]
    [Display(Name = "Email cím vagy felhasználónév")]
    public string EmailOrUsername { get; set; }

    [Required(ErrorMessage = "A jelszó mező kitöltése kötelező!")]
    [Display(Name = "Jelszó")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Jegyezz meg")]
    public bool RememberMe { get; set; }
}
