﻿namespace Cinema.DataProcessor
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Globalization;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;
    
    using Newtonsoft.Json;

    using Data;
    using Data.Models;
    using Data.Models.Enums;
    using DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";
        private const string SuccessfulImportMovie
            = "Successfully imported {0} with genre {1} and rating {2}!";
        private const string SuccessfulImportHallSeat
            = "Successfully imported {0}({1}) with {2} seats!";
        private const string SuccessfulImportProjection
            = "Successfully imported projection {0} on {1}!";
        private const string SuccessfulImportCustomerTicket
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var movieDtos = JsonConvert.DeserializeObject<ImportMovieDto[]>(jsonString);
            var movies = new List<Movie>();

            foreach (var dto in movieDtos)
            {
                var genreIsValid = Enum.IsDefined(typeof(Genre), dto.Genre);

                if (!IsValid(dto) || !genreIsValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var movie = new Movie
                {
                    Title = dto.Title,
                    Director = dto.Director,
                    Genre = Enum.Parse<Genre>(dto.Genre),
                    Duration = TimeSpan.ParseExact(dto.Duration, "c", CultureInfo.InvariantCulture),
                    Rating = dto.Rating,
                };

                sb.AppendLine(String.Format(SuccessfulImportMovie, movie.Title, movie.Genre.ToString(), movie.Rating.ToString("F2")));
                movies.Add(movie);
            }

            context.Movies.AddRange(movies);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var hallDtos = JsonConvert.DeserializeObject<ImportHallDto[]>(jsonString);

            var halls = new List<Hall>();

            foreach (var dto in hallDtos)
            {
                var seatCountIsValid = dto.Seats > 0;

                if (!IsValid(dto) || !seatCountIsValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var seats = new List<Seat>();


                var hall = new Hall
                {
                    Name = dto.Name,
                    Is3D = dto.Is3D,
                    Is4Dx = dto.Is4Dx,
                };

                for (int i = 0; i < dto.Seats; i++)
                {
                    hall.Seats.Add(new Seat { Hall = hall });
                }

                var projection = "Normal";
                if (dto.Is3D == true && dto.Is4Dx == true)
                {
                    projection = "4Dx/3D";
                }
                else if (dto.Is3D == true && dto.Is4Dx == false)
                {
                    projection = "3D";
                }
                else if (dto.Is3D == false && dto.Is4Dx == true)
                {
                    projection = "4Dx";
                }

                sb.AppendLine(String.Format(SuccessfulImportHallSeat, hall.Name, projection, hall.Seats.Count));

                halls.Add(hall);
            }

            context.Halls.AddRange(halls);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            var sb = new StringBuilder();
            var projections = new List<Projection>();
            var movieIds = context.Movies.Select(m => m.Id).ToList();
            var hallIds = context.Halls.Select(m => m.Id).ToList();

            var serializer = new XmlSerializer(typeof(ImportProjectionDto[]), new XmlRootAttribute("Projections"));

            ImportProjectionDto[] importProjectionDtos;

            using (var reader = new StringReader(xmlString))
            {
                importProjectionDtos = (ImportProjectionDto[])serializer.Deserialize(reader);
            }

            foreach (var dto in importProjectionDtos)
            {
                var hallIsValid = hallIds.Contains(dto.HallId);
                var movieIsValid = movieIds.Contains(dto.MovieId);

                if (!IsValid(dto) || !hallIsValid || !movieIsValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var projection = new Projection
                {
                    Movie = context.Movies.Find(dto.MovieId),
                    Hall = context.Halls.Find(dto.HallId),
                    DateTime = DateTime.ParseExact(dto.DateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                };

                sb.AppendLine(String.Format(SuccessfulImportProjection, projection.Movie.Title, projection.DateTime.ToString("MM/dd/yyyy")));
                projections.Add(projection);
            }

            context.Projections.AddRange(projections);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            var sb = new StringBuilder();
            var customers = new List<Customer>();

            var serializer = new XmlSerializer(typeof(ImportCustomerDto[]), new XmlRootAttribute("Customers"));

            ImportCustomerDto[] importCustomerDtos;

            using (var reader = new StringReader(xmlString))
            {
                importCustomerDtos = (ImportCustomerDto[])serializer.Deserialize(reader);
            }

            foreach (var dto in importCustomerDtos)
            {

                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var customer = new Customer
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Age = dto.Age,
                    Balance = dto.Balance,
                    Tickets = dto.Tickets
                        .Select(t => new Ticket
                        {
                            ProjectionId = t.ProjectionId,
                            Price = t.Price,
                        })
                        .ToArray()
                };

                sb.AppendLine(String.Format(
                    SuccessfulImportCustomerTicket, customer.FirstName, customer.LastName, customer.Tickets.Count));

                customers.Add(customer);
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}