﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Capstone.Models;
using Capstone.Security;
using Capstone.Security.Models;

namespace Capstone.DAO
{
    public class UserSqlDao : IUserDao
    {
        private readonly string connectionString;

        public UserSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public User GetUser(string username)
        {
            User returnUser = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt, user_role, " +
                        "first_name, last_name, email_address, phone_number, street_address, city, state, zip " +
                        "FROM users WHERE username = @username", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnUser = CreateUserFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;
        }

        public User AddUser(string username, string password, string role, string firstName, string lastName, string emailAddress, string phoneNumber, string streetAddress, int city, int state, int zip)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            PasswordHash hash = passwordHasher.ComputeHash(password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO users (username, password_hash, salt, user_role, first_name, last_name, email_address, " +
                        "phone_number, street_address, city, state, zip) " +
                        "VALUES (@username, @password_hash, @salt, 'user', @first_name, @last_name, @email_address, @phone_number, @street_address, @city, @state," +
                        "@zip)", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", hash.Password);
                    cmd.Parameters.AddWithValue("@salt", hash.Salt);
                    cmd.Parameters.AddWithValue("@user_role", role);
                    cmd.Parameters.AddWithValue("@first_name", firstName);
                    cmd.Parameters.AddWithValue("@last_name", lastName);
                    cmd.Parameters.AddWithValue("@email_address", emailAddress);
                    cmd.Parameters.AddWithValue("@phone_number", phoneNumber);
                    cmd.Parameters.AddWithValue("@street_address", streetAddress);
                    cmd.Parameters.AddWithValue("@city", city);
                    cmd.Parameters.AddWithValue("@state", state);
                    cmd.Parameters.AddWithValue("@zip", zip);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return GetUser(username);
        }

        public DisplayUser DisplayUser(int userId) //display user info
        {
            DisplayUser user = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT u.user_id, u.username, u.password_hash, u.salt, u.user_role, u.first_name, u.last_name, u.email_address, u.phone_number, u.street_address, c.city_name, s.state_name, z.zipcode FROM users u JOIN states s ON s.state_id = u.state JOIN cities c ON c.city_id = u.city JOIN zips z ON z.zip_id = u.zip WHERE u.user_id = @user_id;";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user_id", userId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    user = CreateUserFromReaderForDisplay(reader);
                }

                return user;
            }
        }

        public bool DeleteUser(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("DELETE FROM users " +
                        "WHERE user_id = @user_id", conn);
                    cmd.Parameters.AddWithValue("@user_id", userId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    return (rowsAffected > 0);
                }
            }
            catch (SqlException)
            {

                throw;
            }
        }

        public bool UpdateUser(User updatedUser, int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE users " +
                        "SET first_name = @first_name, last_name = @last_name, email_address = @email_address, phone_number = @phone_number, " +
                        "street_address = @street_address, city = @city, state = @state, zip = @zip " +
                        "WHERE user_id = @user_id", conn);
                    cmd.Parameters.AddWithValue("@user_id", updatedUser.UserId);
                    cmd.Parameters.AddWithValue("@first_name", updatedUser.FirstName);
                    cmd.Parameters.AddWithValue("@last_name", updatedUser.LastName);
                    cmd.Parameters.AddWithValue("@email_address", updatedUser.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone_number", updatedUser.PhoneNumber);
                    cmd.Parameters.AddWithValue("@street_address", updatedUser.StreetAddress);
                    cmd.Parameters.AddWithValue("@city", updatedUser.City);
                    cmd.Parameters.AddWithValue("@state", updatedUser.State);
                    cmd.Parameters.AddWithValue("@zip", updatedUser.Zip);


                    int rowsAffected = cmd.ExecuteNonQuery();

                    return (rowsAffected > 0);
                }
            }
            catch (SqlException)
            {

                throw;
            }
        }
        public bool UpdateUserRole(User updatedUser, int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE users " +
                        "SET user_role = @user_role " +
                        "WHERE user_id = @user_id", conn);
                    cmd.Parameters.AddWithValue("@user_id", updatedUser.UserId);
                    cmd.Parameters.AddWithValue("@user_role", updatedUser.Role);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    return (rowsAffected > 0);
                }
            }
            catch (SqlException)
            {

                throw;
            }
        }
        public List<City> GetAllCities()
        {
            List<City> allCities = new List<City>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = "SELECT city_id, state_id, city_name FROM cities";

                    SqlCommand cmd = new SqlCommand(sql, conn);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                       City city = CreateCityFromReader(reader);
                        allCities.Add(city);
                    }
                }
            }
            catch (SqlException)
            {

                throw;
            }

            return allCities;
        }

        private User CreateUserFromReader(SqlDataReader reader)
        {
            User u = new User()
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                PasswordHash = Convert.ToString(reader["password_hash"]),
                Salt = Convert.ToString(reader["salt"]),
                Role = Convert.ToString(reader["user_role"]),
                FirstName = Convert.ToString(reader["first_name"]),
                LastName = Convert.ToString(reader["last_name"]),
                EmailAddress = Convert.ToString(reader["email_address"]),
                PhoneNumber = Convert.ToString(reader["phone_number"]),
                StreetAddress = Convert.ToString(reader["street_address"]),
                City = Convert.ToInt32(reader["city"]),
                State = Convert.ToInt32(reader["state"]),
                Zip = Convert.ToInt32(reader["zip"]),
            };

            return u;
        }

        //added for user display to not break things immediately; refactor later
        private DisplayUser CreateUserFromReaderForDisplay(SqlDataReader reader)
        {
            DisplayUser du = new DisplayUser()
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                PasswordHash = Convert.ToString(reader["password_hash"]),
                Salt = Convert.ToString(reader["salt"]),
                Role = Convert.ToString(reader["user_role"]),
                FirstName = Convert.ToString(reader["first_name"]),
                LastName = Convert.ToString(reader["last_name"]),
                EmailAddress = Convert.ToString(reader["email_address"]),
                PhoneNumber = Convert.ToString(reader["phone_number"]),
                StreetAddress = Convert.ToString(reader["street_address"]),
                City = Convert.ToString(reader["city_name"]),
                State = Convert.ToString(reader["state_name"]),
                Zip = Convert.ToInt32(reader["zipcode"]),
            };

            return du;
        }

        private City CreateCityFromReader(SqlDataReader reader)
        {
            City city= new City();

            city.cityId = Convert.ToInt32(reader["city_id"]);
            city.stateId = Convert.ToInt32(reader["state_id"]);
            city.cityName = Convert.ToString(reader["city_name"]);

            return city;
        }
    }
}
