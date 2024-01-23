using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingApp.Models;

public partial class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Customerid { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required")]
    [Display(Name = "Phone No.")]
    [MaxLength(15)]
    [MinLength(10)]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [NotMapped]
    [DataType(DataType.Password)]
    [Compare("Password",ErrorMessage = "Passwords do not match")]
    [Required(ErrorMessage = "Please enter the password again")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = null!;

    public override string ToString()
    {
        return $"Customer ID: {Customerid}, Name: {Name}, Email: {Email}, Phone: {Phone}, Address: {Address}, Password: {Password}";
    }

    public virtual ICollection<Bookingdetail> Bookingdetails { get; set; } = new List<Bookingdetail>();
}