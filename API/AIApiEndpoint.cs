using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API
{
    class AIApiEndpoint
    {
        public static void setAIEndpoints(WebApplication app, Users db)
        {
            string AI_endpoint = "/AIask";
            string AI_model = "qwen3-4b-2507";
            string AI_server = "http://127.0.0.1:1234/v1/chat/completions";
            app.MapPost(AI_endpoint, async (AImessage message) =>
            {
                Console.WriteLine("Someone want to ask mr AI something");
                if (message == null)
                {
                    return Results.BadRequest("Empty message");
                }
                // time to get all the db...
                HttpClient client = new HttpClient();
                // { "role": "system", "content": "Always answer in rhymes. Today is Thursday" },
                // { "role": "user", "content": "What day is it today?" }
                var sysmsg = new
                {
                    role = "system",
                    content = "You are a database assistant and have to respond with ONLY the SQL queries, this is all the db scheme: " + db.GetDbScheme()// ottenere la struttura/schema del db
                };
                var usrmsg = new
                { 
                    role = "user",
                    content = message.Message
                };
                var msg_data = new
                {
                    model = AI_model,
                    messages = new[] {
                        sysmsg,
                        usrmsg
                    },
                };
                HttpResponseMessage response = await client.PostAsync(AI_server, new StringContent(JsonSerializer.Serialize(msg_data), Encoding.UTF8, "application/json"));

                // serializing the query
                try
                {
                    var raw_str = await response.Content.ReadAsStringAsync();
                    JsonDocument json = JsonDocument.Parse(raw_str);
                    JsonElement rootjson = json.RootElement;
                    string content = rootjson
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    var cmd = db.Database.GetDbConnection().CreateCommand(); // sending the query to the db
                    cmd.CommandText = content;
                    db.Database.OpenConnection();

                    var results = new List<Dictionary<string, object>>(); // getting the response in dict entries

                    if (content.TrimStart().StartsWith("INSERT"))
                    {
                        var writer = cmd.ExecuteNonQuery();
                        return Results.Ok("Query inserted succesfully!");
                    }
                    else
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read()) // read all the response
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var rawcolumname = reader.GetName(i);

                                var columnName = rawcolumname // polishing the response
                                    .Replace("(", "")
                                    .Replace(")", "")
                                    .Replace("*", "all")
                                    .Replace(" ", "_")
                                    .ToLowerInvariant();

                                var value = reader.IsDBNull(i) ? null : reader.GetValue(i); // null values can exists
                                row[columnName] = value;
                            }
                            results.Add(row);
                        }
                    }

                    db.Database.CloseConnection(); // close the runtime db
                    var results_string = JsonSerializer.Serialize(results);
                    if (message.Elaborate) // elaborate thinking
                    {
                        HttpClient client2 = new HttpClient();
                        var sysmsg2 = new
                        {
                            role = "system",
                            content = "You just have to humanize the json given by the user in a human readable phrase this is the original question: "+message.Message 
                        };
                        var usrmsg2 = new
                        {
                            role = "user",
                            content = results_string
                        };
                        var msg_data2 = new
                        {
                            model = AI_model,
                            messages = new[] {
                                sysmsg2,
                                usrmsg2
                            },
                        };
                        HttpResponseMessage response2 = await client.PostAsync(AI_server, new StringContent(JsonSerializer.Serialize(msg_data2), Encoding.UTF8, "application/json"));
                        var raw_str2 = await response2.Content.ReadAsStringAsync();
                        JsonDocument json2 = JsonDocument.Parse(raw_str2);
                        JsonElement rootjson2 = json2.RootElement;
                        string content2 = rootjson2
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

                        return Results.Ok(content2);
                    }
                    return Results.Ok(results_string);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Internal server error: {ex}");
                }
            })
            // .RequireAuthorization()
            .WithOpenApi();

        }

    }

} 