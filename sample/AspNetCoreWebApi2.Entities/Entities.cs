﻿using System.Text.Json.Serialization;
using LaDeak.JsonMergePatch.Abstractions;

namespace AspNetCoreWebApi2.Entities;

[Patchable]
public class CitiesData
{
    public Dictionary<string, string> Cities { get; set; }
}

[Patchable]
public class WeatherForecast
{
    public DateTime Date { get; set; }

    [JsonPropertyName("temp")]
    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string Summary { get; set; }
}

[Patchable]
public class DeviceData
{
    public double Watts { get; set; }
    public string Name { get; set; }
}

