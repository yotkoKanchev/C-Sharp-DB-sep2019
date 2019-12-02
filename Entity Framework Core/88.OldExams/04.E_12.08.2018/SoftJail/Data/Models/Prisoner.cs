﻿namespace SoftJail.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Prisoner
    {
        public int Id { get; set; }

        [MinLength(3), MaxLength(20), Required]
        public string FullName { get; set; }

        [RegularExpression(@"^The [A-Z][a-z]+$"), Required]
        public string Nickname { get; set; }

        [Range(18, 65)]
        public int Age { get; set; }

        public DateTime IncarcerationDate { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal? Bail { get; set; }

        public int? CellId { get; set; }
        public Cell Cell { get; set; }

        public ICollection<Mail> Mails { get; set; } = new HashSet<Mail>();

        public ICollection<OfficerPrisoner> PrisonerOfficers { get; set; } = new HashSet<OfficerPrisoner>();
    }
}