using MySql.Data.MySqlClient;
using RestaurantBookingReport.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RestaurantBookingReport.Services
{
    public class RestaurantService
    {
        private readonly string _connectionString;

        public RestaurantService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<RestaurantReport> GetRestaurantReportAsync(int restaurantId, DateTime date)
        {
            var report = new RestaurantReport();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Fetch restaurant name
                var cmd = new MySqlCommand("SELECT name FROM restaurantbooking_restaurants WHERE id = @restaurantId", conn);
                cmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                report.RestaurantTitle = (await cmd.ExecuteScalarAsync())?.ToString();

                // Fetch total reservations and pax
                cmd = new MySqlCommand("SELECT COUNT(dt) as Total_res FROM restaurantbooking_bookings WHERE restaurant_id = @restaurantId AND status = 'Confirmed' AND DATE(created) = @date", conn);
                cmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                cmd.Parameters.AddWithValue("@date", date);
                var totalReservationsResult = await cmd.ExecuteScalarAsync();
                report.TotalReservations = totalReservationsResult != DBNull.Value ? Convert.ToInt32(totalReservationsResult) : 0;

                cmd = new MySqlCommand("SELECT SUM(people) as Total_people FROM restaurantbooking_bookings WHERE restaurant_id = @restaurantId AND status = 'Confirmed' AND DATE(created) = @date", conn);
                cmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                cmd.Parameters.AddWithValue("@date", date);
                var totalPaxResult = await cmd.ExecuteScalarAsync();
                report.TotalPax = totalPaxResult != DBNull.Value ? Convert.ToInt32(totalPaxResult) : 0;

                // Fetch seat times
                cmd = new MySqlCommand("SELECT DISTINCT TIME(dt) as times FROM restaurantbooking_bookings WHERE restaurant_id = @restaurantId AND status = 'Confirmed' AND DATE(created) = @date ORDER BY TIME(created) ASC", conn);
                cmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                cmd.Parameters.AddWithValue("@date", date);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        report.SeatData.Add(new SeatTimeData { Time = reader["times"].ToString() });
                    }
                }

                // Fetch seat data
                foreach (var seat in report.SeatData)
                {
                    cmd = new MySqlCommand("SELECT COUNT(created) as Total_res, SUM(people) as Total_people FROM restaurantbooking_bookings WHERE restaurant_id = @restaurantId AND TIME(created) = @time AND status = 'Confirmed' AND DATE(created) = @date", conn);
                    cmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                    cmd.Parameters.AddWithValue("@time", seat.Time);
                    cmd.Parameters.AddWithValue("@date", date);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            seat.TotalReservations = reader["Total_res"] != DBNull.Value ? Convert.ToInt32(reader["Total_res"]) : 0;
                            seat.TotalPeople = reader["Total_people"] != DBNull.Value ? Convert.ToInt32(reader["Total_people"]) : 0;
                        }
                    }

                    // Fetch individual bookings for each seat time
                    cmd = new MySqlCommand("SELECT DATE_FORMAT(created, '%d %b %y %h:%i:%s %p') as created, c_lname, c_room, people, c_events, c_vip, c_notes, c_kids FROM restaurantbooking_bookings WHERE restaurant_id = @restaurantId AND status = 'Confirmed' AND TIME(created) = @time AND DATE(created) = @date ORDER BY TIME(created)", conn);
                    cmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                    cmd.Parameters.AddWithValue("@time", seat.Time);
                    cmd.Parameters.AddWithValue("@date", date);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            seat.Bookings.Add(new Booking
                            {
                                Created = reader["created"].ToString(),
                                LastName = reader["c_lname"].ToString(),
                                Room = reader["c_room"].ToString(),
                                People = reader["people"] != DBNull.Value ? Convert.ToInt32(reader["people"]) : 0,
                                Events = reader["c_events"].ToString(),
                                VIP = reader["c_vip"].ToString(),
                                Notes = reader["c_notes"].ToString(),
                                Kids = reader["c_kids"] != DBNull.Value ? Convert.ToInt32(reader["c_kids"]) : 0
                            });
                        }
                    }
                }
            }

            return report;
        }
    }
}
