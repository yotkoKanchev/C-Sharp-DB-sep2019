﻿namespace CarDealer.DTO
{
    public class ImportPartCarDto
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public long TravelledDistance { get; set; }
        public int[] PartsId { get; set; }
    }
}
