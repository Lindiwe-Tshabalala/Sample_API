using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

public class Program
{
    static void Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/api/");
        listener.Start();
        Console.WriteLine("API is running on http://localhost:5000/api/");

        var userController = new UserController();

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string path = request.Url.AbsolutePath.TrimEnd('/');
            string method = request.HttpMethod;

            if (path == "/api/user" && method == "GET")
            {
                userController.HandleGetUsers(response);
            }
            else if (path == "/api/user" && method == "POST")
            {
                userController.HandleCreateUser(request, response);
            }
            else
            {
                response.StatusCode = 404;
                byte[] errorBuffer = System.Text.Encoding.UTF8.GetBytes("{\"error\": \"Not found\"}");
                response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
            }

            response.OutputStream.Close();
        }
    }
}

public class UserController
{
    private static List<User> Users = new List<User>
    {
        new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
        new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
    };

    public void HandleGetUsers(HttpListenerResponse response)
    {
        string json = JsonSerializer.Serialize(Users);
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);

        response.ContentType = "application/json";
        response.StatusCode = 200;
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    public void HandleCreateUser(HttpListenerRequest request, HttpListenerResponse response)
    {
        using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
        {
            string body = reader.ReadToEnd();
            User newUser = JsonSerializer.Deserialize<User>(body);

            newUser.Id = Users.Count + 1;
            Users.Add(newUser);

            string json = JsonSerializer.Serialize(newUser);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);

            response.ContentType = "application/json";
            response.StatusCode = 201;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
